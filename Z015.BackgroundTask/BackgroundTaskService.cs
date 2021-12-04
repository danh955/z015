// <copyright file="BackgroundTaskService.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.BackgroundTask
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Z015.Repository;

    /// <summary>
    /// Background task service class.
    /// </summary>
    public class BackgroundTaskService : BackgroundService
    {
        private const StockFrequency DefaultStockFrequency = StockFrequency.Monthly;
        private const int DelayAfterCloseMinutes = 60;
        private const int FirstYearOfData = 2000;
        private readonly IDbContextFactory<RepositoryDbContext> dbFactory;
        private readonly HttpClient keepAliveHttpClient;
        private readonly ILogger<BackgroundTaskService> logger;
        private readonly UpdateStockPrices updateStockPrices;
        private readonly UpdateStockSymbol updateStockSymbol;
        private DateTimeOffset lastMarketClosed;
        private DateTimeOffset nextMarketClosed;

        private BackgroundTaskOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundTaskService"/> class.
        /// </summary>
        /// <param name="optionMonitor">BackgroundTaskOptions.</param>
        /// <param name="updateStockSymbol">UpdateStockSymbol.</param>
        /// <param name="updateStockPrices">UpdateStockPrices.</param>
        /// <param name="dbFactory">IDbContextFactory for RepositoryDbContext.</param>
        /// <param name="logger">ILogger.</param>
        public BackgroundTaskService(
            IOptionsMonitor<BackgroundTaskOptions> optionMonitor,
            UpdateStockSymbol updateStockSymbol,
            UpdateStockPrices updateStockPrices,
            IDbContextFactory<RepositoryDbContext> dbFactory,
            ILogger<BackgroundTaskService> logger)
        {
            this.updateStockSymbol = updateStockSymbol;
            this.updateStockPrices = updateStockPrices;
            this.dbFactory = dbFactory;
            this.logger = logger;
            this.keepAliveHttpClient = new();

            this.SetBackgroundTaskOptions(optionMonitor.CurrentValue);
            optionMonitor.OnChange(newValue => this.SetBackgroundTaskOptions(newValue));
        }

        private static DateTimeOffset EasternTimeNow => TimeZoneInfo.ConvertTime(DateTimeOffset.Now, Constant.EasternTimeZone);

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(5 * 1000, cancellationToken).ConfigureAwait(false);
                this.logger.LogInformation("Starting {Class}.{Function}", nameof(BackgroundTaskService), nameof(this.ExecuteAsync));

                bool canUpdateStockSymbols = false;
                bool canUpdateStockPrices = false;

                while (!cancellationToken.IsCancellationRequested)
                {
                    if (this.SetLastNextMarketClosedTime())
                    {
                        canUpdateStockSymbols = true;
                        canUpdateStockPrices = true;
                    }

                    if (canUpdateStockSymbols)
                    {
                        await this.updateStockSymbol.DoUpdateFromTiingoAsync(this.lastMarketClosed, cancellationToken).ConfigureAwait(false);
                        canUpdateStockSymbols = false;
                    }

                    if (canUpdateStockPrices)
                    {
                        bool hasFinished = await this.updateStockPrices.DoUpdateAsync(
                                                        frequency: DefaultStockFrequency,
                                                        firstDate: new(FirstYearOfData, 1, 1),
                                                        cutOffDate: this.lastMarketClosed,
                                                        takeCount: int.MaxValue,
                                                        cancellationToken: cancellationToken).ConfigureAwait(false);
                        canUpdateStockPrices = !hasFinished;
                    }

                    var tickDelay = Math.Max(this.options.TickDelayMinutes ?? 5, 1);
                    this.logger.LogInformation("Tick ({TickDelayMinutes})", tickDelay);

                    await this.KeepAliveAsync(canUpdateStockSymbols || canUpdateStockPrices);
                    await Task.Delay(tickDelay * 60 * 1000, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException ex)
            {
                this.logger.LogInformation("{Class}.{Function} TaskCanceledException: {ErrorMessage}", nameof(BackgroundTaskService), nameof(this.ExecuteAsync), ex.Message);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error {Class}.{Function}", nameof(BackgroundTaskService), nameof(this.ExecuteAsync));
            }
            finally
            {
                this.logger.LogInformation("Ending {Class}.{Function}", nameof(BackgroundTaskService), nameof(this.ExecuteAsync));
            }
        }

        /// <summary>
        /// This is used to keep asp.net running while the background task is working.
        /// </summary>
        /// <param name="doPing">Ping ourselves to keep working.  False if not doing any work.</param>
        /// <returns>Task.</returns>
        private async Task KeepAliveAsync(bool doPing)
        {
            if (!doPing || string.IsNullOrWhiteSpace(this.options?.KeepAliveUrl))
            {
                return;
            }

            try
            {
                this.logger.LogInformation("Keeping Alive {URL}", this.options.KeepAliveUrl);
                UriBuilder uriBuilder = new(this.options.KeepAliveUrl);
                _ = await this.keepAliveHttpClient.GetAsync(uriBuilder.Uri);
            }
            catch (Exception ex)
            {
                this.logger.LogWarning("{Function}({KeepAliveUrl}) Error: {ErrorMessage}.", nameof(this.KeepAliveAsync), this.options.KeepAliveUrl, ex.Message);
            }
        }

        private void SetBackgroundTaskOptions(BackgroundTaskOptions newValue)
        {
            this.options = newValue;
            this.updateStockPrices.YahooRequestDelay = newValue.YahooRequestDelay ?? 250;

            this.logger.LogInformation("BackgroundTaskOptions  YahooRequestDelay={YahooRequestDelay}, TickDelayMinutes={TickDelayMinutes}, KeepAliveUrl={KeepAliveUrl}", newValue.YahooRequestDelay, newValue.TickDelayMinutes, newValue.KeepAliveUrl);
        }

        /// <summary>
        /// This will set the lastMarketClosed and nextMarketClosed based on the DefaultStockFrequency.
        /// It will only change if the nextMarketClosed is in the past.
        /// Date and time are sets to the eastern time zone.
        /// </summary>
        /// <returns>True if lastMarketClosed and nextMarketClosed has been changed.</returns>
        private bool SetLastNextMarketClosedTime()
        {
            DateTimeOffset easternTime = EasternTimeNow;
            if (this.nextMarketClosed.AddMinutes(DelayAfterCloseMinutes) > easternTime)
            {
                return false;
            }

            switch (DefaultStockFrequency)
            {
                case StockFrequency.Monthly:
                    {
                        // The monthly market closes at the end of the last day of the month.
                        var lastDayOfMonth = easternTime.AddDays(1 - easternTime.Day).AddMonths(1).AddDays(-1);
                        var closedTime = new DateTimeOffset(lastDayOfMonth.Date, lastDayOfMonth.Offset).Add(Constant.MarketClosedTime);
                        this.lastMarketClosed = easternTime < closedTime ? closedTime.AddDays(1).AddMonths(-1).AddDays(-1) : closedTime;
                        this.nextMarketClosed = this.lastMarketClosed.AddDays(1).AddMonths(1).AddDays(-1);
                        break;
                    }

                default:
                    throw new NotImplementedException();
            }

            this.logger.LogInformation("lastMarketClosed = {LastMarketClosed}, nextMarketClosed = {NextMarketClosed}", this.lastMarketClosed, this.nextMarketClosed);
            return true;
        }

        //// <summary>
        //// This get the last market closed time for this day.  Its always a date and time before the eastern time parameter.
        //// </summary>
        //// <param name="easternTime">DateTimeOffset.</param>
        //// <returns>DateTimeOffset of when the market is closed.</returns>
        ////private static DateTimeOffset GetLastMarketClosed(DateTimeOffset easternTime) =>
        ////            new DateTimeOffset(easternTime.Subtract(Constant.MarketClosedTime).Date, easternTime.Offset).Add(Constant.MarketClosedTime);
    }
}