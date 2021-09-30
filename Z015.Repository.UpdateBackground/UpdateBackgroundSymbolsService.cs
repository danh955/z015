// <copyright file="UpdateBackgroundSymbolsService.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.Repository.UpdateBackground
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Hilres.FinanceClient.Abstraction;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Z015.Repository;

    /// <summary>
    /// Update the symbols in the database from the finance client data.
    /// </summary>
    internal class UpdateBackgroundSymbolsService
    {
        private static readonly string[] ExchangeList = { "NASDAQ", "NYSE" };
        private readonly IDbContextFactory<RepositoryDbContext> dbFactory;
        private readonly ILogger<UpdateBackgroundSymbolsService> logger;
        private readonly ITiingoService tiingo;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateBackgroundSymbolsService"/> class.
        /// </summary>
        /// <param name="tiingo">ITiingoService.</param>
        /// <param name="dbFactory">IDbContextFactory for RepositoryDbContext.</param>
        /// <param name="logger">ILogger.</param>
        public UpdateBackgroundSymbolsService(ITiingoService tiingo, IDbContextFactory<RepositoryDbContext> dbFactory, ILogger<UpdateBackgroundSymbolsService> logger)
        {
            this.logger = logger;
            this.tiingo = tiingo;
            this.dbFactory = dbFactory;
        }

        /// <summary>
        /// Do update from Tiingo of symbols in database.
        /// </summary>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>Task.</returns>
        internal async Task DoUpdateFromTiingo(CancellationToken cancellationToken)
        {
            // Any stock ending before the cut off date will not be considered.
            DateTime cutOffDate = DateTime.Today.AddDays(-7);

            try
            {
                this.logger.LogInformation("{0}.{1}", nameof(UpdateBackgroundSymbolsService), nameof(this.DoUpdateFromTiingo));

                // Get a list of supported Tiingo tickers.
                var (tiingoStocks, errorMessage) = await this.tiingo.GetSupportedTickersAsync(cancellationToken);
                if (errorMessage != null)
                {
                    this.logger.LogWarning("{0} error: {2}", nameof(this.tiingo.GetSupportedTickersAsync), errorMessage);
                    return;
                }

                using var db = this.dbFactory.CreateDbContext();
                var dbStocks = await db.Stocks.Select(s => s).ToListAsync(cancellationToken);
                Dictionary<string, StockEntity> stocks = dbStocks.ToDictionary(s => s.Symbol);

                var sourceStocks = tiingoStocks
                                    .Where(d => d.Ticker.Trim().Length > 0
                                            && d.Ticker.Trim().All(char.IsLetter)
                                            && ExchangeList.Contains(d.Exchange)
                                            && d.StartDate.HasValue && d.EndDate.HasValue
                                            && d.EndDate.Value > cutOffDate)
                                    .GroupBy(d => new { Symbol = d.Ticker.Trim().ToUpper(), Exchange = d.Exchange.Trim() })
                                    .Select(d => new StockEntity
                                    {
                                        Symbol = d.Key.Symbol,
                                        Name = d.Key.Symbol,
                                        Exchange = d.Key.Exchange,
                                        //// StartDate = d.Min(v => v.StartDate),
                                        //// EndDate = d.Max(v => v.EndDate),
                                    });

                var newStocks = sourceStocks.Where(d => !stocks.ContainsKey(d.Symbol.Trim().ToUpper()));

                this.logger.LogInformation("Adding {0:#,##0} stock symbols.", newStocks.Count());

                db.Stocks.AddRange(newStocks.Distinct());
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Error: {0}", e.Message);
            }
        }
    }
}