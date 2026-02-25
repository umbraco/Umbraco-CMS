using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Api.Delivery.OpenApi;

/// <summary>
/// Tests the default OpenAPI contract with <see cref="DeliveryApiSettings.OpenApiSettings.GenerateContentTypeSchemas"/> disabled.
/// This produces generic schemas without type-specific response models.
/// </summary>
[TestFixture]
internal sealed class OpenApiContractTestDefault : OpenApiContractTestBase
{
    private const string ExpectedContractFileName = "default.json";

    public override void Setup()
    {
        // Disable content type schema generation
        InMemoryConfiguration[$"{Constants.Configuration.ConfigDeliveryApi}:OpenApi:GenerateContentTypeSchemas"] = "false";
        base.Setup();
    }

    [Test]
    public async Task OpenApiDocument_IsValid()
    {
        var openApiSpec = await FetchOpenApiSpecAsync();
        await ValidateOpenApiSpecAsync(openApiSpec);
    }

    [Test]
    public async Task OpenApiContract_MatchesExpected()
    {
        var openApiSpec = await FetchOpenApiSpecAsync();
        await ValidateContractAsync(openApiSpec, ExpectedContractFileName);
    }

    [Test]
    public async Task OpenApiContract_HasExpectedSchemas()
    {
        var openApiSpec = await FetchOpenApiSpecAsync();
        var openApiDocument = ParseOpenApiSpec(openApiSpec);

        // Verify built-in media type schemas are NOT present when disabled
        AssertSchemaDoesNotExist(openApiDocument, "ImageMediaWithCropsResponseModel");
        AssertSchemaDoesNotExist(openApiDocument, "FileMediaWithCropsResponseModel");
        AssertSchemaDoesNotExist(openApiDocument, "FolderMediaWithCropsResponseModel");
        AssertSchemaDoesNotExist(openApiDocument, "ImagePropertiesModel");
        AssertSchemaDoesNotExist(openApiDocument, "FilePropertiesModel");
        AssertSchemaDoesNotExist(openApiDocument, "FolderPropertiesModel");
    }
}
