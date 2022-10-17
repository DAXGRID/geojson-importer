using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace DatafordelerUtil;

internal static class JsonReaderExtensions
{
    public static IEnumerable<T?> StreamInnerArrayWithRegex<T>(
        this JsonReader jsonReader, Regex regex)
    {
        JsonSerializer serializer = new JsonSerializer();
        while (jsonReader.Read())
        {
            if (regex.IsMatch(jsonReader.Path)
                && jsonReader.TokenType != JsonToken.PropertyName)
            {
                break;
            }
        }

        while (jsonReader.Read())
        {
            if (jsonReader.TokenType == JsonToken.EndArray)
            {
                break;
            }

            if (jsonReader.TokenType == JsonToken.StartObject)
            {
                yield return serializer.Deserialize<T?>(jsonReader);
            }
        }
    }
}
