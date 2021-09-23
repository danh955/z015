// <copyright file="TiingoServiceSupportedTickersTest.cs" company="None">
// Free and open source code.
// </copyright>

namespace Hilres.FinanceClient.Tiingo.Test
{
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Tiingo service supported tickers test class.
    /// </summary>
    public class TiingoServiceSupportedTickersTest
    {
        private readonly ITestOutputHelper loggerHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TiingoServiceSupportedTickersTest"/> class.
        /// </summary>
        /// <param name="loggerHelper">ITestOutputHelper.</param>
        public TiingoServiceSupportedTickersTest(ITestOutputHelper loggerHelper)
        {
            this.loggerHelper = loggerHelper;
        }

        /// <summary>
        /// Get.
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task GetSupportedTickersAsyncTest()
        {
            var logger = this.loggerHelper.BuildLoggerFor<TiingoService>();

            var service = new TiingoService(logger);
            var (items, errorMessage) = await service.GetSupportedTickersAsync(CancellationToken.None);

            Assert.NotNull(items);
            Assert.Null(errorMessage);
        }
    }
}