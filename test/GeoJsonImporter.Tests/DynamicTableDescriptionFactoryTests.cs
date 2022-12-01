using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;

namespace GeoJsonImporter.Tests;

public sealed class DynamicTableDescriptionFactoryTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Creates_dynamic_table_description_from_geojson_feature()
    {
        var geoJson = "{ \"type\": \"Polygon\", \"coordinates\": [ [ [ 9.6138793, 55.7433699, 0.0 ], [ 9.6128974, 55.7422721, 0.0 ], [ 9.6135094, 55.7420128, 0.0 ], [ 9.614114, 55.741748, 0.0 ], [ 9.614711, 55.7414779, 0.0 ], [ 9.6153002, 55.7412023, 0.0 ], [ 9.6163491, 55.7423193, 0.0 ], [ 9.6162263, 55.742377, 0.0 ], [ 9.6155584, 55.7426627, 0.0 ], [ 9.6152355, 55.7428005, 0.0 ], [ 9.6145223, 55.7431149, 0.0 ], [ 9.6139249, 55.743356, 0.0 ], [ 9.6138793, 55.7433699, 0.0 ] ] ] } }";

        Geometry geometry;
        var serializer = GeoJsonSerializer.Create();
        using (var stringReader = new StringReader(geoJson))
        using (var jsonReader = new JsonTextReader(stringReader))
        {
            geometry = serializer.Deserialize<Geometry>(jsonReader);
        }

        var geoJsonFeature = new GeoJsonFeature(
             type: "Feature",
             properties: new Dictionary<string, object?>
             {
                {"id", 4909206},
                {"forretningshaendelse", "Afledt ændring"},
                {"senestesaglokalid", "90000424"},
                {"forretningsproces", "Anden sagskategori"},
                {"id_namespace", "http,//data.gov.dk/Matriklen/Jordstykke"},
                {"lokalid", "100000870"},
                {"paataenkthandling", null},
                {"registreringfra", "2018-06-03T05,58,41.538609+02,00"},
                {"virkningfra", "2008-11-17T14,06,10.674000+01,00"},
                {"status", "Gældende"},
                {"virkningsaktoer", "Geodatastyrelsen"},
                {"samletfastejendomlokalid", "100001223"},
                {"skelforretningssagslokalid", "0"},
                {"stammerfrajordstykkelokalid", "2567340"},
                {"supplerendemaalingsaglokalid", "0"},
                {"arealberegningsmetode", "Areal beregnet efter opmåling - o"},
                {"arealbetegnelse", ""},
                {"arealtype", null},
                {"brugsretsareal", "0"},
                {"delnummer", null},
                {"faelleslod", "0"},
                {"matrikelnummer", "25s"},
                {"ejerlavskode", 1110252},
                {"sognekode", "7973"},
                {"kommunekode", "0630"},
                {"regionskode", "1083"},
                {"registreretareal", "26358"},
                {"vandarealinkludering", "ukendt"},
                {"vejareal", "0"},
                {"vejarealberegningsstatus", "vejareal beregnet"},
                {"fredskov_fredskovsareal", null},
                {"fredskov_omfang", null},
                {"jordrente_omfang", null},
                {"klitfredning_klitfredningsareal", null},
                {"klitfredning_omfang", null},
                {"majoratsskov_majoratsskovsnummer", null},
                {"majoratsskov_omfang", null},
                {"strandbeskyttelse_omfang", null},
                {"strandbeskyttelse_strandbeskyttelsesareal", null},
                {"stormfaldsnotering", "false"}
             },
             geometry: geometry
         );

        var expected = new DynamicTableDescription(
            schema: "dbo",
            name: "jordstykke",
            key: "id",
            columns: new List<DynamicColumnDescription>
            {
                new("id", ColumnType.Int, true),
                new("forretningshaendelse", ColumnType.String),
                new("senestesaglokalid", ColumnType.String),
                new("forretningsproces", ColumnType.String),
                new("id_namespace", ColumnType.String),
                new("lokalid", ColumnType.String),
                new("paataenkthandling", ColumnType.String),
                new("registreringfra", ColumnType.String),
                new("virkningfra", ColumnType.String),
                new("status", ColumnType.String),
                new("virkningsaktoer", ColumnType.String),
                new("samletfastejendomlokalid", ColumnType.String),
                new("skelforretningssagslokalid", ColumnType.String),
                new("stammerfrajordstykkelokalid", ColumnType.String),
                new("supplerendemaalingsaglokalid", ColumnType.String),
                new("arealberegningsmetode", ColumnType.String),
                new("arealbetegnelse", ColumnType.String),
                new("arealtype", ColumnType.String),
                new("brugsretsareal", ColumnType.String),
                new("delnummer", ColumnType.String),
                new("faelleslod", ColumnType.String),
                new("matrikelnummer", ColumnType.String),
                new("ejerlavskode", ColumnType.Int),
                new("sognekode", ColumnType.String),
                new("kommunekode", ColumnType.String),
                new("regionskode", ColumnType.String),
                new("registreretareal", ColumnType.String),
                new("vandarealinkludering", ColumnType.String),
                new("vejareal", ColumnType.String),
                new("vejarealberegningsstatus", ColumnType.String),
                new("fredskov_fredskovsareal", ColumnType.String),
                new("fredskov_omfang", ColumnType.String),
                new("jordrente_omfang", ColumnType.String),
                new("klitfredning_klitfredningsareal", ColumnType.String),
                new("klitfredning_omfang", ColumnType.String),
                new("majoratsskov_majoratsskovsnummer", ColumnType.String),
                new("majoratsskov_omfang", ColumnType.String),
                new("strandbeskyttelse_omfang", ColumnType.String),
                new("strandbeskyttelse_strandbeskyttelsesareal", ColumnType.String),
                new("stormfaldsnotering", ColumnType.String),
                new("coord", ColumnType.Geometry)
            });

        var result = DynamicTableDescriptionFactory.Create(
            "dbo",
            "jordstykke",
            "id",
            geoJsonFeature,
            new Dictionary<string, string>());

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Creates_dynamic_table_description_from_geojson_feature_with_mappings()
    {
        var geoJson = "{ \"type\": \"Polygon\", \"coordinates\": [ [ [ 9.6138793, 55.7433699, 0.0 ], [ 9.6128974, 55.7422721, 0.0 ], [ 9.6135094, 55.7420128, 0.0 ], [ 9.614114, 55.741748, 0.0 ], [ 9.614711, 55.7414779, 0.0 ], [ 9.6153002, 55.7412023, 0.0 ], [ 9.6163491, 55.7423193, 0.0 ], [ 9.6162263, 55.742377, 0.0 ], [ 9.6155584, 55.7426627, 0.0 ], [ 9.6152355, 55.7428005, 0.0 ], [ 9.6145223, 55.7431149, 0.0 ], [ 9.6139249, 55.743356, 0.0 ], [ 9.6138793, 55.7433699, 0.0 ] ] ] } }";

        Geometry geometry;
        var serializer = GeoJsonSerializer.Create();
        using (var stringReader = new StringReader(geoJson))
        using (var jsonReader = new JsonTextReader(stringReader))
        {
            geometry = serializer.Deserialize<Geometry>(jsonReader);
        }

        var geoJsonFeature = new GeoJsonFeature(
             type: "Feature",
             properties: new Dictionary<string, object?>
             {
                {"id", 4909206},
                {"forretningshaendelse", "Afledt ændring"},
                {"senestesaglokalid", "90000424"},
                {"forretningsproces", "Anden sagskategori"},
                {"id_namespace", "http,//data.gov.dk/Matriklen/Jordstykke"},
                {"lokalid", "100000870"},
                {"paataenkthandling", null},
                {"registreringfra", "2018-06-03T05,58,41.538609+02,00"},
                {"virkningfra", "2008-11-17T14,06,10.674000+01,00"},
                {"status", "Gældende"},
                {"virkningsaktoer", "Geodatastyrelsen"},
                {"samletfastejendomlokalid", "100001223"},
                {"skelforretningssagslokalid", "0"},
                {"stammerfrajordstykkelokalid", "2567340"},
                {"supplerendemaalingsaglokalid", "0"},
                {"arealberegningsmetode", "Areal beregnet efter opmåling - o"},
                {"arealbetegnelse", ""},
                {"arealtype", null},
                {"brugsretsareal", "0"},
                {"delnummer", null},
                {"faelleslod", "0"},
                {"matrikelnummer", "25s"},
                {"ejerlavskode", 1110252},
                {"sognekode", "7973"},
                {"kommunekode", "0630"},
                {"regionskode", "1083"},
                {"registreretareal", "26358"},
                {"vandarealinkludering", "ukendt"},
                {"vejareal", "0"},
                {"vejarealberegningsstatus", "vejareal beregnet"},
                {"fredskov_fredskovsareal", null},
                {"fredskov_omfang", null},
                {"jordrente_omfang", null},
                {"klitfredning_klitfredningsareal", null},
                {"klitfredning_omfang", null},
                {"majoratsskov_majoratsskovsnummer", null},
                {"majoratsskov_omfang", null},
                {"strandbeskyttelse_omfang", null},
                {"strandbeskyttelse_strandbeskyttelsesareal", null},
                {"stormfaldsnotering", "false"}
             },
             geometry: geometry
         );

        var expected = new DynamicTableDescription(
            schema: "dbo",
            name: "jordstykke",
            key: "id",
            columns: new List<DynamicColumnDescription>
            {
                new("id", ColumnType.Int, true),
                new("forretningshaendelse", ColumnType.String),
                new("senestesaglokalid", ColumnType.String),
                new("forretningsproces", ColumnType.String),
                new("id_namespace", ColumnType.String),
                new("lokalid", ColumnType.String),
                new("paataenkthandling", ColumnType.String),
                new("registreringfra", ColumnType.String),
                new("virkningfra", ColumnType.String),
                new("status", ColumnType.String),
                new("virkningsaktoer", ColumnType.String),
                new("samletfastejendomlokalid", ColumnType.String),
                new("skelforretningssagslokalid", ColumnType.String),
                new("stammerfrajordstykkelokalid", ColumnType.String),
                new("supplerendemaalingsaglokalid", ColumnType.String),
                new("arealberegningsmetode", ColumnType.String),
                new("arealbetegnelse", ColumnType.String),
                new("arealtype", ColumnType.String),
                new("brugsretsareal", ColumnType.String),
                new("delnummer", ColumnType.String),
                new("faelleslod", ColumnType.String),
                new("matrikelnummer", ColumnType.String),
                new("ejerlavskode", ColumnType.Int),
                new("sognekode", ColumnType.String),
                new("kommunekode", ColumnType.String),
                new("regionskode", ColumnType.String),
                new("registreretareal", ColumnType.String),
                new("vandarealinkludering", ColumnType.String),
                new("vejareal", ColumnType.String),
                new("vejarealberegningsstatus", ColumnType.String),
                new("fredskov_fredskovsareal", ColumnType.String),
                new("fredskov_omfang", ColumnType.String),
                new("jordrente_omfang", ColumnType.String),
                new("klitfredning_klitfredningsareal_test", ColumnType.String),
                new("klitfredning_omfang", ColumnType.String),
                new("majoratsskov_majoratsskovsnummer", ColumnType.String),
                new("majoratsskov_omfang", ColumnType.String),
                new("strandbeskyttelse_omfang_test", ColumnType.String),
                new("strandbeskyttelse_strandbeskyttelsesareal", ColumnType.String),
                new("stormfaldsnotering", ColumnType.String),
                new("coord", ColumnType.Geometry)
            });

        var result = DynamicTableDescriptionFactory.Create(
            "dbo",
            "jordstykke",
            "id",
            geoJsonFeature,
            new Dictionary<string, string>
            {
                {"klitfredning_klitfredningsareal", "klitfredning_klitfredningsareal_test"},
                {"strandbeskyttelse_omfang", "strandbeskyttelse_omfang_test"}
            }
        );

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Creates_dynamic_table_description_from_geojson_features_with_mappings()
    {
        var geoJson = "{ \"type\": \"Polygon\", \"coordinates\": [ [ [ 9.6138793, 55.7433699, 0.0 ], [ 9.6128974, 55.7422721, 0.0 ], [ 9.6135094, 55.7420128, 0.0 ], [ 9.614114, 55.741748, 0.0 ], [ 9.614711, 55.7414779, 0.0 ], [ 9.6153002, 55.7412023, 0.0 ], [ 9.6163491, 55.7423193, 0.0 ], [ 9.6162263, 55.742377, 0.0 ], [ 9.6155584, 55.7426627, 0.0 ], [ 9.6152355, 55.7428005, 0.0 ], [ 9.6145223, 55.7431149, 0.0 ], [ 9.6139249, 55.743356, 0.0 ], [ 9.6138793, 55.7433699, 0.0 ] ] ] } }";

        Geometry geometry;
        var serializer = GeoJsonSerializer.Create();
        using (var stringReader = new StringReader(geoJson))
        using (var jsonReader = new JsonTextReader(stringReader))
        {
            geometry = serializer.Deserialize<Geometry>(jsonReader);
        }

        // We use the same GeoJSON features here, since it does not matter
        // what we care about testing is if we can find all attributes on
        // the geoJSON features.
        var geoJsonFeatures = new List<GeoJsonFeature>
        {
            new GeoJsonFeature(
                type: "Feature",
                properties: new Dictionary<string, object?>
                {
                    {"id", 4909206},
                    {"forretningshaendelse", "Afledt ændring"},
                    {"senestesaglokalid", "90000424"},
                    {"forretningsproces", "Anden sagskategori"},
                    {"id_namespace", "http,//data.gov.dk/Matriklen/Jordstykke"},
                    {"lokalid", "100000870"},
                    {"paataenkthandling", null},
                    {"registreringfra", "2018-06-03T05,58,41.538609+02,00"},
                    {"virkningfra", "2008-11-17T14,06,10.674000+01,00"},
                    {"status", "Gældende"},
                    {"virkningsaktoer", "Geodatastyrelsen"},
                    {"samletfastejendomlokalid", "100001223"},
                    {"skelforretningssagslokalid", "0"},
                    {"stammerfrajordstykkelokalid", "2567340"},
                    {"supplerendemaalingsaglokalid", "0"},
                    {"arealberegningsmetode", "Areal beregnet efter opmåling - o"},
                    {"arealbetegnelse", ""},
                    {"arealtype", null},
                    {"brugsretsareal", "0"},
                    {"delnummer", null},
                    {"faelleslod", "0"},
                    {"matrikelnummer", "25s"},
                    {"ejerlavskode", 1110252},
                    {"sognekode", "7973"},
                    {"kommunekode", "0630"},
                    {"regionskode", "1083"},
                    {"registreretareal", "26358"},
                    {"vandarealinkludering", "ukendt"},
                    {"vejareal", "0"},
                    {"vejarealberegningsstatus", "vejareal beregnet"},
                    {"fredskov_omfang", null},
                    {"jordrente_omfang", null},
                    {"klitfredning_klitfredningsareal", null},
                    {"klitfredning_omfang", null},
                    {"majoratsskov_majoratsskovsnummer", null},
                    {"majoratsskov_omfang", null},
                    {"strandbeskyttelse_omfang", null},
                    {"strandbeskyttelse_strandbeskyttelsesareal", null},
                    {"stormfaldsnotering", "false"}
                },
                geometry: geometry
            ),
            new GeoJsonFeature(
                type: "Feature",
                properties: new Dictionary<string, object?>
                {
                    {"id", 4909207},
                    {"forretningshaendelse", "Afledt ændring"},
                    {"senestesaglokalid", "90000424"},
                    {"forretningsproces", "Anden sagskategori"},
                    {"id_namespace", "http,//data.gov.dk/Matriklen/Jordstykke"},
                    {"lokalid", "100000870"},
                    {"paataenkthandling", null},
                    {"registreringfra", "2018-06-03T05,58,41.538609+02,00"},
                    {"virkningfra", "2008-11-17T14,06,10.674000+01,00"},
                    {"status", "Gældende"},
                    {"virkningsaktoer", "Geodatastyrelsen"},
                    {"samletfastejendomlokalid", "100001223"},
                    {"skelforretningssagslokalid", "0"},
                    {"stammerfrajordstykkelokalid", "2567340"},
                    {"supplerendemaalingsaglokalid", "0"},
                    {"arealberegningsmetode", "Areal beregnet efter opmåling - o"},
                    {"arealbetegnelse", ""},
                    {"arealtype", null},
                    {"brugsretsareal", "0"},
                    {"delnummer", null},
                    {"faelleslod", "0"},
                    {"matrikelnummer", "25s"},
                    {"ejerlavskode", 1110252},
                    {"sognekode", "7973"},
                    {"kommunekode", "0630"},
                    {"regionskode", "1083"},
                    {"registreretareal", "26358"},
                    {"vandarealinkludering", "ukendt"},
                    {"vejareal", "0"},
                    {"vejarealberegningsstatus", "vejareal beregnet"},
                    {"fredskov_fredskovsareal", null},
                    {"fredskov_omfang", null},
                    {"jordrente_omfang", null},
                    {"klitfredning_klitfredningsareal", null},
                    {"klitfredning_omfang", null},
                    {"majoratsskov_majoratsskovsnummer", null},
                    {"majoratsskov_omfang", null},
                    {"strandbeskyttelse_omfang", null},
                    {"strandbeskyttelse_strandbeskyttelsesareal", null},
                    {"stormfaldsnotering", "false"}
                },
                geometry: geometry
            ),
            new GeoJsonFeature(
                type: "Feature",
                properties: new Dictionary<string, object?>
                {
                    {"id", 4909208},
                    {"forretningshaendelse", "Afledt ændring"},
                    {"senestesaglokalid", "90000424"},
                    {"forretningsproces", "Anden sagskategori"},
                    {"id_namespace", "http,//data.gov.dk/Matriklen/Jordstykke"},
                    {"lokalid", "100000870"},
                    {"registreringfra", "2018-06-03T05,58,41.538609+02,00"},
                    {"virkningfra", "2008-11-17T14,06,10.674000+01,00"},
                    {"status", "Gældende"},
                    {"virkningsaktoer", "Geodatastyrelsen"},
                    {"samletfastejendomlokalid", "100001223"},
                    {"skelforretningssagslokalid", "0"},
                    {"supplerendemaalingsaglokalid", "0"},
                    {"arealberegningsmetode", "Areal beregnet efter opmåling - o"},
                    {"arealbetegnelse", ""},
                    {"arealtype", null},
                    {"brugsretsareal", "0"},
                    {"delnummer", null},
                    {"faelleslod", "0"},
                    {"matrikelnummer", "25s"},
                    {"ejerlavskode", 1110252},
                    {"sognekode", "7973"},
                    {"kommunekode", "0630"},
                    {"regionskode", "1083"},
                    {"registreretareal", "26358"},
                    {"vandarealinkludering", "ukendt"},
                    {"vejareal", "0"},
                    {"vejarealberegningsstatus", "vejareal beregnet"},
                    {"fredskov_fredskovsareal", null},
                    {"fredskov_omfang", null},
                    {"jordrente_omfang", null},
                    {"klitfredning_klitfredningsareal", null},
                    {"klitfredning_omfang", null},
                    {"majoratsskov_majoratsskovsnummer", null},
                    {"majoratsskov_omfang", null},
                    {"strandbeskyttelse_omfang", null},
                    {"strandbeskyttelse_strandbeskyttelsesareal", null},
                    {"stormfaldsnotering", "false"}
                },
                geometry: geometry
            )
        };

        var expected = new DynamicTableDescription(
            schema: "dbo",
            name: "jordstykke",
            key: "id",
            columns: new List<DynamicColumnDescription>
            {
                new("id", ColumnType.Int, true),
                new("forretningshaendelse", ColumnType.String),
                new("senestesaglokalid", ColumnType.String),
                new("forretningsproces", ColumnType.String),
                new("id_namespace", ColumnType.String),
                new("lokalid", ColumnType.String),
                new("paataenkthandling", ColumnType.String),
                new("registreringfra", ColumnType.String),
                new("virkningfra", ColumnType.String),
                new("status", ColumnType.String),
                new("virkningsaktoer", ColumnType.String),
                new("samletfastejendomlokalid", ColumnType.String),
                new("skelforretningssagslokalid", ColumnType.String),
                new("stammerfrajordstykkelokalid", ColumnType.String),
                new("supplerendemaalingsaglokalid", ColumnType.String),
                new("arealberegningsmetode", ColumnType.String),
                new("arealbetegnelse", ColumnType.String),
                new("arealtype", ColumnType.String),
                new("brugsretsareal", ColumnType.String),
                new("delnummer", ColumnType.String),
                new("faelleslod", ColumnType.String),
                new("matrikelnummer", ColumnType.String),
                new("ejerlavskode", ColumnType.Int),
                new("sognekode", ColumnType.String),
                new("kommunekode", ColumnType.String),
                new("regionskode", ColumnType.String),
                new("registreretareal", ColumnType.String),
                new("vandarealinkludering", ColumnType.String),
                new("vejareal", ColumnType.String),
                new("vejarealberegningsstatus", ColumnType.String),
                new("fredskov_fredskovsareal", ColumnType.String),
                new("fredskov_omfang", ColumnType.String),
                new("jordrente_omfang", ColumnType.String),
                new("klitfredning_klitfredningsareal_test", ColumnType.String),
                new("klitfredning_omfang", ColumnType.String),
                new("majoratsskov_majoratsskovsnummer", ColumnType.String),
                new("majoratsskov_omfang", ColumnType.String),
                new("strandbeskyttelse_omfang_test", ColumnType.String),
                new("strandbeskyttelse_strandbeskyttelsesareal", ColumnType.String),
                new("stormfaldsnotering", ColumnType.String),
                new("coord", ColumnType.Geometry)
            });

        var result = DynamicTableDescriptionFactory.Create(
            "dbo",
            "jordstykke",
            "id",
            geoJsonFeatures,
            new Dictionary<string, string>
            {
                {"klitfredning_klitfredningsareal", "klitfredning_klitfredningsareal_test"},
                {"strandbeskyttelse_omfang", "strandbeskyttelse_omfang_test"}
            }
        );

        result.Should().BeEquivalentTo(expected);
    }
}
