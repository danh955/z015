namespace ConsoleAppTest
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Serilog;
    using Z015.BackgroundTask;
    using Z015.Repository;

    internal static class Program
    {
        /// <summary>
        /// Start of main program.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>Task.</returns>
        private static async Task Main(string[] args)
        {
            try
            {
                await Host.CreateDefaultBuilder(args)
                    .ConfigureSerilog()
                    .ConfigureServices((hostContext, services) =>
                    {
                        var connectionString = hostContext.Configuration.GetConnectionString("SqlDatabase");

                        services.AddPooledDbContextFactory<RepositoryDbContext>(options => options.UseSqlServer(connectionString));
                        services.AddBackgroundTaskService(hostContext.Configuration);
                    })
                    .RunConsoleAsync();
            }
            catch (Exception e)
            {
                Log.Error(e, "Fatal error.");
            }
            finally
            {
                Log.Information("Program exiting.");
                Log.CloseAndFlush();
            }
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