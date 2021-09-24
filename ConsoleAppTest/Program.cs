namespace ConsoleAppTest
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Hilres.FinanceClient.Yahoo;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Serilog.Extensions.Logging;

    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Stopwatch stopWatch = new();

            using var log = new LoggerConfiguration()
                                .MinimumLevel.Debug()
                                .WriteTo.Console()
                                .CreateLogger();

            var loggerFactory = new SerilogLoggerFactory(log).AddSerilog();
            ILogger<YahooService> logger = loggerFactory.CreateLogger<YahooService>();

            log.Information("Starting {0}", args);

            stopWatch.Start();
            var service = new YahooService(logger);
            var result1 = await service.GetStockPricesAsync("QQQ", new(2021, 3, 9), new(2021, 3, 14), YahooInterval.Daily, CancellationToken.None);
            var result2 = await service.GetStockPricesAsync("QQQ", new(2020, 12, 14), new(2021, 3, 14), YahooInterval.Weekly, CancellationToken.None);
            var result3 = await service.GetStockPricesAsync("QQQ", new(2020, 10, 1), new(2021, 3, 14), YahooInterval.Monthly, CancellationToken.None);
            stopWatch.Stop();

            TimeSpan ts = stopWatch.Elapsed;
            log.Information($"RunTime {ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}");
            ////log.Information($"RunTime {ts:H:mm:ss.ff}");
        }
    }
}