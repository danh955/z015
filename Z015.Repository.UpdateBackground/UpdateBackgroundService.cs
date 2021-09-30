// <copyright file="UpdateBackgroundService.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.Repository.UpdateBackground
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Update background service class.
    /// </summary>
    public class UpdateBackgroundService : BackgroundService
    {
        private readonly ILogger<UpdateBackgroundService> logger;
        private readonly IServiceProvider services;
        private DateTime nextMarketClosed = DateTime.MinValue;  // EST.

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateBackgroundService"/> class.
        /// </summary>
        /// <param name="services">IServiceProvider.</param>
        /// <param name="logger">ILogger.</param>
        public UpdateBackgroundService(IServiceProvider services, ILogger<UpdateBackgroundService> logger)
        {
            this.services = services;
            this.logger = logger;
        }

        private static DateTime EasternTimeNow => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Constant.EasternTimeZone);

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(2000, cancellationToken);
            this.logger.LogInformation("Start of {0}.{1}", nameof(UpdateBackgroundService), nameof(this.ExecuteAsync));

            while (!cancellationToken.IsCancellationRequested)
            {
                await this.UpdateSymbolList(cancellationToken);

                //// TODO: Get list of stock symbol to update prices.  Any that need update that is less then the currentEasternTime.

                await Task.Delay(1000, cancellationToken);
                this.logger.LogInformation("Tick");
            }

            this.logger.LogInformation("End of {0}.{1}", nameof(UpdateBackgroundService), nameof(this.ExecuteAsync));
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

        private async Task UpdateSymbolList(CancellationToken cancellationToken)
        {
            DateTime currentEasternTime = EasternTimeNow;

            // Is it time to update the symbol list?
            if (currentEasternTime > this.nextMarketClosed.AddMinutes(30))
            {
                var lastMarketClosed = GetLastMarketClosedTime(currentEasternTime);
                this.logger.LogInformation("currentEasternTime = {0}, lastMarketClosed = {1}", currentEasternTime, lastMarketClosed);

                using (var scope = this.services.CreateScope())
                {
                    var scopedService = scope.ServiceProvider.GetRequiredService<UpdateBackgroundSymbolsService>();
                    await scopedService.DoUpdateFromTiingo(cancellationToken);
                }

                // Set the next market closed time to the next day.
                this.nextMarketClosed = lastMarketClosed.AddDays(1);  // EST.
                this.logger.LogInformation("currentEasternTime = {0}, nextMarketClosed = {1}", currentEasternTime, this.nextMarketClosed);
            }
        }
    }
}