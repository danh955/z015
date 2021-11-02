// <copyright file="StockEntity.cs" company="None">
// Free and open source code.
// </copyright>
namespace Z015.Repository
{
    using System.Collections.Generic;

    /// <summary>
    /// Stock class.
    /// </summary>
    public class StockEntity
    {
        /// <summary>
        /// Gets or sets the stock ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///  Gets or sets the stock symbol.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Gets or sets the stock name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the listing stock exchange or market of the security.
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        /// Gets or sets the asset type.
        /// </summary>
        public string AssetType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the symbol was not found.
        /// </summary>
        public bool IsSymbolNotFound { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is to be deleted.
        /// </summary>
        public bool ToBeDeleted { get; set; }

        /// <summary>
        /// Gets or sets list of stock prices.
        /// </summary>
        public List<StockPriceEntity> StockPrices { get; set; }

        /// <summary>
        /// Gets or sets list of stock price last updates.
        /// </summary>
        public List<StockPriceLastUpdatedEntity> StockPriceLastUpdates { get; set; }
    }
}