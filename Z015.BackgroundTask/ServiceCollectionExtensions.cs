// <copyright file="ServiceCollectionExtensions.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.BackgroundTask
{
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Service collection extensions class.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add the background task service to the service collection.
        /// </summary>
        /// <param name="service">IServiceCollection.</param>
        /// <returns>Updated IServiceCollection.</returns>
        public static IServiceCollection AddBackgroundTaskService(this IServiceCollection service)
        {
            service.AddSingleton<UpdateStockPrices>();
            service.AddHostedService<BackgroundTaskService>();
            return service;
        }
    }
}