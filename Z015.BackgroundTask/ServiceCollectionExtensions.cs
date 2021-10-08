// <copyright file="ServiceCollectionExtensions.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.BackgroundTask
{
    using Hilres.FinanceClient.Tiingo;
    using Hilres.FinanceClient.Yahoo;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Service collection extensions class.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add the background task service to the service collection.
        /// </summary>
        /// <param name="services">IServiceCollection.</param>
        /// <returns>Updated IServiceCollection.</returns>
        public static IServiceCollection AddBackgroundTaskService(this IServiceCollection services)
        {
            services.AddTiingoService();
            services.AddYahooService();
            services.AddSingleton<UpdateStockSymbol>();
            services.AddSingleton<UpdateStockPrices>();
            services.AddHostedService<BackgroundTaskService>();
            return services;
        }
    }
}