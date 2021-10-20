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
        /// <param name="frequency">The frequency of the stock price.</param>
        /// <param name="firstDate">The first date of stock prices to get.  Null for max.</param>
        /// <param name="cutOffDate">Get stock where the PriceUpdatedDate is less then the cutOffDate.</param>
        /// <param name="takeCount">Only get this many.</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>True if no more to process.  False need to process more.</returns>
        internal async Task<bool> DoUpdateAsync(StockFrequency frequency, DateTime firstDate, DateTimeOffset cutOffDate, int takeCount, CancellationToken cancellationToken)
        {
            if (this.actionBlock.InputCount > 0)
            {
                // The queue is not empty.
                return false;
            }

            var symbols = await this.GetListOfSymbols(cutOffDate, takeCount, cancellationToken);
            if (!symbols.Any())
            {
                // No more to process. All done.
                return true;
            }

            this.logger.LogInformation("Queuing symbols.  Count: {0:#,##0}", symbols.Count());
            foreach (var (stockId, symbol) in symbols)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var options = new UpdateStockPricesOptions()
                {
                    StockId = stockId,
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
                    this.logger.LogWarning("ActionBlock rejecting new items. {0}", options);
                }
            }

            // There will be more to process.
            return false;
        }

        private async Task<IEnumerable<Tuple<int, string>>> GetListOfSymbols(DateTimeOffset cutOffDate, int takeCount, CancellationToken cancellationToken)
        {
            var db = this.dbFactory.CreateDbContext();
            return await db.Stocks
                .Where(s => s.PriceUpdatedDate == null || s.PriceUpdatedDate < cutOffDate)
                .OrderBy(s => s.PriceUpdatedDate)
                .Take(takeCount)
                .Select(s => Tuple.Create(s.Id, s.Symbol))
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Update one database stock prices.
        /// This worker is called by the ActionBlock.
        /// </summary>
        /// <param name="options">UpdatePriceOptions.</param>
        /// <returns>Task.</returns>
        private async Task UpdatePriceWorker(UpdateStockPricesOptions options)
        {
            try
            {
                this.count++;
                this.logger.LogDebug("({0}, {1}) Processing update for {2}", this.actionBlock.InputCount, this.count, options);

                if (options.CancellationToken.IsCancellationRequested)
                {
                    this.logger.LogInformation("UpdatePriceWorker has been canceled.  {0}", options.Symbol);
                    return;
                }

                using var db = this.dbFactory.CreateDbContext();

                var (isSuccessful, rawPrices, errorMessage) = await this.yahoo.GetStockPricesAsync(options.Symbol, options.FirstDate, options.LastDate, options.Frequency.ToYahooInterval(), options.CancellationToken).ConfigureAwait(false);
                if (!isSuccessful)
                {
                    if (errorMessage == HttpStatusCode.NotFound.ToString())
                    {
                        // Mark it as not found and try it again in the far future.
                        DateTimeOffset nextTryDate = DateTimeOffset.Now.AddDays(5 + (DateTime.Now.Ticks % 30));
                        var notFoundStock = await db.Stocks
                                            .Where(s => s.Id == options.StockId)
                                            .SingleAsync(options.CancellationToken);
                        notFoundStock.IsSymbolNotFound = true;
                        notFoundStock.PriceUpdatedDate = nextTryDate;
                        await db.SaveChangesAsync(options.CancellationToken);
                        this.logger.LogInformation("{0} not found.  Next try on {1}", options.Symbol, nextTryDate);
                        return;
                    }

                    this.logger.LogWarning("{0}.{1} error: {2}", nameof(UpdateStockPrices), nameof(this.UpdatePriceWorker), errorMessage);
                    return;
                }

                this.logger.LogInformation("({0}, {1}) Retrieved {2,5:#,##0} {3}", this.actionBlock.InputCount, this.count, rawPrices?.Count, options);

                var yahooDictionary = rawPrices
                        .Where(y => y.Open.HasValue && y.High.HasValue && y.Low.HasValue && y.Close.HasValue && y.AdjClose.HasValue && y.Volume.HasValue)
                        .GroupBy(y => y.Date) // remove duplicates.
                        .Select(yy =>
                        {
                            var y = yy.OrderByDescending(y => y.Volume).First();
                            double adjust = 1 + ((y.AdjClose.Value - y.Close.Value) / y.Close.Value);
                            return new StockPriceEntity
                            {
                                StockId = options.StockId,
                                Frequency = options.Frequency,
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
                                        .Where(s => s.StockId == options.StockId)
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

                // Say we are done updating this stock.
                var stock = await db.Stocks
                                .Where(s => s.Id == options.StockId)
                                .SingleAsync(options.CancellationToken);
                stock.IsSymbolNotFound = false;
                stock.PriceUpdatedDate = DateTimeOffset.Now;
                await db.SaveChangesAsync(options.CancellationToken);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "{0}.{1} {2}", nameof(UpdateStockPrices), nameof(this.UpdatePriceWorker), options);
            }
        }
    }
}