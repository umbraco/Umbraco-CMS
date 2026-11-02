using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.Extensions.FileProviders;
using NUnit.Framework;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.OpenApi;

[TestFixture]
internal sealed class OpenApiDocumentTests
{
    private static readonly Regex _identifierRegex = new("^[A-Za-z][A-Za-z0-9_]*$");

    [Test]
    public void Cannot_Contain_Duplicate_Operation_Ids()
    {
        var duplicates = OperationIds()
            .GroupBy(operationId => operationId, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.That(
            duplicates,
            Is.Empty,
            $"Operation IDs must be unique across the document, otherwise the generated client gets colliding members. Duplicated: {string.Join(", ", duplicates)}");
    }

    [Test]
    public void Cannot_Contain_Operation_Ids_That_Are_Not_Valid_Identifiers()
    {
        var invalid = OperationIds()
            .Where(operationId => _identifierRegex.IsMatch(operationId) == false)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.That(
            invalid,
            Is.Empty,
            $"Operation IDs become member names in the generated clients, so they must be valid identifiers. Invalid: {string.Join(", ", invalid)}");
    }

    private static IEnumerable<string> OperationIds()
    {
        JsonObject paths = JsonNode.Parse(ReadEmbeddedDocument())!["paths"]!.AsObject();

        return paths
            .SelectMany(path => path.Value!.AsObject())
            .Where(operation => operation.Value is JsonObject operationObject && operationObject.ContainsKey("operationId"))
            .Select(operation => operation.Value!["operationId"]!.GetValue<string>());
    }

    private static string ReadEmbeddedDocument()
    {
        using Stream stream =
            new EmbeddedFileProvider(typeof(global::Umbraco.Cms.Api.Management.ManagementApiComposer).Assembly)
                .GetFileInfo("OpenApi.json")
                .CreateReadStream();

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
