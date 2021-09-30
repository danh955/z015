// <copyright file="StockPriceEntity.cs" company="None">
// Free and open source code.
// </copyright>
namespace Z015.Repository
{
    using System;

    /// <summary>
    /// Stock price entity class.
    /// </summary>
    public class StockPriceEntity
    {
        /// <summary>
        /// Gets or sets the stock price ID.
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
        /// Gets or sets the date Period.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the open price.
        /// </summary>
        public double Open { get; set; }

        /// <summary>
        /// Gets or sets the low price.
        /// </summary>
        public double Low { get; set; }

        /// <summary>
        /// Gets or sets the high price.
        /// </summary>
        public double High { get; set; }

        /// <summary>
        /// Gets or sets the close price.
        /// </summary>
        public double Close { get; set; }

        /// <summary>
        /// Gets or sets the volume.
        /// </summary>
        public double Volume { get; set; }

        /// <summary>
        /// Gets or sets stock.
        /// </summary>
        public StockEntity Stock { get; set; }
    }
}