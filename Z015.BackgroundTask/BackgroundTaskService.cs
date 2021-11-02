// <copyright file="BackgroundTaskService.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.BackgroundTask
{
    using System;
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
        private const int FirstYearOfData = 2000;
        private const int DelayAfterCloseMinutes = 30;
        private const int TickDelayMinutes = 5; // minutes.

        private readonly IDbContextFactory<RepositoryDbContext> dbFactory;
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
            this.options = optionMonitor.CurrentValue;
            this.updateStockSymbol = updateStockSymbol;
            this.updateStockPrices = updateStockPrices;
            this.dbFactory = dbFactory;
            this.logger = logger;

            optionMonitor.OnChange(newValue =>
            {
                this.options = newValue;
            });
        }

        private static DateTimeOffset EasternTimeNow => TimeZoneInfo.ConvertTime(DateTimeOffset.Now, Constant.EasternTimeZone);

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(5 * 1000, cancellationToken).ConfigureAwait(false);
                this.logger.LogInformation("Starting {0}.{1}", nameof(BackgroundTaskService), nameof(this.ExecuteAsync));

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

                    this.logger.LogInformation("Tick");
                    await Task.Delay(TickDelayMinutes * 60 * 1000, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Error {0}.{1}", nameof(BackgroundTaskService), nameof(this.ExecuteAsync));
            }
            finally
            {
                this.logger.LogInformation("Ending {0}.{1}", nameof(BackgroundTaskService), nameof(this.ExecuteAsync));
            }
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
                        var firstDayOfMonth = easternTime.AddDays(0 - easternTime.Day + 1);
                        this.lastMarketClosed = GetLastMarketClosed(firstDayOfMonth);
                        this.nextMarketClosed = this.lastMarketClosed.AddMonths(1);
                        break;
                    }

                default:
                    throw new NotImplementedException();
            }

            this.logger.LogInformation("lastMarketClosed = {0}, nextMarketClosed = {1}", this.lastMarketClosed, this.nextMarketClosed);

            return true;

            static DateTimeOffset GetLastMarketClosed(DateTimeOffset easternTime)
            {
                return new DateTimeOffset(easternTime.Subtract(Constant.MarketClosedTime).Date, easternTime.Offset)
                    .Add(Constant.MarketClosedTime);
            }
        }
    }
}