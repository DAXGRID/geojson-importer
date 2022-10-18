# GeoJSON Importer

## Data requirement

The only takes in data in the `GeoJSON` format. It handles both features with and without `geometries`.

## Testing

### Running test suite

```sh
dotnet test
```

#### Unit testing

```sh
dotnet test --filter Category=Unit
```

#### Integration testing

```sh
dotnet test --filter Category=Integration
```

Running the integration tests requires a running instance of MS-SQL.

```sh
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=myAwesomePassword1" -e "MSSQL_AGENT_ENABLED=True"  -p 1433:1433 -d  mcr.microsoft.com/mssql/server:2019-CU13-ubuntu-20.04
```
