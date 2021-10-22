// <copyright file="Program.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.Website
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
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
            SetupSerilog();
            Log.Information("Program Started.");

            try
            {
                await Host.CreateDefaultBuilder(args)
                    .UseSerilog()
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

        private static void SetupSerilog()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }
    }
}