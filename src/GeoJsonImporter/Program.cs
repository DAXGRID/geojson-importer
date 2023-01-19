using GeoJsonImporter.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System.Text.Json;

namespace GeoJsonImporter;

internal sealed class Program
{
    public static async Task Main(string[] args)
    {
        var appSettingsFilePath = "appsettings.json";

        if (args.Length == 1)
        {
            appSettingsFilePath = args[0];
        }

        using var serviceProvider = BuildServiceProvider(appSettingsFilePath);
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        var logger = loggerFactory!.CreateLogger(nameof(Program));
        var startup = serviceProvider.GetService<Startup>();

        try
        {
            await startup!.StartAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            logger.LogError("{Exception}", ex);
            throw;
        }
    }

    private static ServiceProvider BuildServiceProvider(string appSettingsFilePath)
    {
        var loggingConfiguration = new ConfigurationBuilder()
            .AddJsonFile(appSettingsFilePath)
            .Build();

        var logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .ReadFrom.Configuration(loggingConfiguration)
            .Enrich.FromLogContext()
            .CreateLogger();

        var settingsJson = JsonDocument.Parse(File.ReadAllText(appSettingsFilePath))
            .RootElement.GetProperty("settings").ToString();

        var settings = JsonSerializer.Deserialize<Settings>(settingsJson) ??
            throw new ArgumentException("Could not deserialize appsettings into settings.");

        return new ServiceCollection()
           .AddLogging(logging =>
           {
               logging.AddSerilog(logger, true);
           })
           .AddSingleton<Settings>(settings)
           .AddSingleton<Startup>()
           .AddSingleton<IDatafordelerDatabase, SqlServerDatabase>()
           .BuildServiceProvider();
    }
}
