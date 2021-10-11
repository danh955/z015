// <copyright file="UpdateStockPrices.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.BackgroundTask
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Hilres.FinanceClient.Abstraction;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Z015.Repository;

    /// <summary>
    /// Update stock prices class.
    /// </summary>
    public class UpdateStockPrices
    {
        private readonly ActionBlock<UpdateStockPricesOptions> actionBlock;
        private readonly IDbContextFactory<RepositoryDbContext> dbFactory;
        private readonly ILogger<UpdateStockPrices> logger;
        private readonly IYahooService yahoo;

        private int count = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateStockPrices"/> class.
        /// </summary>
        /// <param name="yahoo">IYahooService.</param>
        /// <param name="dbFactory">IDbContextFactory for RepositoryDbContext.</param>
        /// <param name="logger">ILogger.</param>
        public UpdateStockPrices(IYahooService yahoo, IDbContextFactory<RepositoryDbContext> dbFactory, ILogger<UpdateStockPrices> logger)
        {
            this.yahoo = yahoo;
            this.dbFactory = dbFactory;
            this.logger = logger;
            this.actionBlock = new(this.UpdatePriceWorker, new() { BoundedCapacity = -1 });
        }

        /// <summary>
        /// Gets how many items are in the input count.
        /// </summary>
        /// <returns>Number of items in the input queue.</returns>
        internal int InputCount => this.actionBlock.InputCount;

        /// <summary>
        /// Do the stock price update.
        /// </summary>
        /// <param name="frequency">tThe frequency of the stock price.</param>
        /// <param name="firstDate">The first date of stock prices to get.  Null for max.</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>True if process has started.  False try again later.</returns>
        internal async Task<bool> DoUpdateAsync(StockFrequency frequency, DateTime firstDate, CancellationToken cancellationToken)
        {
            if (this.actionBlock.InputCount > 0)
            {
                return false;
            }

            var symbols = await this.GetListOfSymbols(cancellationToken);

            this.logger.LogInformation("Queuing symbols.  Count: {0:#,##0}", symbols.Count());
            foreach (string symbol in symbols)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var options = new UpdateStockPricesOptions()
                {
                    Symbol = symbol,
                    Frequency = frequency,
                    FirstDate = firstDate,
                    LastDate = null,
                    CancellationToken = cancellationToken,
                };

                this.logger.LogDebug("Queuing {0}", options);
                var isAccepted = await this.actionBlock.SendAsync(options, cancellationToken).ConfigureAwait(false);

                if (!isAccepted)
                {
                    this.logger.LogWarning("Queuing not accepted {0}", options);
                }
            }

            this.logger.LogInformation("Queuing symbols finished.");

            return true;
        }

        private async Task<IEnumerable<string>> GetListOfSymbols(CancellationToken cancellationToken)
        {
            var db = this.dbFactory.CreateDbContext();
            return await db.Stocks.Select(s => s.Symbol).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Update one database stock prices.
        /// This worker is called by the ActionBlock.
        /// </summary>
        /// <param name="options">UpdatePriceOptions.</param>
        /// <returns>Task.</returns>
        private async Task UpdatePriceWorker(UpdateStockPricesOptions options)
        {
            //// await Task.Delay(100, options.CancellationToken);
            this.count++;
            this.logger.LogInformation("({0}, {1}) Processing update for {2}", this.actionBlock.InputCount, this.count, options);

            if (options.CancellationToken.IsCancellationRequested)
            {
                this.logger.LogInformation("UpdatePriceWorker has been canceled.  {0}", options.Symbol);
                return;
            }

            using var db = this.dbFactory.CreateDbContext();
            var stockId = await db.Stocks
                            .Where(s => s.Symbol == options.Symbol)
                            .Select(s => s.Id)
                            .FirstOrDefaultAsync(options.CancellationToken).ConfigureAwait(false);

            if (stockId == 0)
            {
                this.logger.LogWarning("Symbol {0} not found in {1} table.", options.Symbol, nameof(db.Stocks));
                return;
            }

            var (isSuccessful, rawPrices, errorMessage) = await this.yahoo.GetStockPricesAsync(options.Symbol, options.FirstDate, options.LastDate, options.Frequency.ToYahooInterval(), options.CancellationToken).ConfigureAwait(false);
            if (!isSuccessful)
            {
                if (errorMessage == HttpStatusCode.NotFound.ToString())
                {
                    //// TODO: If starts with "NotFound", mark it as invalid symbol.
                    return;
                }

                this.logger.LogWarning("{0}.{1} error: {2}", nameof(UpdateStockPrices), nameof(this.UpdatePriceWorker), errorMessage);
                return;
            }

            this.logger.LogDebug("Retrieved {0} prices", rawPrices?.Count);

            var yahooDictionary = rawPrices
                                    .Where(y => y.Open.HasValue && y.High.HasValue && y.Low.HasValue && y.Close.HasValue && y.AdjClose.HasValue && y.Volume.HasValue)
                                    .Select(y =>
                                    {
                                        double adjust = 1 + ((y.AdjClose.Value - y.Close.Value) / y.Close.Value);
                                        return new StockPriceEntity
                                        {
                                            StockId = stockId,
                                            Frequency = StockFrequency.Monthly,
                                            Date = y.Date,
                                            Open = y.Open.Value * adjust,
                                            High = y.High.Value * adjust,
                                            Low = y.Low.Value * adjust,
                                            Close = y.AdjClose.Value,
                                            Volume = y.Volume.Value,
                                        };
                                    })
                                    .ToDictionary(p => p.Date);

            var dbDictionary = await db.StockPrices
                                    .Where(s => s.StockId == stockId)
                                    .Select(s => s)
                                    .ToDictionaryAsync(t => t.Date, options.CancellationToken).ConfigureAwait(false);

            //// Delete stock prices.

            var deletePrices = dbDictionary.Values
                                .Where(d => !yahooDictionary.ContainsKey(d.Date));

            if (deletePrices.Any())
            {
                this.logger.LogDebug("Deleting {0:#,##0} items from {1} table.", deletePrices.Count(), nameof(db.StockPrices));
                db.StockPrices.RemoveRange(deletePrices);
                await db.SaveChangesAsync(options.CancellationToken).ConfigureAwait(false);
            }
            else
            {
                this.logger.LogDebug("No deletions");
            }

            //// Update existing stock prices.

            var updatePrices = new List<StockPriceEntity>();
            foreach (var dbPrice in dbDictionary.Values)
            {
                if (yahooDictionary.TryGetValue(dbPrice.Date, out StockPriceEntity yPrice))
                {
                    if (dbPrice.Open != yPrice.Open
                        || dbPrice.High != yPrice.High
                        || dbPrice.Low != yPrice.Low
                        || dbPrice.Close != yPrice.Close
                        || dbPrice.Volume != yPrice.Volume)
                    {
                        updatePrices.Add(dbPrice);
                    }
                }
            }

            if (updatePrices.Any())
            {
                this.logger.LogDebug("Updating {0:#,##0} item in {1} table.", updatePrices.Count, nameof(db.StockPrices));
                db.StockPrices.UpdateRange(updatePrices);
                await db.SaveChangesAsync(options.CancellationToken).ConfigureAwait(false);
            }
            else
            {
                this.logger.LogDebug("No updates");
            }

            //// Add new stock prices.

            var newPricess = yahooDictionary.Values
                            .Where(y => !dbDictionary.ContainsKey(y.Date));

            if (newPricess.Any())
            {
                this.logger.LogDebug("Adding {0:#,##0} items into {1} table.", newPricess.Count(), nameof(db.StockPrices));
                db.StockPrices.AddRange(newPricess);
                await db.SaveChangesAsync(options.CancellationToken).ConfigureAwait(false);
            }
            else
            {
                this.logger.LogDebug("No additions");
            }
        }
    }
}