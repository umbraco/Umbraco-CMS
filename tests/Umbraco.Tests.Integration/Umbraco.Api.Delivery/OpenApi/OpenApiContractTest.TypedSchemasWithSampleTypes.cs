using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Api.Delivery.OpenApi;

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

        // Verify sample document type schemas are present
        AssertSchemaExists(openApiDocument, "ArticlePageContentResponseModel");
        AssertSchemaExists(openApiDocument, "ArticlePagePropertiesModel");
        AssertSchemaExists(openApiDocument, "LandingPageContentResponseModel");
        AssertSchemaExists(openApiDocument, "LandingPagePropertiesModel");

        // Verify element type schemas are present (via block list on landing page)
        AssertSchemaExists(openApiDocument, "TestElementElementModel");
        AssertSchemaExists(openApiDocument, "TestElementPropertiesModel");

        // Verify the SEO composition schema is present and referenced from the composing type's properties model
        AssertSchemaExists(openApiDocument, "SeoMetadataPropertiesModel");
        AssertSchemaComposesFrom(openApiDocument, "ArticlePagePropertiesModel", "SeoMetadataPropertiesModel");

        // Verify built-in media types are also present
        AssertSchemaExists(openApiDocument, "ImageMediaWithCropsResponseModel");
        AssertSchemaExists(openApiDocument, "FileMediaWithCropsResponseModel");

        // Verify sample media type schemas are present
        AssertSchemaExists(openApiDocument, "VideoMediaWithCropsResponseModel");
        AssertSchemaExists(openApiDocument, "VideoPropertiesModel");

        // Verify the polymorphic interface schemas wire each derived type into oneOf + discriminator mapping
        AssertSchemaIsPolymorphicUnion(
            openApiDocument,
            "IApiContentResponseModel",
            "contentType",
            new Dictionary<string, string>
            {
                ["articlePage"] = "ArticlePageContentResponseModel",
                ["landingPage"] = "LandingPageContentResponseModel",
                ["seoMetadata"] = "SeoMetadataContentResponseModel",
            });

        AssertSchemaIsPolymorphicUnion(
            openApiDocument,
            "IApiContentModel",
            "contentType",
            new Dictionary<string, string>
            {
                ["articlePage"] = "ArticlePageContentModel",
                ["landingPage"] = "LandingPageContentModel",
                ["seoMetadata"] = "SeoMetadataContentModel",
            });

        AssertSchemaIsPolymorphicUnion(
            openApiDocument,
            "IApiMediaWithCropsResponseModel",
            "mediaType",
            new Dictionary<string, string>
            {
                ["Folder"] = "FolderMediaWithCropsResponseModel",
                ["Image"] = "ImageMediaWithCropsResponseModel",
                ["File"] = "FileMediaWithCropsResponseModel",
                ["video"] = "VideoMediaWithCropsResponseModel",
            });

        AssertSchemaIsPolymorphicUnion(
            openApiDocument,
            "IApiMediaWithCropsModel",
            "mediaType",
            new Dictionary<string, string>
            {
                ["Folder"] = "FolderMediaWithCropsModel",
                ["Image"] = "ImageMediaWithCropsModel",
                ["File"] = "FileMediaWithCropsModel",
                ["video"] = "VideoMediaWithCropsModel",
            });
    }
}
