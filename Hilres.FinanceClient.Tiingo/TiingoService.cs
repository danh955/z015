// <copyright file="TiingoService.cs" company="None">
// Free and open source code.
// </copyright>

namespace Hilres.FinanceClient.Tiingo
{
    using System.Net.Http;
    using Hilres.FinanceClient.Abstraction;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Tiingo finance service class.
    /// </summary>
    public partial class TiingoService : ITiingoService
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<TiingoService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TiingoService"/> class.
        /// </summary>
        /// <param name="logger">ILogger.</param>
        public TiingoService(ILogger<TiingoService> logger)
        {
            this.logger = logger;
            this.httpClient = new();
        }
    }
}