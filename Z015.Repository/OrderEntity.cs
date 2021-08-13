﻿// <copyright file="OrderEntity.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.Repository
{
    using System;

    /// <summary>
    /// Stock order class.
    /// </summary>
    public class OrderEntity
    {
        /// <summary>
        /// Gets or sets the order ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        ///  Gets or sets the stock symbol.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Gets or sets the order action type.
        /// </summary>
        public ActionType ActionType { get; set; }

        /// <summary>
        /// Gets or sets the quantity of stocks.
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Gets or sets the date the order was filled.  Null if not filled.
        /// </summary>
        public DateTime? FillDate { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public UserEntity User { get; set; }
    }
}