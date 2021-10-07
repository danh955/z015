// <copyright file="UpdateStockPrices.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.BackgroundTask
{
    using System.Collections.Generic;
    using System.Linq;
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
            this.actionBlock = new(this.UpdatePriceWorker);
        }

        /// <summary>
        /// Do the stock price update.
        /// </summary>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>True if process has started.  False try again later.</returns>
        internal async Task<bool> DoUpdate(CancellationToken cancellationToken)
        {
            if (this.actionBlock.InputCount > 0)
            {
                return false;
            }

            var options = new UpdateStockPricesOptions()
            {
                Symbol = "QQQ",
                Frequency = StockFrequency.Monthly,
                FirstDate = new(2020, 1, 1),
                LastDate = null,
                CancellationToken = cancellationToken,
            };

            this.logger.LogInformation("Queuing {0}", options);
            await this.actionBlock.SendAsync(options, cancellationToken);
            return true;
        }

        /// <summary>
        /// Update one database stock prices.
        /// This worker is called by the ActionBlock.
        /// </summary>
        /// <param name="options">UpdatePriceOptions.</param>
        /// <returns>Task.</returns>
        private async Task UpdatePriceWorker(UpdateStockPricesOptions options)
        {
            this.logger.LogInformation("Updating prices for {0}", options);

            using var db = this.dbFactory.CreateDbContext();
            var stockId = await db.Stocks
                            .Where(s => s.Symbol == options.Symbol)
                            .Select(s => s.Id)
                            .FirstOrDefaultAsync(options.CancellationToken);

            if (stockId == 0)
            {
                this.logger.LogWarning("Symbol {0} not found in {1} table.", options.Symbol, nameof(db.Stocks));
                return;
            }

            var (rawPrices, errorMessage) = await this.yahoo.GetStockPricesAsync(options.Symbol, options.FirstDate, options.LastDate, options.Frequency.ToYahooInterval(), options.CancellationToken);
            if (errorMessage != null)
            {
                this.logger.LogWarning("{0}.{1} error: {2}", nameof(UpdateStockPrices), nameof(this.UpdatePriceWorker), errorMessage);
                return;
            }

            this.logger.LogInformation("Retrieved {0} prices", rawPrices?.Count);

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
                                    .ToDictionaryAsync(t => t.Date, options.CancellationToken);

            //// Delete stock prices.

            var deletePrices = dbDictionary.Values
                                .Where(d => !yahooDictionary.ContainsKey(d.Date));

            if (deletePrices.Any())
            {
                this.logger.LogInformation("Deleting {0:#,##0} items from {1} table.", deletePrices.Count(), nameof(db.StockPrices));
                db.StockPrices.RemoveRange(deletePrices);
                await db.SaveChangesAsync(options.CancellationToken);
            }
            else
            {
                this.logger.LogInformation("No deletions");
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
                this.logger.LogInformation("Updating {0:#,##0} item in {1} table.", updatePrices.Count, nameof(db.StockPrices));
                db.StockPrices.UpdateRange(updatePrices);
                await db.SaveChangesAsync(options.CancellationToken);
            }
            else
            {
                this.logger.LogInformation("No updates");
            }

            //// Add new stock prices.

            var newPricess = yahooDictionary.Values
                            .Where(y => !dbDictionary.ContainsKey(y.Date));

            if (newPricess.Any())
            {
                this.logger.LogInformation("Adding {0:#,##0} items into {1} table.", newPricess.Count(), nameof(db.StockPrices));
                db.StockPrices.AddRange(newPricess);
                await db.SaveChangesAsync(options.CancellationToken);
            }
            else
            {
                this.logger.LogInformation("No additions");
            }
        }
    }
}