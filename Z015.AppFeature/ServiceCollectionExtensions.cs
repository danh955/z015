// <copyright file="ServiceCollectionExtensions.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.AppFeature
{
    using Microsoft.Extensions.DependencyInjection;
    using Z015.AppFeature.LongTrendPage;
    using Z015.AppFeature.PortfolioPage;

    /// <summary>
    /// Service collection extensions class.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add application feature service collection.
        /// </summary>
        /// <param name="service">IServiceCollection.</param>
        /// <returns>Updated IServiceCollection.</returns>
        public static IServiceCollection AddAppFeatureService(this IServiceCollection service)
        {
            service.AddTransient<LongTrendListService>();
            service.AddTransient<OpenPositionListService>();
            return service;
        }
    }
}
