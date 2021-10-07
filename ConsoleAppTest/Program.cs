namespace ConsoleAppTest
{
    using System.Threading.Tasks;
    using Hilres.FinanceClient.Tiingo;
    using Hilres.FinanceClient.Yahoo;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Serilog;
    using Z015.BackgroundTask;
    using Z015.Repository;
    using Z015.Repository.UpdateBackground;

    internal static class Program
    {
        /// <summary>
        /// Start of main program.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>Task.</returns>
        private static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                .ConfigureSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    var connectionString = hostContext.Configuration.GetConnectionString("SqlDatabase");

                    services.AddPooledDbContextFactory<RepositoryDbContext>(options => options.UseSqlServer(connectionString));
                    services.AddTiingoService();
                    services.AddYahooService();
                    services.AddUpdateBackgroundService();
                    services.AddBackgroundTaskService();
                })
                .RunConsoleAsync();

            Log.Information("Program exiting.");
            Log.CloseAndFlush();
        }

        private static IHostBuilder ConfigureSerilog(this IHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
                 {
                     Log.Logger = new LoggerConfiguration()
                                 .ReadFrom.Configuration(context.Configuration)
                                 .CreateLogger();
                 })
                .UseSerilog();

            Log.Information("Program Starting.");

            return builder;
        }
    }
}