# JsonToCvsConverter

This project converts password data from JSON into CSV format that can be imported into Proton Pass.

## Current status

- Built as a .NET 10 console application
- Uses `System.Text.Json` for JSON parsing
- Supports mapping-based conversion for different JSON shapes
- Includes unit tests with xUnit and FluentAssertions

## Supported mapping types

The mapping type is required as the first command-line argument.

- `firefox`
- `dashlane`

## Usage

```powershell
dotnet run --project .\JsonToCvsConverter.csproj -- <mapping-type> [input-json-path] [output-csv-path]
```

Examples:

```powershell
dotnet run --project .\JsonToCvsConverter.csproj -- firefox
dotnet run --project .\JsonToCvsConverter.csproj -- dashlane
dotnet run --project .\JsonToCvsConverter.csproj -- firefox .\files\passwords.firefox.json .\files\passwords.generated.csv
```

If no input path is provided, the app uses the default sample file for the selected mapping:

- `firefox` -> `files/passwords.firefox.json`
- `dashlane` -> `files/passwords.dashlane.json`

If no output path is provided, the app writes the CSV next to the input JSON file using the same file name with a `.csv` extension.

## Output schema

The generated CSV uses this fixed column order:

- `url`
- `username`
- `password`
- `httpRealm`
- `formActionOrigin`
- `guid`
- `timeCreated`
- `timeLastUsed`
- `timePasswordChanged`

## Sample files

The `files` folder contains sample input and output files:

- `passwords.firefox.json`
- `passwords.dashlane.json`
- `passwords.generated.csv`

All example data in this repository is synthetic or generated for demonstration purposes only.
The sample identities, passwords, notes, and domains are not real, and the sample domains use reserved `example.*` names.
The CSV sample files are generated from the JSON sample files.

## Tests

Run tests with:

```powershell
dotnet test .\JsonToCvsConverter.Tests\JsonToCvsConverter.Tests.csproj
```
