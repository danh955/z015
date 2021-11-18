// <copyright file="UpdateStockSymbol.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.BackgroundTask
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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
    public class UpdateStockSymbol
    {
        private static readonly string[] ExchangeList = { "NASDAQ", "NYSE" };
        private readonly IDbContextFactory<RepositoryDbContext> dbFactory;
        private readonly ILogger<UpdateStockSymbol> logger;
        private readonly ITiingoService tiingo;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateStockSymbol"/> class.
        /// </summary>
        /// <param name="tiingo">ITiingoService.</param>
        /// <param name="dbFactory">IDbContextFactory for RepositoryDbContext.</param>
        /// <param name="logger">ILogger.</param>
        public UpdateStockSymbol(ITiingoService tiingo, IDbContextFactory<RepositoryDbContext> dbFactory, ILogger<UpdateStockSymbol> logger)
        {
            this.logger = logger;
            this.tiingo = tiingo;
            this.dbFactory = dbFactory;
        }

        /// <summary>
        /// Do update from Tiingo of symbols in database.
        /// </summary>
        /// <param name="lastMarketClosed">The last time the market was closed.</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>Task.</returns>
        internal async Task DoUpdateFromTiingoAsync(DateTimeOffset lastMarketClosed, CancellationToken cancellationToken)
        {
            try
            {
                using var db = this.dbFactory.CreateDbContext();
                DateTimeOffset lastDateUpdated = await db.TiingoSupportedTickers
                                                .Select(t => t.DateUpdated)
                                                .OrderBy(date => date)
                                                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

                if (lastDateUpdated > lastMarketClosed)
                {
                    this.logger.LogInformation("Skipping updating symbols.");
                    return;
                }

                this.logger.LogInformation("{Class}.{Function}", nameof(UpdateStockSymbol), nameof(this.DoUpdateFromTiingoAsync));

                // Get a list of supported Tiingo tickers.
                var (tiingoStocks, errorMessage) = await this.tiingo.GetSupportedTickersAsync(cancellationToken);
                if (errorMessage != null)
                {
                    this.logger.LogWarning("{Function} error: {ErrorMessage}", nameof(this.tiingo.GetSupportedTickersAsync), errorMessage);
                    return;
                }

                await this.UpdateStockSymbolTable(tiingoStocks, cancellationToken);
                await this.UpdateSupportedTickerTable(tiingoStocks, cancellationToken);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Error: {ErrorMessage}", e.Message);
            }
        }

        /// <summary>
        /// Update stock symbol database table from Tiingo.
        /// </summary>
        /// <param name="tiingoStocks">List of Tiingo stocks.</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>Task.</returns>
        private async Task UpdateStockSymbolTable(IEnumerable<TiingoSupportedStockTicker> tiingoStocks, CancellationToken cancellationToken)
        {
            this.logger.LogInformation("{Class}.{Function}", nameof(UpdateStockSymbol), nameof(this.UpdateStockSymbolTable));

            // Any stock ending before the cut off date will not be considered.
            DateTime cutOffDate = DateTime.Today.AddDays(-7);

            using var db = this.dbFactory.CreateDbContext();
            var dbStockDictionary = await db.Stocks.ToDictionaryAsync(s => new { s.Symbol, s.Exchange }, cancellationToken);

            var newStocks = tiingoStocks
                                .Where(d => d.Ticker.Trim().Length > 0
                                        && d.Ticker.Trim().All(char.IsLetter)
                                        && ExchangeList.Contains(d.Exchange)
                                        && d.StartDate.HasValue && d.EndDate.HasValue
                                        && d.EndDate.Value > cutOffDate)
                                .Where(d => !dbStockDictionary.ContainsKey(new { Symbol = d.Ticker.Trim().ToUpper(), d.Exchange }))
                                .GroupBy(d => new { Symbol = d.Ticker.Trim().ToUpper(), Exchange = d.Exchange.Trim() }) // remove duplicates.
                                .Select(d => new StockEntity
                                {
                                    Symbol = d.Key.Symbol,
                                    Name = d.Key.Symbol,
                                    Exchange = d.Key.Exchange,
                                    AssetType = d.Max(v => v.AssetType),
                                    //// StartDate = d.Min(v => v.StartDate),
                                    //// EndDate = d.Max(v => v.EndDate),
                                });

            this.logger.LogInformation("Adding {NewStocksCount:#,##0} stock symbols.", newStocks.Count());

            db.Stocks.AddRange(newStocks);
            await db.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Update Tiingo supported ticker database table from Tiingo.
        /// </summary>
        /// <param name="tiingoStocks">List of Tiingo stocks.</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>Task.</returns>
        private async Task UpdateSupportedTickerTable(IEnumerable<TiingoSupportedStockTicker> tiingoStocks, CancellationToken cancellationToken)
        {
            this.logger.LogInformation("{Class}.{Function}", nameof(UpdateStockSymbol), nameof(this.UpdateSupportedTickerTable));

            using var db = this.dbFactory.CreateDbContext();
            var dbTickerDictionary = await db.TiingoSupportedTickers.ToDictionaryAsync(t => new { t.Ticker, t.Exchange, t.StartDate }, cancellationToken);

            var tiingoStockDictionary = tiingoStocks
                                        .GroupBy(t => new { t.Ticker, t.Exchange, t.StartDate })
                                        .Select(g => g.First()) // Remove duplicates.
                                        .ToDictionary(t => new { t.Ticker, t.Exchange, t.StartDate });

            var now = DateTimeOffset.Now;

            //// Delete tickers.

            var deleteTickers = dbTickerDictionary.Values
                                .Where(t => !tiingoStockDictionary.ContainsKey(new { t.Ticker, t.Exchange, t.StartDate }));

            if (deleteTickers.Any())
            {
                this.logger.LogInformation("Deleting {DeleteTickers.Count:#,##0} tickers from {TableName} table.", deleteTickers.Count(), nameof(db.TiingoSupportedTickers));
                db.TiingoSupportedTickers.RemoveRange(deleteTickers);
                await db.SaveChangesAsync(cancellationToken);
            }

            //// Update existing tickers.

            var updateTickers = new Collection<TiingoSupportedTickerEntity>();
            foreach (var dbTicker in dbTickerDictionary.Values)
            {
                if (tiingoStockDictionary.TryGetValue(new { dbTicker.Ticker, dbTicker.Exchange, dbTicker.StartDate }, out TiingoSupportedStockTicker tiingoTicker))
                {
                    if (dbTicker.AssetType != tiingoTicker.AssetType
                        || dbTicker.PriceCurrency != tiingoTicker.PriceCurrency
                        || dbTicker.EndDate != tiingoTicker.EndDate)
                    {
                        dbTicker.AssetType = tiingoTicker.AssetType;
                        dbTicker.PriceCurrency = tiingoTicker.PriceCurrency;
                        dbTicker.EndDate = tiingoTicker.EndDate;
                        dbTicker.DateUpdated = now;
                        updateTickers.Add(dbTicker);
                    }
                }
            }

            if (updateTickers.Any())
            {
                this.logger.LogInformation("Updating {UpdateTickersCount:#,##0} tickers in {TableName} table.", updateTickers.Count, nameof(db.TiingoSupportedTickers));
                db.TiingoSupportedTickers.UpdateRange(updateTickers);
                await db.SaveChangesAsync(cancellationToken);
            }

            //// Add new tickers.

            var newTickers = tiingoStockDictionary.Values
                            .Where(t => !dbTickerDictionary.ContainsKey(new { t.Ticker, t.Exchange, t.StartDate }))
                            .Select(t => new TiingoSupportedTickerEntity
                            {
                                Ticker = t.Ticker,
                                Exchange = t.Exchange,
                                AssetType = t.AssetType,
                                PriceCurrency = t.PriceCurrency,
                                StartDate = t.StartDate,
                                EndDate = t.EndDate,
                                DateAdded = now,
                                DateUpdated = now,
                            });

            if (newTickers.Any())
            {
                this.logger.LogInformation("Adding {NewTickersCount:#,##0} to {TableName} table.", newTickers.Count(), nameof(db.TiingoSupportedTickers));
                db.TiingoSupportedTickers.AddRange(newTickers);
                await db.SaveChangesAsync(cancellationToken);
            }
        }
    }
}