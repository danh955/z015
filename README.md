# z015


#### Putting Serilog in CreateDefaultBuilder
```c
await Host.CreateDefaultBuilder(args)
    .ConfigureSerilog()
    /* more code*/
    .RunConsoleAsync();

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
```

