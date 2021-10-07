// <copyright file="UpdateStockPricesOptions.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.BackgroundTask
{
    using System;
    using System.Threading;
    using Z015.Repository;

    /// <summary>
    /// Update stock prices options class.
    /// </summary>
    internal class UpdateStockPricesOptions
    {
        /// <summary>
        /// Gets or sets the symbol to update.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Gets or sets the frequency of the stock price.
        /// </summary>
        public StockFrequency Frequency { get; set; }

        /// <summary>
        /// Gets or sets the first date of stock prices to get.  Null for max.
        /// </summary>
        public DateTime? FirstDate { get; set; }

        /// <summary>
        /// Gets or sets the last date of stock prices to get.  Null for today.
        /// </summary>
        public DateTime? LastDate { get; set; }

        /// <summary>
        /// Gets or sets the cancellation token.
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{this.Symbol}, {this.Frequency}, {this.FirstDate}, {this.LastDate}";
        }
    }
}