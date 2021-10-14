// <copyright file="YahooService.Historical.cs" company="None">
// Free and open source code.
// </copyright>

namespace Hilres.FinanceClient.Yahoo
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using CsvHelper;
    using CsvHelper.Configuration;
    using Hilres.FinanceClient.Abstraction;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Yahoo finance service for historical data class.
    /// </summary>
    public partial class YahooService
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
        public async Task<PriceListResult> GetStockPricesAsync(string symbol, DateTime? firstDate, DateTime? lastDate, YahooInterval? interval, CancellationToken cancellationToken)
        {
            this.logger.LogDebug("GetStockPricesAsync symbol={0}, firstDate={1}, lastDate={2}, interval={3}", symbol, firstDate, lastDate, interval);

            string period1 = firstDate.HasValue ? firstDate.Value.ToUnixTimestamp() : Constant.EpochString;
            string period2 = lastDate.HasValue ? lastDate.Value.ToUnixTimestamp() : DateTime.Today.ToUnixTimestamp();
            string intervalString = ToIntervalString(interval);

            if (this.crumb == null)
            {
                await this.RefreshCookieAndCrumbAsync(symbol, cancellationToken);
            }

            PriceListResult result = null;

            int tryCount = 3;
            while (!cancellationToken.IsCancellationRequested && tryCount > 0)
            {
                string uri = $"https://query1.finance.yahoo.com/v7/finance/download/{symbol}?period1={period1}&period2={period2}&interval={intervalString}&events=history&includeAdjustedClose=true&crumb={this.crumb}";

                result = await this.GetCsvItems(
                                        uri: uri,
                                        cancellationToken: cancellationToken,
                                        createItem: (csv) =>
                                        {
                                            try
                                            {
                                                return new YahooPrice(
                                                            Date: csv.GetField<DateTime>(0),
                                                            Open: csv.GetDouble(1),
                                                            High: csv.GetDouble(2),
                                                            Low: csv.GetDouble(3),
                                                            Close: csv.GetDouble(4),
                                                            AdjClose: csv.GetDouble(5),
                                                            Volume: csv.GetLong(6));
                                            }
                                            catch (FormatException e)
                                            {
                                                this.logger.LogError($"{e.Message}  CSV[{csv.Parser.RawRow}] = {csv.Parser.RawRecord}");
                                                return null;
                                            }
                                            catch (BadDataException e)
                                            {
                                                this.logger.LogError($"{e.Message}  CSV[{csv.Parser.RawRow}] = {csv.Parser.RawRecord}");
                                                return null;
                                            }
                                        });

                if (result.IsSuccessful || result.ErrorMessage != HttpStatusCode.Unauthorized.ToString())
                {
                    return result;
                }

                await this.RefreshCookieAndCrumbAsync(symbol, cancellationToken);
                tryCount--;
            }

            return new(false, result.Prices, $"Error: Too many retries. {result.ErrorMessage}");
        }

        private async Task<PriceListResult> GetCsvItems(string uri, Func<CsvReader, YahooPrice> createItem, CancellationToken cancellationToken)
        {
            List<YahooPrice> items = new();

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

                if (cancellationToken.IsCancellationRequested)
                {
                    this.logger.LogInformation($"{nameof(this.GetCsvItems)} canceled.");
                    return new(false, items, "Canceled");
                }

                return new(true, items, null);
            }

            this.logger.LogWarning($"Failed HTTP status code: {response.StatusCode} - {response.ReasonPhrase}\n  URL: {uri}");
            return new(false, null, response.StatusCode.ToString());
        }
    }
}