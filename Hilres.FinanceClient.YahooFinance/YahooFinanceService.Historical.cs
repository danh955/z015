// <copyright file="YahooFinanceService.Historical.cs" company="None">
// Free and open source code.
// </copyright>

namespace Hilres.FinanceClient.YahooFinance
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using CsvHelper;
    using CsvHelper.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Yahoo finance service for historical data class.
    /// </summary>
    public partial class YahooFinanceService
    {
        /// <summary>
        /// Get stock history data from Yahoo.
        /// </summary>
        /// <param name="symbol">Symbol of prices to get.</param>
        /// <param name="firstDate">First date.</param>
        /// <param name="lastDate">Last date.</param>
        /// <param name="interval">Stock price interval.</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>Task with PriceListResult.</returns>
        public async Task<IReadOnlyList<StockPrice>> GetStockPricesAsync(string symbol, DateTime? firstDate, DateTime? lastDate, StockInterval? interval, CancellationToken cancellationToken)
        {
            this.logger.LogDebug("GetStockPricesAsync symbol={0}, firstDate={1}, lastDate={2}, interval={3}", symbol, firstDate, lastDate, interval);

            string period1 = firstDate.HasValue ? firstDate.Value.ToUnixTimestamp() : "0";
            string period2 = lastDate.HasValue ? lastDate.Value.ToUnixTimestamp() : DateTime.Now.ToUnixTimestamp();
            string intervalString = ToIntervalString(interval);

            string uri = $"https://query1.finance.yahoo.com/v7/finance/download/{symbol}?period1={period1}&period2={period2}&interval={intervalString}&events=history&includeAdjustedClose=true";

            return await this.GetItems(
                                    uri: uri,
                                    cancellationToken: cancellationToken,
                                    createItem: (csv) =>
                                    {
                                        return new StockPrice(
                                                    Date: csv.GetField<DateTime>(0),
                                                    Open: csv.GetDouble(1),
                                                    High: csv.GetDouble(2),
                                                    Low: csv.GetDouble(3),
                                                    Close: csv.GetDouble(4),
                                                    AdjClose: csv.GetDouble(5),
                                                    Volume: csv.GetLong(6));
                                    });
        }

        private async Task<List<StockPrice>> GetItems(string uri, Func<CsvReader, StockPrice> createItem, CancellationToken cancellationToken)
        {
            List<StockPrice> items = new();

            var response = await this.httpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                using var streamReader = new StreamReader(responseStream);
                using var csv = new CsvReader(streamReader, new CsvConfiguration(CultureInfo.InvariantCulture));

                if (!cancellationToken.IsCancellationRequested && await csv.ReadAsync().ConfigureAwait(false))
                {
                    csv.ReadHeader();

                    while (!cancellationToken.IsCancellationRequested && await csv.ReadAsync().ConfigureAwait(false))
                    {
                        var newItem = createItem(csv);
                        if (newItem != null)
                        {
                            items.Add(newItem);
                        }
                    }
                }

                return items;
            }

            this.logger.LogWarning("Failed HTTP status code: {0} - {1}\n  URL: {2}", response.StatusCode, response.ReasonPhrase, uri);
            return null;
        }
    }
}