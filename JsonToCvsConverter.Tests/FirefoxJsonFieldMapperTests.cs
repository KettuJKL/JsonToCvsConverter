using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace JsonToCvsConverter.Tests;

public class FirefoxJsonFieldMapperTests
{
    [Fact]
    public void Map_ShouldKeepFirefoxFieldNamesAsCsvFields()
    {
        var mapper = JsonFieldMapperFactory.Create("firefox");
        using var document = JsonDocument.Parse(
            """
            {
              "url": "https://forge.example.com",
              "username": "LukeSkywalker",
              "password": "obfuscated-test-password",
              "httpRealm": null,
              "formActionOrigin": "https://forge.example.com",
              "guid": "{1f3a9c2e-7b4d-4a81-9e65-2c8d7f10ab41}",
              "timeCreated": "1767175803453",
              "timeLastUsed": "1788179172814",
              "timePasswordChanged": "1767175803453"
            }
            """);

        var result = mapper.Map(document.RootElement);

        result.Should().BeEquivalentTo(new Dictionary<string, string?>
        {
            ["url"] = "https://forge.example.com",
            ["username"] = "LukeSkywalker",
            ["password"] = "obfuscated-test-password",
            ["httpRealm"] = null,
            ["formActionOrigin"] = "https://forge.example.com",
            ["guid"] = "{1f3a9c2e-7b4d-4a81-9e65-2c8d7f10ab41}",
            ["timeCreated"] = "1767175803453",
            ["timeLastUsed"] = "1788179172814",
            ["timePasswordChanged"] = "1767175803453"
        });
    }

    [Fact]
    public void Map_ShouldReturnNullForMissingFirefoxFields()
    {
        var mapper = JsonFieldMapperFactory.Create("firefox");
        using var document = JsonDocument.Parse(
            """
            {
              "url": "https://example.com",
              "username": "sample-user"
            }
            """);

        var result = mapper.Map(document.RootElement);

        result.Keys.Should().BeEquivalentTo(CsvSchema.Headers);
        result["url"].Should().Be("https://example.com");
        result["username"].Should().Be("sample-user");
        result["password"].Should().BeNull();
        result["httpRealm"].Should().BeNull();
        result["formActionOrigin"].Should().BeNull();
        result["guid"].Should().BeNull();
        result["timeCreated"].Should().BeNull();
        result["timeLastUsed"].Should().BeNull();
        result["timePasswordChanged"].Should().BeNull();
    }
}
