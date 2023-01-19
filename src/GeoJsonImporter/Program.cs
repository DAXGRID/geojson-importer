using GeoJsonImporter.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using System.Text.Json;

namespace GeoJsonImporter;

internal sealed class Program
{
    public static async Task Main()
    {
        using var serviceProvider = BuildServiceProvider();
        var startup = serviceProvider.GetService<Startup>();

        await startup!.StartAsync().ConfigureAwait(false);
    }

    private static ServiceProvider BuildServiceProvider()
    {
        const string APP_SETTINGS_FILE_NAME = "appsettings.json";

        var loggingConfiguration = new ConfigurationBuilder()
            .AddJsonFile(APP_SETTINGS_FILE_NAME)
            .Build();

        var logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .ReadFrom.Configuration(loggingConfiguration)
            .Enrich.FromLogContext()
            .CreateLogger();

        var settingsJson = JsonDocument.Parse(File.ReadAllText(APP_SETTINGS_FILE_NAME))
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
