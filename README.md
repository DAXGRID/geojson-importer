# GeoJSON Importer

Takes GeoJSON and imports it into whatever is supported. :) It handles features with and without `geometries`.

Currently we only support Microsoft SQL-server, but PostgreSQL is on the drawing board.

## Note

By default it looks for the `appsettings.json` file in same folder as the binary file. This can be overwritten by calling the binary file with a full path to the file.

Example:

```sh
./my_executeable "/home/my_user/appsettings.json"
```

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
