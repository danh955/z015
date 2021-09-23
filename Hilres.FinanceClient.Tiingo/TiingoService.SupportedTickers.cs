// <copyright file="TiingoService.SupportedTickers.cs" company="None">
// Free and open source code.
// </copyright>

namespace Hilres.FinanceClient.Tiingo
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Threading;
    using System.Threading.Tasks;
    using CsvHelper;
    using CsvHelper.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Tiingo service supported tickers class.
    /// </summary>
    public partial class TiingoService
    {
        /// <summary>
        /// Get supported tickers.
        /// </summary>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>List of supported stock tickers.  Successful if error message is null.</returns>
        public async Task<(IReadOnlyList<TiingoSupportedStockTicker> Stocks, string ErrorMessage)> GetSupportedTickersAsync(CancellationToken cancellationToken)
        {
            this.logger.LogDebug("GetSupportedTickersAsync");

            string uri = "https://apimedia.tiingo.com/docs/tiingo/daily/supported_tickers.zip";

            return await this.GetZipCsvItems<TiingoSupportedStockTicker>(
                                    uri: uri,
                                    cancellationToken: cancellationToken,
                                    createItem: (csv) =>
                                    {
                                        try
                                        {
                                            return new(
                                                       Ticker: csv[0],
                                                       Exchange: csv[1],
                                                       AssetType: csv[2],
                                                       PriceCurrency: csv[3],
                                                       StartDate: csv.GetField<DateTime?>(4),
                                                       EndDate: csv.GetField<DateTime?>(5));
                                        }
                                        catch (FormatException e)
                                        {
                                            this.logger.LogError($"{e.Message}  CSV[{csv.Parser.RawRow}] = {csv.Parser.RawRecord}");
                                            throw;
                                        }
                                    });
        }

        private async Task<(List<T> Items, string ErrorMessage)> GetZipCsvItems<T>(string uri, Func<CsvReader, T> createItem, CancellationToken cancellationToken)
        {
            var response = await this.httpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                List<T> items = new();

                using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                using var zip = new ZipArchive(responseStream);

                foreach (ZipArchiveEntry item in zip.Entries)
                {
                    if (item.Name.EndsWith(".csv"))
                    {
                        using var streamReader = new StreamReader(item.Open());
                        using var csv = new CsvReader(streamReader, new CsvConfiguration(CultureInfo.InvariantCulture));

                        if (!cancellationToken.IsCancellationRequested
                            && await csv.ReadAsync().ConfigureAwait(false))
                        {
                            csv.ReadHeader();

                            while (!cancellationToken.IsCancellationRequested
                                && await csv.ReadAsync().ConfigureAwait(false))
                            {
                                var newItem = createItem(csv);
                                if (newItem != null)
                                {
                                    items.Add(newItem);
                                }
                            }
                        }
                    }
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    this.logger.LogInformation($"{nameof(this.GetZipCsvItems)} canceled.");
                    return (items, "Canceled");
                }

                return (items, null);
            }

            this.logger.LogWarning($"Failed HTTP status code: {response.StatusCode} - {response.ReasonPhrase}\n  URL: {uri}");
            return (null, $"{response.StatusCode}");
        }
    }
}