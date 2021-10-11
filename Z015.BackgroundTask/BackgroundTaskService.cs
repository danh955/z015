// <copyright file="BackgroundTaskService.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.BackgroundTask
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
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

        private readonly ILogger<BackgroundTaskService> logger;
        private readonly UpdateStockPrices updateStockPrices;
        private readonly UpdateStockSymbol updateStockSymbol;
        private DateTime nextMarketClosed = DateTime.MinValue; // EST.

        private BackgroundTaskOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundTaskService"/> class.
        /// </summary>
        /// <param name="optionMonitor">BackgroundTaskOptions.</param>
        /// <param name="updateStockSymbol">UpdateStockSymbol.</param>
        /// <param name="updateStockPrices">UpdateStockPrices.</param>
        /// <param name="logger">ILogger.</param>
        public BackgroundTaskService(
            IOptionsMonitor<BackgroundTaskOptions> optionMonitor,
            UpdateStockSymbol updateStockSymbol,
            UpdateStockPrices updateStockPrices,
            ILogger<BackgroundTaskService> logger)
        {
            this.options = optionMonitor.CurrentValue;
            this.updateStockSymbol = updateStockSymbol;
            this.updateStockPrices = updateStockPrices;
            this.logger = logger;

            optionMonitor.OnChange(newValue =>
            {
                this.options = newValue;
            });
        }

        private static DateTime EasternTimeNow => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Constant.EasternTimeZone);

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(5 * 1000, cancellationToken);
            this.logger.LogInformation("Starting {0}.{1}", nameof(BackgroundTaskService), nameof(this.ExecuteAsync));

            bool canUpdateStockPrices = false;

            while (!cancellationToken.IsCancellationRequested)
            {
                if (this.IsTimeToProcess())
                {
                    await this.updateStockSymbol.DoUpdateFromTiingo(cancellationToken);
                    canUpdateStockPrices = true;
                }

                if (canUpdateStockPrices)
                {
                    bool hasStarted = await this.updateStockPrices.DoUpdateAsync(StockFrequency.Monthly, new(2020, 1, 1), cancellationToken).ConfigureAwait(false);
                    canUpdateStockPrices = !hasStarted;
                }

                this.logger.LogInformation("Tick  count: {0:#,##0}", this.updateStockPrices.InputCount);
                await Task.Delay(TickDelay * 60 * 1000, cancellationToken);
            }

            this.logger.LogInformation("Ending {0}.{1}", nameof(BackgroundTaskService), nameof(this.ExecuteAsync));
        }

        /// <summary>
        /// Get the market closed time.
        /// </summary>
        /// <param name="currentEasternTime">DateTime.</param>
        /// <returns>The last time the market closed.</returns>
        private static DateTime GetLastMarketClosedTime(DateTime currentEasternTime)
        {
            return (currentEasternTime - Constant.MarketClosedTime).Date + Constant.MarketClosedTime;
        }

        /// <summary>
        /// Check if its time to processing.
        /// </summary>
        /// <returns>True if its time to process.</returns>
        private bool IsTimeToProcess()
        {
            DateTime currentEasternTime = EasternTimeNow;

            // Is it time to do updates?
            if (currentEasternTime < this.nextMarketClosed.AddMinutes(30))
            {
                return false;
            }

            var lastMarketClosed = GetLastMarketClosedTime(currentEasternTime);

            // Set the next market closed time to the next day.
            this.nextMarketClosed = lastMarketClosed.AddDays(1);  // EST.
            this.logger.LogInformation("currentEasternTime = {0}, nextMarketClosed = {1} EST", currentEasternTime, this.nextMarketClosed);

            return true;
        }
    }
}