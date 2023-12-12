

using Microsoft.Extensions.Options;
using System.Text.Json;

namespace SnowFall.Common.Json;

public class SFJsonHelper
{
    private static readonly JsonSerializerOptions DefaultOptions = new() { PropertyNameCaseInsensitive = true };

    public static T Deserialize<T>(string jsonString)
    {
        var obj = JsonSerializer.Deserialize<T>(jsonString, DefaultOptions)
            ?? throw new JsonException($"Fail deserialize : {jsonString}");
        return obj;
    }

    public static string Serialize(object obj)
    {
        var jsonString = JsonSerializer.Serialize(obj);
        return jsonString;
    }
}
