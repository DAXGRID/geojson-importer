using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace DatafordelerUtil;

internal class Startup
{
    private readonly ILogger<Startup> _logger;
    private readonly Settings _settings;
    private readonly IDatafordelerDatabase _datafordelerDatabase;

    public Startup(
        ILogger<Startup> logger,
        Settings settings,
        IDatafordelerDatabase datafordelerDatabase)
    {
        _logger = logger;
        _settings = settings;
        _datafordelerDatabase = datafordelerDatabase;
    }

    public async Task StartAsync()
    {
        _logger.LogInformation("Starting {ServiceName}", nameof(Startup));

        if ((await _datafordelerDatabase.TableExists("", "dbo").ConfigureAwait(false)))
        {
            await _datafordelerDatabase.CreateTable(
                new("", new List<DynamicColumnDescription>())).ConfigureAwait(false);
        }

    }
}
