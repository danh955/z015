// <copyright file="StockPriceLastUpdatedEntity.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.Repository
{
    using System;

    /// <summary>
    /// Stock price last updated entity class.
    /// </summary>
    public class StockPriceLastUpdatedEntity
    {
        /// <summary>
        /// Gets or sets the stock last updated ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the stock ID.
        /// </summary>
        public int StockId { get; set; }

        /// <summary>
        /// Gets or sets frequency of stock.
        /// </summary>
        public StockFrequency Frequency { get; set; }

        /// <summary>
        /// Gets or sets the last date the price was updated.
        /// </summary>
        public DateTimeOffset? PriceUpdatedDate { get; set; }

        /// <summary>
        /// Gets or sets stock.
        /// </summary>
        public StockEntity Stock { get; set; }
    }
}