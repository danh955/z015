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
        private const int TickDelay = 1; // minutes.

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

            this.lastMarketClosed = GetLastMarketClosedTime(EasternTimeNow);
            this.nextMarketClosed = this.lastMarketClosed;
        }

        private static DateTimeOffset EasternTimeNow => TimeZoneInfo.ConvertTime(DateTimeOffset.Now, Constant.EasternTimeZone);

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(5 * 1000, cancellationToken).ConfigureAwait(false);
                this.logger.LogInformation("Starting {0}.{1}", nameof(BackgroundTaskService), nameof(this.ExecuteAsync));

                bool canUpdateStockPrices = false;

                while (!cancellationToken.IsCancellationRequested)
                {
                    if (await this.IsTimeToProcessAsync(StockFrequency.Monthly, cancellationToken).ConfigureAwait(false))
                    {
                        await this.updateStockSymbol.DoUpdateFromTiingoAsync(cancellationToken).ConfigureAwait(false);
                        canUpdateStockPrices = true;
                    }

                    if (canUpdateStockPrices)
                    {
                        bool hasFinished = await this.updateStockPrices.DoUpdateAsync(
                                                        frequency: StockFrequency.Monthly,
                                                        firstDate: new(2000, 1, 1),
                                                        cutOffDate: this.lastMarketClosed,
                                                        takeCount: int.MaxValue,
                                                        cancellationToken: cancellationToken).ConfigureAwait(false);
                        canUpdateStockPrices = !hasFinished;
                    }

                    this.logger.LogInformation("Tick");
                    await Task.Delay(TickDelay * 60 * 1000, cancellationToken).ConfigureAwait(false);
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
        /// Get the last time the market has closed.
        /// </summary>
        /// <param name="date">DateTimeOffset.</param>
        /// <returns>The last time the market has closed.</returns>
        private static DateTimeOffset GetLastMarketClosedTime(DateTimeOffset date)
        {
            DateTimeOffset easternTime = TimeZoneInfo.ConvertTime(date, Constant.EasternTimeZone);
            return new DateTimeOffset(easternTime.Subtract(Constant.MarketClosedTime).Date, easternTime.Offset)
                .Add(Constant.MarketClosedTime);
        }

        /// <summary>
        /// Check if its time to processing.
        /// </summary>
        /// <returns>True if its time to process.</returns>
        private async Task<bool> IsTimeToProcessAsync(StockFrequency frequency, CancellationToken cancellationToken)
        {
            DateTimeOffset currentEasternTime = EasternTimeNow;

            if (currentEasternTime < this.nextMarketClosed.AddMinutes(30))
            {
                return false;
            }

            this.lastMarketClosed = GetLastMarketClosedTime(currentEasternTime);
            bool isReady = await this.updateStockPrices.ReadyToUpdate(frequency, this.lastMarketClosed, cancellationToken).ConfigureAwait(false);
            if (!isReady)
            {
                return false;
            }

            this.nextMarketClosed = this.lastMarketClosed.AddDays(1);
            this.logger.LogInformation("currentEasternTime = {0}, nextMarketClosed = {1} EST", currentEasternTime, this.nextMarketClosed);

            return true;
        }
    }
}