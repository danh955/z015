// <copyright file="YahooServiceHistoricalTest.cs" company="None">
// Free and open source code.
// </copyright>

namespace Hilres.FinanceClient.Yahoo.Test
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Hilres.FinanceClient.Abstraction;
    using Hilres.FinanceClient.Yahoo;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Yahoo finance service for historical test class.
    /// </summary>
    public class YahooServiceHistoricalTest
    {
        private readonly ITestOutputHelper loggerHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="YahooServiceHistoricalTest"/> class.
        /// </summary>
        /// <param name="loggerHelper">ITestOutputHelper.</param>
        public YahooServiceHistoricalTest(ITestOutputHelper loggerHelper)
        {
            this.loggerHelper = loggerHelper;
        }

        /// <summary>
        /// Get stock price with invalid symbol test.
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task GetStockPricesAsyncInvalidSymbolTest()
        {
            var logger = this.loggerHelper.BuildLoggerFor<YahooService>();

            var service = new YahooService(logger);
            var (isSuccessful, prices, errorMessage) = await service.GetStockPricesAsync("QQQzzz", null, null, YahooInterval.Quorterly, CancellationToken.None);

            Assert.False(isSuccessful);
            Assert.Null(prices);
            Assert.Equal("NotFound", errorMessage);
        }

        /// <summary>
        /// Get stock price with null dates test.
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task GetStockPricesAsyncNullDatesTest()
        {
            var logger = this.loggerHelper.BuildLoggerFor<YahooService>();

            var service = new YahooService(logger);
            var (isSuccessful, prices, errorMessage) = await service.GetStockPricesAsync("QQQ", null, null, YahooInterval.Quorterly, CancellationToken.None);

            Assert.True(isSuccessful);
            Assert.NotNull(prices);
            Assert.Null(errorMessage);
            Assert.True(prices.Any());
        }

        /// <summary>
        /// Get stock price test.
        /// </summary>
        /// <param name="symbol">Stock symbol.</param>
        /// <param name="firstDate">The first date.</param>
        /// <param name="lastDate">The last date.</param>
        /// <param name="interval">Interval of the data.</param>
        /// <param name="count">Expected number of rows returned.</param>
        /// <returns>Task.</returns>
        [Theory]
        [InlineData("QQQ", "3/9/2021", "3/14/2021", YahooInterval.Daily, 4)]
        [InlineData("QQQ", "12/14/2020", "3/14/2021", YahooInterval.Weekly, 13)]
        [InlineData("QQQ", "10/1/2020", "3/14/2021", YahooInterval.Monthly, 6)]
        public async Task GetStockPricesAsyncTest(string symbol, string firstDate, string lastDate, YahooInterval interval, int count)
        {
            var logger = this.loggerHelper.BuildLoggerFor<YahooService>();

            var service = new YahooService(logger);
            var (isSuccessful, prices, errorMessage) = await service.GetStockPricesAsync(symbol, DateTime.Parse(firstDate), DateTime.Parse(lastDate), interval, CancellationToken.None);

            Assert.True(isSuccessful);
            Assert.NotNull(prices);
            Assert.Null(errorMessage);
            Assert.True(prices.Any());
            Assert.Equal(count, prices.Count);
        }
    }
}