// <copyright file="YahooService.cs" company="None">
// Free and open source code.
// </copyright>

namespace Hilres.FinanceClient.Yahoo
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Hilres.FinanceClient.Abstraction;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Yahoo finance service class.
    /// </summary>
    public partial class YahooService : IYahooService
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<YahooService> logger;

        private readonly Regex regexCrumb = new(
                                    "CrumbStore\":{\"crumb\":\"(?<crumb>.+?)\"}",
                                    RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private string crumb;

        /// <summary>
        /// Initializes a new instance of the <see cref="YahooService"/> class.
        /// </summary>
        /// <param name="logger">ILogger.</param>
        public YahooService(ILogger<YahooService> logger)
        {
            this.logger = logger;
            this.httpClient = new();
        }

        /// <summary>
        /// Convert the interval into a string.
        /// </summary>
        /// <param name="interval">StockInterval.</param>
        /// <returns>string.</returns>
        public static string ToIntervalString(YahooInterval? interval) => interval switch
        {
            null => "1d",
            YahooInterval.Daily => "1d",
            YahooInterval.Weekly => "1wk",
            YahooInterval.Monthly => "1mo",
            YahooInterval.Quorterly => "3mo",
            _ => throw new NotImplementedException(interval.ToString()),
        };

        /// <summary>
        /// Get the cookie and crumb value.
        /// </summary>
        /// <param name="symbol">Stock ticker symbol.</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>True if successful.</returns>
        public async Task RefreshCookieAndCrumbAsync(string symbol, CancellationToken cancellationToken)
        {
            int tryCount = 5;
            var (cookie, crumb) = await this.TryGetCookieAndCrumbAsync(symbol);

            while (cookie == null && !cancellationToken.IsCancellationRequested && tryCount > 0)
            {
                await Task.Delay(1000, cancellationToken);
                (cookie, crumb) = await this.TryGetCookieAndCrumbAsync(symbol);
                tryCount--;
            }

            this.logger.LogInformation("Got the cookie. tryCount = {0}", tryCount);
            this.httpClient.DefaultRequestHeaders.Add(HttpRequestHeader.Cookie.ToString(), cookie);
            this.crumb = crumb;
        }

        /// <summary>
        /// Try to get the cookie and crumb value.
        /// </summary>
        /// <param name="symbol">Stock ticker symbol.</param>
        /// <returns>(cookie, crumb) if successful.  Otherwise (null, null).</returns>
        private async Task<(string Cookie, string Crumb)> TryGetCookieAndCrumbAsync(string symbol)
        {
            this.logger.LogDebug("TryGetCookieAndCrumbAsync");

            try
            {
                var url = $"https://finance.yahoo.com/quote/{symbol}?p={symbol}";

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.CookieContainer = new CookieContainer();
                request.Method = "GET";

                using var response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false);
                using var stream = response.GetResponseStream();
                string html = await new StreamReader(stream).ReadToEndAsync().ConfigureAwait(false);

                if (html.Length < 5000)
                {
                    return (null, null);
                }

                var crumb = this.ParseCrumb(html);
                var cookie = response.GetResponseHeader("Set-Cookie").Split(';')[0];

                if (cookie != null && crumb != null)
                {
                    this.logger.LogDebug("Cookie: '{1}', Crumb: '{0}'", cookie, crumb);
                    return (cookie, crumb);
                }

                if (html.Contains("No results for"))
                {
                    this.logger.LogDebug("Cookie: '{1}', Crumb: Invalid symbol", cookie);
                    return (cookie, null);
                }
            }
            catch (Exception e)
            {
                this.logger.LogWarning(e.Message);
            }

            return (null, null);
        }

        /// <summary>
        /// Get crumb value from HTML.
        /// </summary>
        /// <param name="html">HTML code.</param>
        /// <returns>Crumb.</returns>
        private string ParseCrumb(string html)
        {
            string crumb = null;

            try
            {
                var matches = this.regexCrumb.Matches(html);

                if (matches.Count > 0)
                {
                    crumb = matches[0].Groups["crumb"].Value;

                    // fixed Unicode character 'SOLIDUS'
                    if (crumb.Length != 11)
                    {
                        crumb = crumb.Replace("\\u002F", "/");
                    }
                }
                else
                {
                    this.logger.LogWarning("RegEx no match");
                }
            }
            catch (Exception e)
            {
                this.logger.LogWarning(e, e.Message);
            }

            return crumb;
        }
    }
}