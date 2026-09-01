using System.Text.Json;

internal static class CsvSchema
{
    public static readonly IReadOnlyList<string> Headers =
    [
        "url",
        "username",
        "password",
        "httpRealm",
        "formActionOrigin",
        "guid",
        "timeCreated",
        "timeLastUsed",
        "timePasswordChanged"
    ];
}

internal interface IJsonFieldMapper
{
    IReadOnlyList<string> Headers { get; }

    Dictionary<string, string?> Map(JsonElement source);
}

internal static class JsonFieldMapperFactory
{
    public static IJsonFieldMapper Create(string mappingType)
    {
        return mappingType.ToLowerInvariant() switch
        {
            "firefox" => new AliasJsonFieldMapper(new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["url"] = "url",
                ["username"] = "username",
                ["password"] = "password",
                ["httpRealm"] = "httpRealm",
                ["formActionOrigin"] = "formActionOrigin",
                ["guid"] = "guid",
                ["timeCreated"] = "timeCreated",
                ["timeLastUsed"] = "timeLastUsed",
                ["timePasswordChanged"] = "timePasswordChanged"
            }),
            "dashlane" => new AliasJsonFieldMapper(new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["url"] = "url",
                ["username"] = "name",
                ["password"] = "pwd",
                ["httpRealm"] = "httpRealm",
                ["formActionOrigin"] = "formActionOrigin",
                ["guid"] = "guid",
                ["timeCreated"] = "created",
                ["timeLastUsed"] = "lastUsed",
                ["timePasswordChanged"] = "changed"
            }),
            _ => throw new InvalidOperationException($"Unknown mapping type '{mappingType}'. Supported mappings: firefox, dashlane")
        };
    }
}

internal sealed class AliasJsonFieldMapper(IReadOnlyDictionary<string, string> sourceFields) : IJsonFieldMapper
{
    public IReadOnlyList<string> Headers => CsvSchema.Headers;

    public Dictionary<string, string?> Map(JsonElement source)
    {
        if (source.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("Each item in the JSON array must be an object.");
        }

        var row = new Dictionary<string, string?>(StringComparer.Ordinal);

        foreach (var header in Headers)
        {
            if (!sourceFields.TryGetValue(header, out var sourceFieldName))
            {
                row[header] = null;
                continue;
            }

            row[header] = source.TryGetProperty(sourceFieldName, out var property)
                ? ConvertJsonValue(property)
                : null;
        }

        return row;
    }

    private static string? ConvertJsonValue(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.Undefined => null,
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.ToString(),
            JsonValueKind.True => bool.TrueString.ToLowerInvariant(),
            JsonValueKind.False => bool.FalseString.ToLowerInvariant(),
            JsonValueKind.Object => value.GetRawText(),
            JsonValueKind.Array => value.GetRawText(),
            _ => value.ToString(),
        };
    }
}
