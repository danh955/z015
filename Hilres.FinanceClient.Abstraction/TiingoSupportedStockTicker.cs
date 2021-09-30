// <copyright file="TiingoSupportedStockTicker.cs" company="None">
// Free and open source code.
// </copyright>

namespace Hilres.FinanceClient.Abstraction
{
    using System;

    /// <summary>
    /// Tiingo stock ticker item.
    /// </summary>
    /// <param name="Ticker">Ticker symbol.</param>
    /// <param name="Exchange">Exchange.</param>
    /// <param name="AssetType">Asset type.</param>
    /// <param name="PriceCurrency">Price currency.</param>
    /// <param name="StartDate">Start date.</param>
    /// <param name="EndDate">End date.</param>
    public record TiingoSupportedStockTicker(string Ticker, string Exchange, string AssetType, string PriceCurrency, DateTime? StartDate, DateTime? EndDate);
}