// <copyright file="OpenPositionListService.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.AppFeature.PortfolioPage
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Z015.Repository;

    /// <summary>
    /// Open position list service.
    /// </summary>
    public class OpenPositionListService
    {
        private readonly IDbContextFactory<RepositoryDbContext> dbFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenPositionListService"/> class.
        /// </summary>
        /// <param name="dbFactory">Repository database context factory.</param>
        public OpenPositionListService(IDbContextFactory<RepositoryDbContext> dbFactory)
        {
            this.dbFactory = dbFactory;
        }

        /// <summary>
        /// Get open positions.
        /// </summary>
        /// <param name="portfolioId">Portfolio ID.</param>
        /// <returns>List of open positions.</returns>
        public List<OpenPosition> GetOpenPositions(int portfolioId = 0)
        {
            using var db = this.dbFactory.CreateDbContext();

            return db.OpenPositions
                .Where(o => o.PortfolioId == portfolioId)
                .Select(o => new OpenPosition(o.Id, o.Symbol, o.BuyDate, o.Quantity, o.Purchase, o.Cost))
                .ToList();
        }
    }
}