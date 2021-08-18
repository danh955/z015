// <copyright file="OpenPositionListService.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.AppFeature.PortfolioPage
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Open position list service.
    /// </summary>
    public class OpenPositionListService
    {
        private static readonly List<OpenPosition> TestData = new()
        {
            new OpenPosition(1, "MSFT", new DateTime(2021, 6, 7), 40, 249.95m, 9998.00m),
            new OpenPosition(2, "GOOG", new DateTime(2021, 7, 22), 1, 2653.00m, 2653.00m),
        };

        /// <summary>
        /// Get open positions.
        /// </summary>
        /// <returns>List of open positions.</returns>
        public List<OpenPosition> GetOpenPositions()
        {
            return TestData;
        }

        /// <summary>
        /// Open position record.
        /// </summary>
        public record OpenPosition(int Id, string Symbol, DateTime BuyDate, decimal Quantity, decimal Purchase, decimal Cost);
    }
}