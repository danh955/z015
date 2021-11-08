// <copyright file="Program.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.Website
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using Serilog;

    /// <summary>
    /// Main program class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main program starts here.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>Task.</returns>
        public static async Task Main(string[] args)
        {
            try
            {
                await Host.CreateDefaultBuilder(args)
                    .ConfigureSerilog()
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>();
                    })
                    .Build()
                    .RunAsync();
            }
            catch (Exception e)
            {
                Log.Error(e, "Fatal error.");
            }
            finally
            {
                Log.Information("Program Ended.");
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