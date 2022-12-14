using GeoJsonImporter.SqlServer;
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

        if (startup is not null)
        {
            await startup.StartAsync().ConfigureAwait(false);
        }
        else
        {
            throw new ArgumentNullException(nameof(startup));
        }
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .WriteTo.Console()
            .CreateLogger();

        var settingsJson = JsonDocument.Parse(File.ReadAllText("appsettings.json"))
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
           .AddSingleton<IDatafordelerDatabase, SqlServerDatafordelerDatabase>()
           .BuildServiceProvider();
    }
}
