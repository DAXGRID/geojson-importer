# Datafordeler util

Example to generate the `GeoJSON` from an `gpkg` file using `ogr2ogr`.

```sh
ogr2ogr -f GeoJSON test.geojson MatrikelGeometriGaeldendeDKComplete.gpkg -sql 'SELECT * FROM jordstykke where sognekode = 7973'
```
