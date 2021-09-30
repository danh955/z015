// <copyright file="ServiceCollectionExtensions.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.Repository.UpdateBackground
{
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Service collection extensions class.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add the stock database updater to the service collection.
        /// </summary>
        /// <param name="service">IServiceCollection.</param>
        /// <returns>Updated IServiceCollection.</returns>
        public static IServiceCollection AddUpdateBackgroundService(this IServiceCollection service)
        {
            service.AddTransient<UpdateBackgroundSymbolsService>();
            service.AddHostedService<UpdateBackgroundService>();
            return service;
        }
    }
}