using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.DeliveryApi.OpenApi;

/// <summary>
/// Tests the OpenAPI contract with <see cref="DeliveryApiSettings.OpenApiSettings.GenerateContentTypeSchemas"/> enabled.
/// This produces typed schemas including Umbraco's built-in media types (File, Folder, Image, etc.).
/// </summary>
[TestFixture]
internal sealed class OpenApiContractTestTypedSchemas : OpenApiContractTestBase
{
    private const string ExpectedContractFileName = "typed-schemas.json";

    public override void Setup()
    {
        // Enable content type schema generation
        InMemoryConfiguration[$"{Constants.Configuration.ConfigDeliveryApi}:OpenApi:GenerateContentTypeSchemas"] = "true";
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

        // Verify built-in media types are present in the schema
        AssertSchemaExists(openApiDocument, "ImageMediaWithCropsResponseModel");
        AssertSchemaExists(openApiDocument, "FileMediaWithCropsResponseModel");
        AssertSchemaExists(openApiDocument, "FolderMediaWithCropsResponseModel");

        // Verify the properties models for built-in media types
        AssertSchemaExists(openApiDocument, "ImagePropertiesModel");
        AssertSchemaExists(openApiDocument, "FilePropertiesModel");
        AssertSchemaExists(openApiDocument, "FolderPropertiesModel");
    }
}
