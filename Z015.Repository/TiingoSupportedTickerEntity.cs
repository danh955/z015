// <copyright file="TiingoSupportedTickerEntity.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.Repository
{
    using System;

    /// <summary>
    /// Tiingo supported stock ticker entity class.
    /// </summary>
    public class TiingoSupportedTickerEntity
    {
        /// <summary>
        /// Gets or sets the stock price ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ticker symbol.
        /// </summary>
        public string Ticker { get; set; }

        /// <summary>
        /// Gets or sets the exchange.
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        /// Gets or sets the type of asset.
        /// </summary>
        public string AssetType { get; set; }

        /// <summary>
        /// Gets or sets the currency the price is using.
        /// </summary>
        public string PriceCurrency { get; set; }

        /// <summary>
        /// Gets or sets the start date.  This is the first date the stock is available.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.  This is the last date the stock is available.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the date the entity was added.
        /// </summary>
        public DateTime DateAdded { get; set; }

        /// <summary>
        /// Gets or sets the date the entity was updated.
        /// </summary>
        public DateTime DateUpdated { get; set; }
    }
}