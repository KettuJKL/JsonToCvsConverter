using System.Globalization;
using System.Text;
using System.Text.Json;

return await RunAsync(args);

static async Task<int> RunAsync(string[] args)
{
    if (args.Length is < 1 or > 3)
    {
        Console.Error.WriteLine("Usage: JsonToCvsConverter <mapping-type> [input-json-path] [output-csv-path]");
        Console.Error.WriteLine("Mapping types: firefox, dashlane");
        return 1;
    }

    var workingDirectory = Directory.GetCurrentDirectory();
    var mappingType = args[0];
    var inputPath = args.Length >= 2
        ? GetFullPath(args[1], workingDirectory)
        : Path.Combine(workingDirectory, "files", GetDefaultInputFileName(mappingType));
    var outputPath = args.Length == 3
        ? GetFullPath(args[2], workingDirectory)
        : Path.ChangeExtension(inputPath, ".csv");

    if (!File.Exists(inputPath))
    {
        Console.Error.WriteLine($"Input file was not found: {inputPath}");
        return 1;
    }

    try
    {
        var mapper = JsonFieldMapperFactory.Create(mappingType);
        var json = await File.ReadAllTextAsync(inputPath);
        var rows = ParseRows(json, mapper);
        var headers = mapper.Headers;

        if (headers.Count == 0)
        {
            Console.Error.WriteLine("Input JSON did not contain any columns.");
            return 1;
        }

        var csv = BuildCsv(headers, rows);
        var outputDirectory = Path.GetDirectoryName(outputPath);

        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        await File.WriteAllTextAsync(outputPath, csv, Encoding.UTF8);

        Console.WriteLine($"Created CSV file: {outputPath}");
        Console.WriteLine($"Rows written: {rows.Count.ToString(CultureInfo.InvariantCulture)}");
        return 0;
    }
    catch (JsonException ex)
    {
        Console.Error.WriteLine($"Invalid JSON: {ex.Message}");
        return 1;
    }
    catch (InvalidOperationException ex)
    {
        Console.Error.WriteLine(ex.Message);
        return 1;
    }
}

static string GetDefaultInputFileName(string mappingType)
{
    return mappingType.ToLowerInvariant() switch
    {
        "firefox" => "passwords.firefox.json",
        "dashlane" => "passwords.dashlane.json",
        _ => throw new InvalidOperationException($"Unknown mapping type '{mappingType}'. Supported mappings: firefox, dashlane"),
    };
}

static string GetFullPath(string path, string workingDirectory)
{
    return Path.IsPathRooted(path) ? path : Path.GetFullPath(Path.Combine(workingDirectory, path));
}

static List<Dictionary<string, string?>> ParseRows(string json, IJsonFieldMapper mapper)
{
    using var document = JsonDocument.Parse(json);

    if (document.RootElement.ValueKind != JsonValueKind.Array)
    {
        throw new InvalidOperationException("Input JSON must be an array of objects.");
    }

    var rows = new List<Dictionary<string, string?>>();

    foreach (var item in document.RootElement.EnumerateArray())
    {
        rows.Add(mapper.Map(item));
    }

    return rows;
}

static string BuildCsv(IReadOnlyList<string> headers, IReadOnlyList<Dictionary<string, string?>> rows)
{
    var builder = new StringBuilder();
    builder.AppendLine(string.Join(',', headers.Select(EscapeCsv)));

    foreach (var row in rows)
    {
        var values = headers.Select(header => row.TryGetValue(header, out var value) ? value : null);
        builder.AppendLine(string.Join(',', values.Select(EscapeCsv)));
    }

    return builder.ToString();
}

static string EscapeCsv(string? value)
{
    if (value is null)
    {
        return string.Empty;
    }

    return $"\"{value.Replace("\"", "\"\"")}\"";
}
