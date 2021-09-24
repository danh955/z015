// <copyright file="YahooInterval.cs" company="None">
// Free and open source code.
// </copyright>

namespace Hilres.FinanceClient.Yahoo
{
    /// <summary>
    /// Yahoo stock price interval.
    /// </summary>
    public enum YahooInterval
    {
        /// <summary>
        /// Daily stock prices.  (1d).
        /// </summary>
        Daily,

        /// <summary>
        /// Weekly stock prices.  (1w).
        /// </summary>
        Weekly,

        /// <summary>
        /// Monthly stock prices.  (1m).
        /// </summary>
        Monthly,

        /// <summary>
        /// Quarterly stock prices.  (3m).
        /// </summary>
        Quorterly,
    }
}