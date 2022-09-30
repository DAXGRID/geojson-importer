using Microsoft.Extensions.Logging;

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
        foreach (var import in _settings.Imports)
        {
            _logger.LogInformation("Starting import of {TableName}.", import.TableName);

            var exampleFeature = StreamGeoJson.StreamFeaturesFile(import.FilePath).First();
            var tableDescription = DynamicTableDescriptionFactory.Create(
                import.SchemaName, import.TableName, exampleFeature);

            var tableExists = await _datafordelerDatabase.TableExists(
                import.TableName, import.SchemaName).ConfigureAwait(false);
            if (!tableExists)
            {
                _logger.LogInformation("Creating table {TableName}.", import.TableName);
                await _datafordelerDatabase.CreateTable(tableDescription)
                    .ConfigureAwait(false);
            }
        }
    }
}
