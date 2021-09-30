// <copyright file="ServiceCollectionExtensions.cs" company="None">
// Free and open source code.
// </copyright>

namespace Hilres.FinanceClient.Yahoo
{
    using Hilres.FinanceClient.Abstraction;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Service collection extensions class.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add the Yahoo service to the service collection.
        /// </summary>
        /// <param name="service">IServiceCollection.</param>
        /// <returns>Updated IServiceCollection.</returns>
        public static IServiceCollection AddYahooService(this IServiceCollection service)
        {
            service.AddSingleton<IYahooService, YahooService>();
            return service;
        }
    }
}