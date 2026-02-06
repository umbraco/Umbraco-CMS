using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.DeliveryApi.OpenApi;

/// <summary>
/// Tests the OpenAPI contract with <see cref="DeliveryApiSettings.OpenApiSettings.GenerateContentTypeSchemas"/> enabled
/// and sample content types defined. This shows how document/element types appear in the generated schema.
/// </summary>
[TestFixture]
internal sealed class OpenApiContractTestTypedSchemasWithSampleTypes : OpenApiContractTestBase
{
    private const string ExpectedContractFileName = "typed-schemas-with-sample-types.json";

    public override void Setup()
    {
        // Enable content type schema generation
        InMemoryConfiguration[$"{Constants.Configuration.ConfigDeliveryApi}:OpenApi:GenerateContentTypeSchemas"] = "true";
        base.Setup();
    }

    [SetUp]
    public async Task SetupSampleTypesAsync() => await CreateSampleContentTypesAsync();

    [Test]
    public async Task OpenApiDocument_IsValid()
    {
        var openApiContract = await FetchOpenApiContractAsync();
        await ValidateOpenApiSpecAsync(openApiContract);
    }

    [Test]
    public async Task OpenApiContract_MatchesExpected()
    {
        var openApiContract = await FetchOpenApiContractAsync();
        await ValidateContractAsync(openApiContract, ExpectedContractFileName);
    }

    [Test]
    public async Task OpenApiContract_HasExpectedSchemas()
    {
        var openApiContract = await FetchOpenApiContractAsync();
        var openApiDocument = ParseOpenApiContract(openApiContract);

        // Verify sample document type schemas are present
        AssertSchemaExists(openApiDocument, "ArticlePageContentResponseModel");
        AssertSchemaExists(openApiDocument, "ArticlePagePropertiesModel");
        AssertSchemaExists(openApiDocument, "LandingPageContentResponseModel");
        AssertSchemaExists(openApiDocument, "LandingPagePropertiesModel");

        // Verify element type schemas are present (via block list on landing page)
        AssertSchemaExists(openApiDocument, "TestElementElementModel");
        AssertSchemaExists(openApiDocument, "TestElementPropertiesModel");

        // Verify built-in media types are also present
        AssertSchemaExists(openApiDocument, "ImageMediaWithCropsResponseModel");
        AssertSchemaExists(openApiDocument, "FileMediaWithCropsResponseModel");

        // Verify sample media type schemas are present
        AssertSchemaExists(openApiDocument, "VideoMediaWithCropsResponseModel");
        AssertSchemaExists(openApiDocument, "VideoPropertiesModel");
    }
}
