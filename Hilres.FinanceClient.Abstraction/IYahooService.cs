// <copyright file="IYahooService.cs" company="None">
// Free and open source code.
// </copyright>

namespace Hilres.FinanceClient.Abstraction
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Yahoo service interface.
    /// </summary>
    public interface IYahooService
    {
        /// <summary>
        /// Get stock history data from Yahoo.
        /// </summary>
        /// <param name="symbol">Symbol of prices to get.</param>
        /// <param name="firstDate">First date.</param>
        /// <param name="lastDate">Last date.</param>
        /// <param name="interval">Stock price interval.</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>Task with PriceListResult.</returns>
        public Task<(IReadOnlyList<YahooPrice> Prices, string ErrorMessage)> GetStockPricesAsync(string symbol, DateTime? firstDate, DateTime? lastDate, YahooInterval? interval, CancellationToken cancellationToken);
    }
}