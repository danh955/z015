// <copyright file="HelperExtensions.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.BackgroundTask
{
    using System;
    using Hilres.FinanceClient.Abstraction;
    using Z015.Repository;

    /// <summary>
    /// For parsing data.
    /// </summary>
    public static class HelperExtensions
    {
        /// <summary>
        /// Convert stock frequency to Yahoo interval value.
        /// </summary>
        /// <param name="frequency">StockFrequency.</param>
        /// <returns>YahooInterval.</returns>
        public static YahooInterval ToYahooInterval(this StockFrequency frequency) => frequency switch
        {
            StockFrequency.Daily => YahooInterval.Daily,
            StockFrequency.Weekly => YahooInterval.Weekly,
            StockFrequency.Monthly => YahooInterval.Monthly,
            StockFrequency.Quarterly => YahooInterval.Quorterly,
            _ => throw new NotImplementedException(frequency.ToString()),
        };
    }
}