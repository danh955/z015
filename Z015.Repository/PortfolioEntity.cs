// <copyright file="PortfolioEntity.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.Repository
{
    using System.Collections.Generic;

    /// <summary>
    /// Portfolio entity class.
    /// </summary>
    public class PortfolioEntity
    {
        /// <summary>
        /// Gets or sets the portfolio entity ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the name of the portfolio.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public UserEntity User { get; set; }

        /// <summary>
        /// Gets or sets the close positions.
        /// </summary>
        public List<ClosePositionEntity> ClosePositions { get; set; }

        /// <summary>
        /// Gets or sets the open positions.
        /// </summary>
        public List<OpenPositionEntity> OpenPositions { get; set; }

        /// <summary>
        /// Gets or sets the orders.
        /// </summary>
        public List<OrderEntity> Orders { get; set; }
    }
}