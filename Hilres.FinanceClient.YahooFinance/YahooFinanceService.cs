// <copyright file="YahooFinanceService.cs" company="None">
// Free and open source code.
// </copyright>

namespace Hilres.FinanceClient.YahooFinance
{
    using System;
    using System.Net.Http;
    using Hilres.FinanceClient.Abstraction;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Yahoo finance service class.
    /// </summary>
    public partial class YahooFinanceService
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<YahooFinanceService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="YahooFinanceService"/> class.
        /// </summary>
        /// <param name="logger">ILogger.</param>
        public YahooFinanceService(ILogger<YahooFinanceService> logger)
        {
            this.logger = logger;
            this.httpClient = new();
        }

        /// <summary>
        /// Convert the interval into a string.
        /// </summary>
        /// <param name="interval">StockInterval.</param>
        /// <returns>string.</returns>
        public static string ToIntervalString(StockInterval? interval) => interval switch
        {
            null => "1d",
            StockInterval.Daily => "1d",
            StockInterval.Weekly => "1wk",
            StockInterval.Monthly => "1mo",
            StockInterval.Quorterly => "3mo",
            _ => throw new NotImplementedException(interval.ToString()),
        };
    }
}