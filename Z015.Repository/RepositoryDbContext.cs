// <copyright file="RepositoryDbContext.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.Repository
{
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Repository database context class.
    /// </summary>
    public class RepositoryDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryDbContext"/> class.
        /// </summary>
        /// <param name="options">DbContextOptions.</param>
        public RepositoryDbContext(DbContextOptions<RepositoryDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the user table.
        /// </summary>
        public DbSet<UserEntity> Users { get; set; }

        /// <summary>
        /// Gets or sets the user table.
        /// </summary>
        public DbSet<PortfolioEntity> Portfolios { get; set; }

        /// <summary>
        /// Gets or sets the closed positions table.
        /// </summary>
        public DbSet<ClosePositionEntity> ClosePositions { get; set; }

        /// <summary>
        /// Gets or sets the open positions table.
        /// </summary>
        public DbSet<OpenPositionEntity> OpenPositions { get; set; }

        /// <summary>
        /// Gets or sets the orders table.
        /// </summary>
        public DbSet<OrderEntity> Orders { get; set; }

        /// <summary>
        /// Gets or sets the stock table.
        /// </summary>
        public DbSet<StockEntity> Stocks { get; set; }

        /// <summary>
        /// Gets or sets the stock price table.
        /// </summary>
        public DbSet<StockPriceEntity> StockPrices { get; set; }

        /// <summary>
        /// Gets or sets the Tiingo supported stock tickers.
        /// </summary>
        public DbSet<TiingoSupportedTickerEntity> TiingoSupportedTickers { get; set; }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RepositoryDbContext).Assembly);
        }
    }
}