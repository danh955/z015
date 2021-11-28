// <copyright file="YahooService.cs" company="None">
// Free and open source code.
// </copyright>

namespace Hilres.FinanceClient.Yahoo
{
    using System;
    using System.Linq;
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
        private readonly HttpClient crumbHttpClient;
        private readonly HttpClient httpClient;
        private readonly ILogger<YahooService> logger;

        private readonly Regex regexCrumb = new(
                                    "\"CrumbStore\":{\"crumb\":\"(?<crumb>.+?)\"}",
                                    RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private string crumb;

        /// <summary>
        /// Initializes a new instance of the <see cref="YahooService"/> class.
        /// </summary>
        /// <param name="logger">ILogger.</param>
        public YahooService(ILogger<YahooService> logger)
        {
            const string UserAgent = "user-agent";
            const string UserAgentText = "Mozilla/5.0 (X11; U; Linux i686) Gecko/20071127 Firefox/2.0.0.11";

            this.logger = logger;
            this.httpClient = new(); // With Set-Cookie.
            this.httpClient.DefaultRequestHeaders.Add(UserAgent, UserAgentText);
            this.crumbHttpClient = new();  // No Set-Cookie.
            this.crumbHttpClient.DefaultRequestHeaders.Add(UserAgent, UserAgentText);
        }

        /// <summary>
        /// Gets or sets the delay between API request to Yahoo in milliseconds.
        /// </summary>
        public int RequestDelay { get; set; } = 250;

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
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>True if successful.</returns>
        public async Task RefreshCookieAndCrumbAsync(CancellationToken cancellationToken)
        {
            const int maxTryCount = 5;
            int tryCount = 0;

            var (cookie, crumb) = await this.TryGetCookieAndCrumbAsync();

            while (crumb == null && !cancellationToken.IsCancellationRequested && tryCount < maxTryCount)
            {
                await Task.Delay(1000, cancellationToken);
                (cookie, crumb) = await this.TryGetCookieAndCrumbAsync();
                tryCount++;
            }

            this.logger.LogInformation("Got cookie. tryCount = {TryCount}, Crumb = {Crumb}, Cookie = {Cookie}", tryCount, crumb, cookie);
            this.httpClient.DefaultRequestHeaders.Add(HttpRequestHeader.Cookie.ToString(), cookie);
            this.crumb = crumb;
        }

        private static string GetCookie(HttpResponseMessage responseMessage, string cookieName)
        {
            var keyValue = responseMessage.Headers.SingleOrDefault(h => h.Key == cookieName);
            return keyValue.Value?.SingleOrDefault();
        }

        /// <summary>
        /// Try to get the cookie and crumb value.
        /// </summary>
        /// <returns>(cookie, crumb) if successful.  Otherwise (null, null) or (cookie, null).</returns>
        private async Task<(string Cookie, string Crumb)> TryGetCookieAndCrumbAsync()
        {
            const string uri = "https://finance.yahoo.com/quote/%5EGSPC";

            var response = await this.crumbHttpClient.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var cookie = GetCookie(response, "Set-Cookie");
                if (string.IsNullOrEmpty(cookie))
                {
                    this.logger.LogWarning("{Function}  Cookie not found", nameof(this.TryGetCookieAndCrumbAsync));
                    return (null, null);
                }

                var html = await response.Content.ReadAsStringAsync();
                var matches = this.regexCrumb.Matches(html);
                if (matches.Count != 1)
                {
                    this.logger.LogWarning("{Function}  Crumb not found", nameof(this.TryGetCookieAndCrumbAsync));
                    return (cookie, null);
                }

                var crumb = matches[0].Groups["crumb"]?.Value?.Replace("\\u002F", "/");
                if (string.IsNullOrWhiteSpace(crumb))
                {
                    this.logger.LogWarning("{Function}  Crumb is empty", nameof(this.TryGetCookieAndCrumbAsync));
                    return (cookie, null);
                }

                return (cookie, crumb);
            }

            return (null, null);
        }
    }
}