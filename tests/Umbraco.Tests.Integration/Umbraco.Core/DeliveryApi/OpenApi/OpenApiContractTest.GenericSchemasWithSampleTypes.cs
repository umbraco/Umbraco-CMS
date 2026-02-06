using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.DeliveryApi.OpenApi;

/// <summary>
/// Tests the OpenAPI contract with <see cref="DeliveryApiSettings.OpenApiSettings.GenerateContentTypeSchemas"/> disabled
/// but with sample content types defined. This verifies that type-specific schemas are NOT generated even when types exist.
/// </summary>
[TestFixture]
internal sealed class OpenApiContractTestGenericSchemasWithSampleTypes : OpenApiContractTestBase
{
    private const string ExpectedContractFileName = "generic-schemas-with-sample-types.json";

    public override void Setup()
    {
        // Disable content type schema generation
        InMemoryConfiguration[$"{Constants.Configuration.ConfigDeliveryApi}:OpenApi:GenerateContentTypeSchemas"] = "false";
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

        // Verify sample document type schemas are NOT present when disabled
        AssertSchemaDoesNotExist(openApiDocument, "ArticlePageContentResponseModel");
        AssertSchemaDoesNotExist(openApiDocument, "ArticlePagePropertiesModel");
        AssertSchemaDoesNotExist(openApiDocument, "LandingPageContentResponseModel");
        AssertSchemaDoesNotExist(openApiDocument, "LandingPagePropertiesModel");

        // Verify element type schemas are NOT present when disabled
        AssertSchemaDoesNotExist(openApiDocument, "TestElementElementModel");
        AssertSchemaDoesNotExist(openApiDocument, "TestElementPropertiesModel");

        // Verify built-in media type schemas are also NOT present when disabled
        AssertSchemaDoesNotExist(openApiDocument, "ImageMediaWithCropsResponseModel");
        AssertSchemaDoesNotExist(openApiDocument, "FileMediaWithCropsResponseModel");
        AssertSchemaDoesNotExist(openApiDocument, "ImagePropertiesModel");
        AssertSchemaDoesNotExist(openApiDocument, "FilePropertiesModel");

        // Verify sample media type schemas are NOT present when disabled
        AssertSchemaDoesNotExist(openApiDocument, "VideoMediaWithCropsResponseModel");
        AssertSchemaDoesNotExist(openApiDocument, "VideoPropertiesModel");
    }
}
