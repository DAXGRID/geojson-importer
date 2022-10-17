using Microsoft.Extensions.Logging;

namespace DatafordelerUtil;

internal class Startup
{
    private readonly ILogger<Startup> _logger;
    private readonly Settings _settings;
    private readonly IDatafordelerDatabase _datafordelerDatabase;
    private const int BulkCount = 100000;

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

            var exampleFeature = StreamGeoJson.FirstGeoJsonFeature(import.FilePath);

            var primaryTableDescription = DynamicTableDescriptionFactory.Create(
                import.SchemaName,
                import.TableName,
                import.KeyFieldName,
                exampleFeature);

            var primaryTableExists = await _datafordelerDatabase.TableExists(
                import.TableName, import.SchemaName).ConfigureAwait(false);

            if (!primaryTableExists)
            {
                _logger.LogInformation("Creating table {TableName}.", import.TableName);
                await _datafordelerDatabase.CreateTable(primaryTableDescription)
                    .ConfigureAwait(false);
            }

            var temporaryTableName = $"{import.TableName}_tmp";
            var temporaryTableDescription = DynamicTableDescriptionFactory.Create(
                import.SchemaName,
                temporaryTableName,
                import.KeyFieldName,
                exampleFeature);

            var temporaryTableExists = await _datafordelerDatabase
                .TableExists(temporaryTableDescription.Name,
                             temporaryTableDescription.Schema)
                .ConfigureAwait(false);

            if (temporaryTableExists)
            {
                _logger.LogInformation("Deleting table {TableName}.", temporaryTableName);
                await _datafordelerDatabase
                    .DeleteTable(
                        temporaryTableDescription.Name,
                        temporaryTableDescription.Schema)
                    .ConfigureAwait(false);
            }

            _logger.LogInformation("Creating table {TableName}.", temporaryTableName);
            await _datafordelerDatabase.CreateTable(temporaryTableDescription)
                .ConfigureAwait(false);

            var features = new List<GeoJsonFeature>();
            foreach (var feature in StreamGeoJson.StreamFeaturesFile(import.FilePath))
            {
                if (features.Count == BulkCount)
                {
                    _logger.LogInformation(
                        "Bulk inserting {Count} into {TableName}",
                        features.Count,
                        temporaryTableName);

                    await _datafordelerDatabase
                        .BulkImportGeoJsonFeatures(
                            temporaryTableName,
                            features,
                            temporaryTableDescription.Schema)
                        .ConfigureAwait(false);

                    features.Clear();
                }
                else
                {
                    features.Add(feature);
                }
            }

            // Insert the remaining features.
            if (features.Any())
            {
                _logger.LogInformation(
                    "Bulk inserting {Count} into {TableName}",
                    features.Count,
                    temporaryTableName);

                await _datafordelerDatabase
                    .BulkImportGeoJsonFeatures(
                        temporaryTableName,
                        features,
                        temporaryTableDescription.Schema)
                    .ConfigureAwait(false);
            }

            _logger.LogInformation(
                "Merging {Target} from {Source}.",
                primaryTableDescription.Name,
                temporaryTableDescription.Name);

            await _datafordelerDatabase
                .Merge(primaryTableDescription, temporaryTableDescription)
                .ConfigureAwait(false);

            _logger.LogInformation(
                "Deleting temporary table {TableName}",
                temporaryTableDescription.Name);

            await _datafordelerDatabase
                .DeleteTable(temporaryTableDescription.Name,
                             temporaryTableDescription.Schema)
                .ConfigureAwait(false);
        }
    }
}
