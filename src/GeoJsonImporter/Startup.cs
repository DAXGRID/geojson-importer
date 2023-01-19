using Microsoft.Extensions.Logging;

namespace GeoJsonImporter;

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
        try
        {
            foreach (var import in _settings.Imports)
            {
                _logger.LogInformation("Starting import of {TableName}.", import.TableName);

                var primaryTableDescription = import.EntireDatasetPropertyScan
                    ? DynamicTableDescriptionFactory.Create(
                        import.SchemaName,
                        import.TableName,
                        import.KeyFieldName,
                        StreamGeoJson.StreamFeaturesFile(import.FilePath),
                        import.FieldNameMappings)
                    : DynamicTableDescriptionFactory.Create(
                        import.SchemaName,
                        import.TableName,
                        import.KeyFieldName,
                        StreamGeoJson.FirstGeoJsonFeature(import.FilePath),
                        import.FieldNameMappings);

                var temporaryTableDescription = primaryTableDescription with
                {
                    Name = $"{import.TableName}_tmp"
                };

                var primaryTableExists = await _datafordelerDatabase.TableExists(
                    import.TableName, import.SchemaName).ConfigureAwait(false);

                if (!primaryTableExists)
                {
                    _logger.LogInformation("Creating table {TableName}.", import.TableName);
                    await _datafordelerDatabase.CreateTable(primaryTableDescription)
                        .ConfigureAwait(false);
                }

                var temporaryTableExists = await _datafordelerDatabase
                    .TableExists(temporaryTableDescription.Name,
                                 temporaryTableDescription.Schema)
                    .ConfigureAwait(false);

                if (temporaryTableExists)
                {
                    _logger.LogInformation(
                        "Deleting table {TableName}.",
                        temporaryTableDescription.Name);

                    await _datafordelerDatabase
                        .DeleteTable(
                            temporaryTableDescription.Name,
                            temporaryTableDescription.Schema)
                        .ConfigureAwait(false);
                }

                _logger.LogInformation(
                    "Creating table {TableName}.",
                    temporaryTableDescription.Name);

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
                            temporaryTableDescription.Name);

                        await _datafordelerDatabase
                            .BulkImportGeoJsonFeatures(
                                temporaryTableDescription,
                                features,
                                import.FieldNameMappings)
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
                        temporaryTableDescription.Name);

                    await _datafordelerDatabase
                        .BulkImportGeoJsonFeatures(
                            temporaryTableDescription,
                            features,
                            import.FieldNameMappings)
                        .ConfigureAwait(false);
                }

                _logger.LogInformation(
                    "Merging into {Target} from {Source}.",
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

                var hasGeometry = primaryTableDescription
                    .Columns
                    .Where(x => x.ColumnType == ColumnType.Geometry)
                    .Any();

                if (!string.IsNullOrWhiteSpace(_settings.SpartialIndexStatement) && hasGeometry)
                {
                    var indexExists = await _datafordelerDatabase
                        .IndexExists(
                            "coord_sidx",
                            primaryTableDescription.Name,
                            primaryTableDescription.Schema)
                        .ConfigureAwait(false);

                    if (indexExists)
                    {
                        _logger.LogInformation(
                            "Index already exists for {TableName}, skipping creation.",
                            primaryTableDescription.Name);
                    }
                    else
                    {
                        _logger.LogInformation(
                                "Creating spatial index for {TableName}.",
                                primaryTableDescription.Name);

                        await _datafordelerDatabase
                            .CreateSpatialIndex(
                                primaryTableDescription.Name,
                                primaryTableDescription.Schema)
                            .ConfigureAwait(false);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("{Exception}", ex);
            throw;
        }
    }
}
