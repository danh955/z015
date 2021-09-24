// <copyright file="YahooService.cs" company="None">
// Free and open source code.
// </copyright>

namespace Hilres.FinanceClient.Yahoo
{
    using System;
    using System.Net.Http;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Yahoo finance service class.
    /// </summary>
    public partial class YahooService
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<YahooService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="YahooService"/> class.
        /// </summary>
        /// <param name="logger">ILogger.</param>
        public YahooService(ILogger<YahooService> logger)
        {
            this.logger = logger;
            this.httpClient = new();
        }

        /// <summary>
        /// Convert the interval into a string.
        /// </summary>
        /// <param name="interval">StockInterval.</param>
        /// <returns>string.</returns>
        public static string ToIntervalString(YahooInterval? interval) => interval switch
        {
            null => "1d",
            YahooInterval.Daily => "1d",
            YahooInterval.Weekly => "1wk",
            YahooInterval.Monthly => "1mo",
            YahooInterval.Quorterly => "3mo",
            _ => throw new NotImplementedException(interval.ToString()),
        };
    }
}