// <copyright file="ITiingoService.cs" company="None">
// Free and open source code.
// </copyright>

namespace Hilres.FinanceClient.Abstraction
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Tiingo service interface.
    /// </summary>
    public interface ITiingoService
    {
        /// <summary>
        /// Get supported tickers.
        /// </summary>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>List of supported stock tickers.  Successful if error message is null.</returns>
        public Task<(IReadOnlyList<TiingoSupportedStockTicker> Stocks, string ErrorMessage)> GetSupportedTickersAsync(CancellationToken cancellationToken);
    }
}