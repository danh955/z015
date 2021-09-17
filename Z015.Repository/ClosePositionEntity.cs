﻿// <copyright file="ClosePositionEntity.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.Repository
{
    using System;

    /// <summary>
    /// Close stock position entity class.
    /// </summary>
    public class ClosePositionEntity
    {
        /// <summary>
        /// Gets or sets the open position record ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the portfolio ID.
        /// </summary>
        public int PortfolioId { get; set; }

        /// <summary>
        ///  Gets or sets the stock symbol.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Gets or sets the date the stock was purchased.
        /// </summary>
        public DateTime BuyDate { get; set; }

        /// <summary>
        /// Gets or sets the quantity of stocks.  (cost / purchase = quantity).
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        ///  Gets or sets the purchase price of each share.  (cost / quantity = purchase).
        /// </summary>
        public decimal Purchase { get; set; }

        /// <summary>
        /// Gets or sets the total cost of the shares.  (quantity * purchase = cost).
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets the date the stock was sold.
        /// </summary>
        public DateTime SellDate { get; set; }

        /// <summary>
        /// Gets or sets the total amount the stock was sold.  (proceeds / quantity = share price).
        /// </summary>
        public decimal Proceeds { get; set; }

        /// <summary>
        /// Gets or sets the portfolio.
        /// </summary>
        public PortfolioEntity Portfolio { get; set; }
    }
}