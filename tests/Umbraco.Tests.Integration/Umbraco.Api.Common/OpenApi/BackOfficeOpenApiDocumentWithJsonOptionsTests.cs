using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Tests.Integration.TestServerTest;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Api.Common.OpenApi;

/// <summary>
/// Integration tests for <see cref="BackOfficeOpenApiDocumentBuilder.WithJsonOptions(JsonOptions)"/> and its
/// overloads. Each concrete subclass configures the builder with one of the three overloads and asserts that
/// the resulting OpenAPI document's schema reflects the configured serialization conventions — i.e. the
/// <c>JsonOptions.SerializerOptions</c> are actually used to generate the schema.
/// </summary>
internal abstract class BackOfficeOpenApiDocumentWithJsonOptionsTestsBase : UmbracoTestServerTestBase
{
    internal const string ApiName = "test-with-json-options";

    private IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();

    /// <summary>
    /// Configures <c>WithJsonOptions</c> using one of the three overloads. The chosen <see cref="JsonOptions"/>
    /// must set <c>SerializerOptions.PropertyNamingPolicy</c> to <see cref="JsonNamingPolicy.SnakeCaseLower"/>
    /// so the multi-word property on <see cref="TestWithJsonOptionsProduct"/> renders as <c>max_item_count</c>
    /// in the generated schema.
    /// </summary>
    protected abstract void ConfigureWithJsonOptions(BackOfficeOpenApiDocumentBuilder document);

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddMvcAndRazor(mvc =>
            mvc.AddApplicationPart(typeof(BackOfficeOpenApiDocumentWithJsonOptionsTestsBase).Assembly));

        ConfigureAdditionalServices(builder);

        builder.AddBackOfficeOpenApiDocument(
            ApiName,
            document =>
            {
                ConfigureWithJsonOptions(document);
            });
    }

    /// <summary>
    /// Hook for subclasses to register supporting services (e.g. named <see cref="JsonOptions"/>) before the
    /// OpenAPI document is registered. Default: no-op.
    /// </summary>
    protected virtual void ConfigureAdditionalServices(IUmbracoBuilder builder)
    {
    }

    [Test]
    public async Task Configured_JsonOptions_Apply_To_Generated_Schema_Property_Names()
    {
        JsonObject document = await ParseDocumentAsync();
        JsonObject? schemas = document["components"]?["schemas"]?.AsObject();

        Assert.That(schemas, Is.Not.Null, "Generated document has no component schemas.");

        JsonObject? properties = schemas![nameof(TestWithJsonOptionsProduct) + "Model"]?["properties"]?.AsObject();
        Assert.That(properties, Is.Not.Null, "Schema for the product payload was not generated.");

        // Snake-case naming policy renders MaxItemCount as max_item_count in the schema. The default ASP.NET
        // Core camelCase policy would render it as maxItemCount, so this assertion proves the WithJsonOptions
        // override actually reaches schema generation.
        Assert.That(
            properties!.ContainsKey("max_item_count"), Is.True,
            $"Expected snake_case property 'max_item_count' on schema. Properties: {string.Join(", ", properties!.Select(p => p.Key))}");
    }

    private async Task<string> FetchDocumentAsync()
    {
        var backOfficePath = HostingEnvironment.GetBackOfficePath();
        return await Client.GetStringAsync($"{backOfficePath}/openapi/{ApiName}.json");
    }

    private async Task<JsonObject> ParseDocumentAsync()
    {
        var spec = await FetchDocumentAsync();
        JsonObject? doc = JsonNode.Parse(spec)?.AsObject();
        Assert.That(doc, Is.Not.Null, "Failed to parse OpenAPI document as JSON.");
        return doc!;
    }
}

[TestFixture]
internal sealed class BackOfficeOpenApiDocumentWithJsonOptionsInstanceTests
    : BackOfficeOpenApiDocumentWithJsonOptionsTestsBase
{
    protected override void ConfigureWithJsonOptions(BackOfficeOpenApiDocumentBuilder document) =>
        document.WithJsonOptions(new JsonOptions
        {
            SerializerOptions = { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower },
        });
}

[TestFixture]
internal sealed class BackOfficeOpenApiDocumentWithJsonOptionsFactoryTests
    : BackOfficeOpenApiDocumentWithJsonOptionsTestsBase
{
    protected override void ConfigureWithJsonOptions(BackOfficeOpenApiDocumentBuilder document) =>
        document.WithJsonOptions(_ => new JsonOptions
        {
            SerializerOptions = { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower },
        });
}

[TestFixture]
internal sealed class BackOfficeOpenApiDocumentWithJsonOptionsNamedTests
    : BackOfficeOpenApiDocumentWithJsonOptionsTestsBase
{
    private const string NamedOptionsKey = "BackOfficeOpenApiDocumentWithJsonOptionsTests";

    protected override void ConfigureAdditionalServices(IUmbracoBuilder builder) =>
        builder.Services.Configure<JsonOptions>(
            NamedOptionsKey,
            options => options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);

    protected override void ConfigureWithJsonOptions(BackOfficeOpenApiDocumentBuilder document) =>
        document.WithJsonOptions(NamedOptionsKey);
}

[ApiController]
[Route(BackOfficeOpenApiDocumentWithJsonOptionsTestsBase.ApiName + "/products")]
[MapToApi(BackOfficeOpenApiDocumentWithJsonOptionsTestsBase.ApiName)]
public class TestWithJsonOptionsProductController : ControllerBase
{
    [HttpPost("save")]
    [ProducesResponseType(typeof(TestWithJsonOptionsProduct), StatusCodes.Status200OK)]
    public IActionResult Save([FromBody] TestWithJsonOptionsProduct product) => Ok(product);
}

public record TestWithJsonOptionsProduct(string Name, int MaxItemCount);
