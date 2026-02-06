using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.DeliveryApi.OpenApi;

/// <summary>
/// Tests the OpenAPI contract with <see cref="DeliveryApiSettings.OpenApiSettings.GenerateContentTypeSchemas"/> disabled.
/// This produces generic schemas without type-specific response models.
/// </summary>
[TestFixture]
internal sealed class OpenApiContractTestGenericSchemas : OpenApiContractTestBase
{
    private const string ExpectedContractFileName = "generic-schemas.json";

    public override void Setup()
    {
        // Disable content type schema generation
        InMemoryConfiguration[$"{Constants.Configuration.ConfigDeliveryApi}:OpenApi:GenerateContentTypeSchemas"] = "false";
        base.Setup();
    }

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

        // Verify built-in media type schemas are NOT present when disabled
        AssertSchemaDoesNotExist(openApiDocument, "ImageMediaWithCropsResponseModel");
        AssertSchemaDoesNotExist(openApiDocument, "FileMediaWithCropsResponseModel");
        AssertSchemaDoesNotExist(openApiDocument, "FolderMediaWithCropsResponseModel");
        AssertSchemaDoesNotExist(openApiDocument, "ImagePropertiesModel");
        AssertSchemaDoesNotExist(openApiDocument, "FilePropertiesModel");
        AssertSchemaDoesNotExist(openApiDocument, "FolderPropertiesModel");
    }
}
