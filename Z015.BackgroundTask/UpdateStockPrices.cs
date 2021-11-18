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
        private static readonly string[] SkipHttpStatusCode =
            {
                HttpStatusCode.NotFound.ToString(),
                HttpStatusCode.BadRequest.ToString(),
            };

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
        /// This will find all the stock that need updated and put them in the workers queue.
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

            var symbols = await this.GetListOfSymbols(frequency, cutOffDate, takeCount, cancellationToken);
            if (!symbols.Any())
            {
                // No more to process. All done.
                return true;
            }

            this.logger.LogInformation("Queuing symbols.  Count: {SymbolsCount:#,##0}", symbols.Count());
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
                    LastDate = cutOffDate.Date,
                    CancellationToken = cancellationToken,
                };

                this.logger.LogDebug("Queuing {Options}", options);
                var isAccepted = await this.actionBlock.SendAsync(options, cancellationToken).ConfigureAwait(false);

                if (!isAccepted)
                {
                    this.logger.LogWarning("ActionBlock rejecting new items. {Options}", options);
                }
            }

            // There will be more to process.
            return false;
        }

        private async Task AddNewStockPricesAsync(UpdateStockPricesOptions options, Dictionary<DateTime, StockPriceEntity> yahooDictionary, Dictionary<DateTime, StockPriceEntity> dbDictionary)
        {
            using var db = this.dbFactory.CreateDbContext();

            var newPricess = yahooDictionary.Values
                            .Where(y => !dbDictionary.ContainsKey(y.Date));

            if (newPricess.Any())
            {
                this.logger.LogDebug("Adding {NewPricessCount:#,##0} items into {TableName} table.", newPricess.Count(), nameof(db.StockPrices));
                db.StockPrices.AddRange(newPricess);
                await db.SaveChangesAsync(options.CancellationToken).ConfigureAwait(false);
            }
            else
            {
                this.logger.LogDebug("No additions");
            }
        }

        private async Task DeleteStockPricesAsync(UpdateStockPricesOptions options, Dictionary<DateTime, StockPriceEntity> yahooDictionary, Dictionary<DateTime, StockPriceEntity> dbDictionary)
        {
            using var db = this.dbFactory.CreateDbContext();

            var deletePrices = dbDictionary.Values
                                .Where(d => !yahooDictionary.ContainsKey(d.Date));

            if (deletePrices.Any())
            {
                this.logger.LogDebug("Deleting {DeletePricesCount:#,##0} items from {TableName} table.", deletePrices.Count(), nameof(db.StockPrices));
                db.StockPrices.RemoveRange(deletePrices);
                await db.SaveChangesAsync(options.CancellationToken).ConfigureAwait(false);
            }
            else
            {
                this.logger.LogDebug("No deletions");
            }
        }

        private async Task<IEnumerable<Tuple<int, string>>> GetListOfSymbols(StockFrequency frequency, DateTimeOffset cutOffDate, int takeCount, CancellationToken cancellationToken)
        {
            var db = this.dbFactory.CreateDbContext();

            return await db.Stocks
                .Where(s => !db.StockPriceLastUpdates
                                    .Any(u => u.StockId == s.Id
                                            && u.Frequency == frequency
                                            && u.PriceUpdatedDate > cutOffDate))
                .OrderBy(s => s.Symbol)
                .Take(takeCount)
                .Select(s => Tuple.Create(s.Id, s.Symbol))
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Set the next try date.
        /// </summary>
        /// <param name="options">Option of stock to work with.</param>
        /// <param name="found">True if the stock was found.</param>
        /// <returns>Task.</returns>
        private async Task SetNextTryDateAsync(UpdateStockPricesOptions options, bool found)
        {
            using var db = this.dbFactory.CreateDbContext();

            // If not found, look at it in the future.
            DateTimeOffset updatedDate = found
                ? DateTimeOffset.Now
                : DateTimeOffset.Now.AddDays(5 + (DateTime.Now.Ticks % 30));

            // Change StockPriceLastUpdates table.
            var lastUpdate = await db.StockPriceLastUpdates
                                .Where(s => s.StockId == options.StockId && s.Frequency == options.Frequency)
                                .SingleOrDefaultAsync(options.CancellationToken).ConfigureAwait(false);
            if (lastUpdate == null)
            {
                db.StockPriceLastUpdates.Add(new StockPriceLastUpdatedEntity
                {
                    StockId = options.StockId,
                    Frequency = options.Frequency,
                    PriceUpdatedDate = updatedDate,
                });
            }
            else
            {
                lastUpdate.PriceUpdatedDate = updatedDate;
            }

            // Change Stocks table.
            var stock = await db.Stocks
                                .Where(s => s.Id == options.StockId)
                                .SingleAsync(options.CancellationToken).ConfigureAwait(false);
            if (stock.IsSymbolNotFound == found)
            {
                stock.IsSymbolNotFound = !found;
            }

            await db.SaveChangesAsync(options.CancellationToken).ConfigureAwait(false);

            if (!found)
            {
                this.logger.LogInformation("{OptionsSymbol} not found.  Next try after {UpdatedDate}", options.Symbol, updatedDate);
            }
        }

        private async Task UpdateExistingStockPricesAsync(UpdateStockPricesOptions options, Dictionary<DateTime, StockPriceEntity> yahooDictionary, Dictionary<DateTime, StockPriceEntity> dbDictionary)
        {
            using var db = this.dbFactory.CreateDbContext();

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
                this.logger.LogDebug("Updating {UpdatePricesCount:#,##0} item in {TableName} table.", updatePrices.Count, nameof(db.StockPrices));
                db.StockPrices.UpdateRange(updatePrices);
                await db.SaveChangesAsync(options.CancellationToken).ConfigureAwait(false);
            }
            else
            {
                this.logger.LogDebug("No updates");
            }
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
                this.logger.LogDebug("({ActionBlockInputCount}, {Count}) Processing update for {Options}", this.actionBlock.InputCount, this.count, options);

                if (options.CancellationToken.IsCancellationRequested)
                {
                    this.logger.LogDebug("UpdatePriceWorker has been canceled.  {Symbol}", options.Symbol);
                    return;
                }

                var (isSuccessful, rawPrices, errorMessage) = await this.yahoo.GetStockPricesAsync(options.Symbol, options.FirstDate, options.LastDate, options.Frequency.ToYahooInterval(), options.CancellationToken).ConfigureAwait(false);
                if (!isSuccessful)
                {
                    if (SkipHttpStatusCode.Contains(errorMessage))
                    {
                        await this.SetNextTryDateAsync(options, false).ConfigureAwait(false);
                        return;
                    }

                    this.logger.LogWarning("{Class}.{Function} error: {ErrorMessage}", nameof(UpdateStockPrices), nameof(this.UpdatePriceWorker), errorMessage);
                    return;
                }

                this.logger.LogInformation("({ActionBlock.InputCount}, {Count}) Retrieved {RawPricesCount,5:#,##0} {Options}", this.actionBlock.InputCount, this.count, rawPrices?.Count, options);

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

                using var db = this.dbFactory.CreateDbContext();
                var dbDictionary = await db.StockPrices
                                        .Where(s => s.StockId == options.StockId)
                                        .Select(s => s)
                                        .ToDictionaryAsync(t => t.Date, options.CancellationToken).ConfigureAwait(false);

                await this.DeleteStockPricesAsync(options, yahooDictionary, dbDictionary).ConfigureAwait(false);
                await this.UpdateExistingStockPricesAsync(options, yahooDictionary, dbDictionary).ConfigureAwait(false);
                await this.AddNewStockPricesAsync(options, yahooDictionary, dbDictionary).ConfigureAwait(false);
                await this.SetNextTryDateAsync(options, true).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "{Class}.{Function} {Options}", nameof(UpdateStockPrices), nameof(this.UpdatePriceWorker), options);
            }
        }
    }
}