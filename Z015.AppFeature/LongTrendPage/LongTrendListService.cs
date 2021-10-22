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

        private static DateTimeOffset EasternTimeNow => TimeZoneInfo.ConvertTime(DateTimeOffset.Now, Constant.EasternTimeZone);

        /// <summary>
        /// Get long trend table.
        /// </summary>
        /// <param name="yearCount">Number of years for the price history.</param>
        /// <returns>ResultTable.</returns>
        public async Task<ResultTable> GetLongTrends(int yearCount)
        {
            var easternTime = EasternTimeNow;

            // Get current start of month.
            DateTime endMonth = easternTime.Date.AddDays(1 - easternTime.Day);

            // get only the month we want for each year.
            var years = Enumerable.Range(0, yearCount + 1)
                .Select(i => endMonth.AddYears(0 - i))
                .ToList();

            var headers = years.Take(yearCount).Select(y => y.Year.ToString());

            using var db = this.dbFactory.CreateDbContext();

            var stockPrices = await db.Stocks
                            .Where(s => !s.IsSymbolNotFound)
                            .Join(
                                db.StockPrices.Where(p => years.Contains(p.Date)),
                                o => o.Id,
                                i => i.StockId,
                                (o, i) => new { o.Symbol, i.Date, i.Close })
                            .GroupBy(k => new { k.Symbol, k.Date }) //// Sometimes the symbol is on both exchanges.
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
                        percentages = new double?[yearCount];
                    }
                }
                else
                {
                    if (validItem)
                    {
                        int idx = endMonth.Year - item.Date.Year;
                        percentages[idx] = (item.Close - lastClose) / item.Close;
                    }
                }

                lastClose = item.Close;
            }

            if (validItem)
            {
                int score = CalculateScore(percentages);
                rows.Add(new(currentSymbol, percentages, firstClose, score));
            }

            return new ResultTable(headers, rows.OrderByDescending(v => v.Score));
        }

        private static int CalculateScore(double?[] percentages)
        {
            string test = string.Empty;
            int score = 0;
            for (int i = 0; i < percentages.Length; i++)
            {
                if (percentages[i].HasValue)
                {
                    bool isPositive = percentages[i].Value >= 0;
                    score = (score << 1) + (isPositive ? 1 : 0);
                    test += isPositive ? "1" : "0";
                }
            }

            return score;
        }
    }
}