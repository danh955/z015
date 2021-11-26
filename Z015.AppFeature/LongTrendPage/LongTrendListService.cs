// <copyright file="LongTrendListService.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.AppFeature.LongTrendPage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Z015.Repository;

    /// <summary>
    /// Long trend list service class.
    /// </summary>
    public class LongTrendListService
    {
        private readonly IDbContextFactory<RepositoryDbContext> dbFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="LongTrendListService"/> class.
        /// </summary>
        /// <param name="dbFactory">Repository database context factory.</param>
        public LongTrendListService(IDbContextFactory<RepositoryDbContext> dbFactory)
        {
            this.dbFactory = dbFactory;
        }

        /// <summary>
        /// Get long trend table.
        /// </summary>
        /// <param name="options">LongTrendListOptions.</param>
        /// <returns>ResultTable.</returns>
        public async Task<ResultTable> GetLongTrends(LongTrendListOptions options)
        {
            //// Convert to DateOnly when EF can handle it.
            DateTime endDate = new(options.EndYear, options.EndMonth, 1);

            // get only the month we want for each year.
            var years = Enumerable.Range(0, options.ColumnCount + 1)
                .Select(i => endDate.AddMonths(0 - (i * options.FrequencyMonths)))
                .ToList();

            var headers = years.Take(options.ColumnCount).Select(y => DateOnly.FromDateTime(y));

            using var db = this.dbFactory.CreateDbContext();

            var stockPrices = await db.Stocks
                            .Where(s => !s.IsSymbolNotFound)
                            .Join(
                                db.StockPrices
                                    .Where(p => p.Frequency == StockFrequency.Monthly)
                                    .Where(p => years.Contains(p.Date)),
                                o => o.Id,
                                i => i.StockId,
                                (o, i) => new { o.Symbol, i.Date, i.Close })
                            .GroupBy(k => new { k.Symbol, k.Date }) //// Sometimes the symbol is in both exchanges.
                            .Select(g => new { g.Key.Symbol, g.Key.Date, Close = g.Average(i => i.Close) })
                            .OrderBy(v => v.Symbol).ThenBy(v => v.Date)
                            .ToListAsync();

            List<ResultRow> rows = new();
            string currentSymbol = null;
            double?[] percentages = null;
            double lastClose = 0;
            double firstClose = 0;
            bool validItem = true;

            foreach (var item in stockPrices)
            {
                if (item.Symbol != currentSymbol)
                {
                    if (currentSymbol != null)
                    {
                        if (validItem)
                        {
                            int score = CalculateScore(percentages);
                            rows.Add(new(currentSymbol, percentages, firstClose, score));
                        }
                    }

                    currentSymbol = item.Symbol;
                    validItem = item.Close > 10;

                    if (validItem)
                    {
                        firstClose = item.Close;
                        percentages = new double?[options.ColumnCount];
                    }
                }
                else
                {
                    if (validItem)
                    {
                        int idx = GetTotalMonths(item.Date, endDate);
                        idx = idx / options.FrequencyMonths;
                        percentages[idx] = (item.Close - lastClose) / item.Close;
                    }
                }

                lastClose = item.Close;
            }

            if (validItem && stockPrices.Any())
            {
                int score = CalculateScore(percentages);
                rows.Add(new(currentSymbol, percentages, firstClose, score));
            }

            return new ResultTable(headers, rows.OrderByDescending(v => v.Score));
        }

        private static int CalculateScore(double?[] percentages)
        {
            int score = 0;
            for (int i = 0; i < percentages.Length; i++)
            {
                if (percentages[i].HasValue)
                {
                    bool isPositive = percentages[i].Value >= 0;
                    score = (score << 1) + (isPositive ? 1 : 0);
                }
            }

            return score;
        }

        /// <summary>
        /// Get the total months between two date.  This will count whole months and not care about the day.
        /// </summary>
        /// <param name="firstDate">First date.</param>
        /// <param name="lastDate">Last date.</param>
        /// <returns>Number of month apart.</returns>
        private static int GetTotalMonths(DateOnly firstDate, DateOnly lastDate)
        {
            int yearsAppart = lastDate.Year - firstDate.Year;
            int monthsAppart = lastDate.Month - firstDate.Month;
            return (yearsAppart * 12) + monthsAppart;
        }

        private static int GetTotalMonths(DateTime firstDate, DateTime lastDate)
        {
            return GetTotalMonths(DateOnly.FromDateTime(firstDate), DateOnly.FromDateTime(lastDate));
        }
    }
}