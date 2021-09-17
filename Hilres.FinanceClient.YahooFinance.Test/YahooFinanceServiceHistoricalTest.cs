// <copyright file="YahooFinanceServiceHistoricalTest.cs" company="None">
// Free and open source code.
// </copyright>

namespace Hilres.FinanceClient.YahooFinance.Test
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Hilres.FinanceClient.Abstraction;
    using Hilres.FinanceClient.YahooFinance;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Yahoo finance service for historical test class.
    /// </summary>
    public class YahooFinanceServiceHistoricalTest
    {
        private readonly ITestOutputHelper loggerHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="YahooFinanceServiceHistoricalTest"/> class.
        /// </summary>
        /// <param name="loggerHelper">ITestOutputHelper.</param>
        public YahooFinanceServiceHistoricalTest(ITestOutputHelper loggerHelper)
        {
            this.loggerHelper = loggerHelper;
        }

        /// <summary>
        /// Handler test.
        /// </summary>
        /// <param name="symbol">Stock symbol.</param>
        /// <param name="firstDate">The first date.</param>
        /// <param name="lastDate">The last date.</param>
        /// <param name="interval">Interval of the data.</param>
        /// <param name="count">Expected number of rows returned.</param>
        /// <returns>Task.</returns>
        [Theory]
        [InlineData("QQQ", "3/9/2021", "3/14/2021", StockInterval.Daily, 4)]
        [InlineData("QQQ", "12/14/2020", "3/14/2021", StockInterval.Weekly, 13)]
        [InlineData("QQQ", "10/1/2020", "3/14/2021", StockInterval.Monthly, 6)]
        public async Task GetStockPricesAsyncTest(string symbol, string firstDate, string lastDate, StockInterval interval, int count)
        {
            var logger = this.loggerHelper.BuildLoggerFor<YahooFinanceService>();

            var service = new YahooFinanceService(logger);
            var result = await service.GetStockPricesAsync(symbol, DateTime.Parse(firstDate), DateTime.Parse(lastDate), interval, CancellationToken.None);

            Assert.NotNull(result);
            Assert.True(result.Any());
            Assert.Equal(count, result.Count);
        }
    }
}