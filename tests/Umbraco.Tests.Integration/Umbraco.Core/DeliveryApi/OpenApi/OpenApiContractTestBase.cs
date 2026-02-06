using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.Controllers;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.TestServerTest;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.DeliveryApi.OpenApi;

/// <summary>
/// Base class for OpenAPI contract tests with shared test logic.
/// </summary>
internal abstract class OpenApiContractTestBase : UmbracoTestServerTestBase
{
    private IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.AddMvcAndRazor(mvcBuilder =>
        {
            // Adds Umbraco.Cms.Api.Delivery
            mvcBuilder.AddApplicationPart(typeof(DeliveryApiControllerBase).Assembly);
        });

    /// <summary>
    /// Fetches the generated OpenAPI contract from the Delivery API endpoint.
    /// </summary>
    protected async Task<string> FetchOpenApiContractAsync()
    {
        var backOfficePath = HostingEnvironment.GetBackOfficePath();
        var openApiPath = $"{backOfficePath}/openapi/delivery.json";
        return await Client.GetStringAsync(openApiPath);
    }

    /// <summary>
    /// Known OpenAPI validation issues that are expected and should be ignored.
    /// These are tracked issues that don't prevent the API from functioning correctly.
    /// </summary>
    private static readonly string[] _knownValidationIssues =
    [
        // These path signature warnings occur because the Delivery API has multiple endpoints
        // that resolve to the same path pattern (e.g., get by id vs get by path)
        "The path signature '/umbraco/delivery/api/v2/content/item/{}' MUST be unique.",
        "The path signature '/umbraco/delivery/api/v2/media/item/{}' MUST be unique.",
    ];

    /// <summary>
    /// Validates that the OpenAPI document is well-formed and compliant with the OpenAPI specification.
    /// Known issues are logged as warnings but don't fail the test. Unexpected issues will fail the test.
    /// </summary>
    /// <param name="openApiJson">The OpenAPI document as JSON string.</param>
    protected static async Task ValidateOpenApiSpecAsync(string openApiJson)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(openApiJson));
        var result = await OpenApiDocument.LoadAsync(stream, "json");

        Assert.That(result.Document, Is.Not.Null, "Failed to parse OpenAPI document.");

        var allErrors = result.Diagnostic?.Errors ?? [];
        var knownIssues = allErrors.Where(e => _knownValidationIssues.Contains(e.Message)).ToList();

        // Log known issues as warnings (test still passes but warnings are visible in reports)
        foreach (var issue in knownIssues)
        {
            Assert.Warn($"(Known) {issue.Message}");
        }

        // Fail on unexpected errors (each error reported individually)
        Assert.Multiple(() =>
        {
            foreach (var error in allErrors.Except(knownIssues))
            {
                Assert.Fail($"(Error) {error.Message}");
            }
        });
    }

    /// <summary>
    /// Validates that the generated OpenAPI contract matches the expected contract file.
    /// If the expected contract file doesn't exist, saves the generated contract to that location.
    /// </summary>
    /// <param name="generatedContract">The generated OpenAPI contract JSON.</param>
    /// <param name="expectedContractFileName">The file name of the expected contract in the ExpectedContracts folder.</param>
    /// <returns>The parsed OpenAPI document as a JsonNode for additional assertions.</returns>
    protected static async Task<JsonNode> ValidateContractAsync(string generatedContract, string expectedContractFileName)
    {
        var expectedContractPath = GetExpectedContractPath(expectedContractFileName);
        if (!System.IO.File.Exists(expectedContractPath))
        {
            // Expected contract doesn't exist - save the generated one
            Directory.CreateDirectory(Path.GetDirectoryName(expectedContractPath)!);
            await System.IO.File.WriteAllTextAsync(expectedContractPath, generatedContract);
            Assert.Inconclusive($"Expected contract file did not exist. Generated contract saved to: {expectedContractPath}");
            return null!;
        }

        // Compare against existing expected contract
        var expectedContract = await System.IO.File.ReadAllTextAsync(expectedContractPath);
        var generatedOpenApiJson = JsonNode.Parse(generatedContract);
        var expectedOpenApiJson = JsonNode.Parse(expectedContract);

        Assert.NotNull(generatedOpenApiJson);
        Assert.NotNull(expectedOpenApiJson);

        Assert.AreEqual(
            expectedOpenApiJson.ToJsonString(),
            generatedOpenApiJson.ToJsonString(),
            "Generated API does not respect the contract.");

        return generatedOpenApiJson!;
    }

    /// <summary>
    /// Parses the OpenAPI contract JSON into a JsonNode for assertions.
    /// </summary>
    protected static JsonNode ParseOpenApiContract(string openApiJson)
    {
        var document = JsonNode.Parse(openApiJson);
        Assert.That(document, Is.Not.Null, "Failed to parse OpenAPI contract JSON.");
        return document!;
    }

    /// <summary>
    /// Asserts that the specified schema exists in the OpenAPI document.
    /// </summary>
    protected static void AssertSchemaExists(JsonNode openApiDocument, string schemaName)
    {
        var schemas = openApiDocument["components"]?["schemas"];
        Assert.That(schemas, Is.Not.Null, "OpenAPI document has no schemas defined.");
        Assert.That(schemas![schemaName], Is.Not.Null, $"Schema '{schemaName}' was not found in the OpenAPI document.");
    }

    /// <summary>
    /// Asserts that the specified schema does NOT exist in the OpenAPI document.
    /// </summary>
    protected static void AssertSchemaDoesNotExist(JsonNode openApiDocument, string schemaName)
    {
        var schemas = openApiDocument["components"]?["schemas"];
        Assert.That(schemas?[schemaName], Is.Null, $"Schema '{schemaName}' should not exist in the OpenAPI document when content type schemas are disabled.");
    }

    /// <summary>
    /// Creates sample content types for testing schema generation.
    /// Includes document types, element types, and media types with various property types.
    /// </summary>
    protected async Task CreateSampleContentTypesAsync()
    {
        // Fetch built-in data types (WithDataTypeId requires the integer ID, not just the GUID)
        var textDataType = await DataTypeService.GetAsync(Constants.DataTypes.Guids.TextstringGuid);
        var richTextDataType = await DataTypeService.GetAsync(Constants.DataTypes.Guids.RichtextEditorGuid);
        var dateDataType = await DataTypeService.GetAsync(Constants.DataTypes.Guids.DatePickerWithTimeGuid);
        var contentPickerDataType = await DataTypeService.GetAsync(Constants.DataTypes.Guids.ContentPickerGuid);
        var mediaPickerDataType = await DataTypeService.GetAsync(Constants.DataTypes.Guids.MediaPicker3Guid);
        var uploadVideoDataType = await DataTypeService.GetAsync(Constants.DataTypes.Guids.UploadVideoGuid);

        // Create an element type (for use in block editors)
        var testElement = new ContentTypeBuilder()
            .WithAlias("testElement")
            .WithName("Test Element")
            .WithIsElement(true)
            .AddPropertyGroup()
                .WithName("Content")
                .WithAlias("content")
                .AddPropertyType()
                    .WithAlias("title")
                    .WithName("Title")
                    .WithDataTypeId(textDataType!.Id)
                    .Done()
                .AddPropertyType()
                    .WithAlias("subtitle")
                    .WithName("Subtitle")
                    .WithDataTypeId(textDataType!.Id)
                    .Done()
                .Done()
            .Build();
        await ContentTypeService.CreateAsync(testElement, Constants.Security.SuperUserKey);

        // Create a block list data type that allows the test element
        var blockListDataType = await CreateBlockListDataTypeAsync(testElement);

        // Create a document type for articles (with content picker for circular reference)
        var articlePage = new ContentTypeBuilder()
            .WithAlias("articlePage")
            .WithName("Article Page")
            .AddPropertyGroup()
                .WithName("Content")
                .WithAlias("content")
                .AddPropertyType()
                    .WithAlias("headline")
                    .WithName("Headline")
                    .WithDataTypeId(textDataType!.Id)
                    .Done()
                .AddPropertyType()
                    .WithAlias("bodyText")
                    .WithName("Body Text")
                    .WithDataTypeId(richTextDataType!.Id)
                    .Done()
                .AddPropertyType()
                    .WithAlias("publishDate")
                    .WithName("Publish Date")
                    .WithDataTypeId(dateDataType!.Id)
                    .Done()
                .AddPropertyType()
                    .WithAlias("relatedArticles")
                    .WithName("Related Articles")
                    .WithDataTypeId(contentPickerDataType!.Id)
                    .Done()
                .Done()
            .Build();
        await ContentTypeService.CreateAsync(articlePage, Constants.Security.SuperUserKey);

        // Create a document type for landing pages (with block list for element type testing)
        var landingPage = new ContentTypeBuilder()
            .WithAlias("landingPage")
            .WithName("Landing Page")
            .AddPropertyGroup()
                .WithName("Content")
                .WithAlias("content")
                .AddPropertyType()
                    .WithAlias("pageTitle")
                    .WithName("Page Title")
                    .WithDataTypeId(textDataType!.Id)
                    .Done()
                .AddPropertyType()
                    .WithAlias("introduction")
                    .WithName("Introduction")
                    .WithDataTypeId(richTextDataType!.Id)
                    .Done()
                .AddPropertyType()
                    .WithAlias("contentBlocks")
                    .WithName("Content Blocks")
                    .WithDataTypeId(blockListDataType.Id)
                    .Done()
                .Done()
            .Build();
        await ContentTypeService.CreateAsync(landingPage, Constants.Security.SuperUserKey);

        // Create a custom media type for videos (with media picker for circular reference)
        var videoMedia = new MediaTypeBuilder()
            .WithAlias("video")
            .WithName("Video")
            .AddPropertyGroup()
                .WithName("Media")
                .WithAlias("media")
                .AddPropertyType()
                    .WithAlias("videoTitle")
                    .WithName("Video Title")
                    .WithDataTypeId(textDataType!.Id)
                    .Done()
                .AddPropertyType()
                    .WithAlias("videoFile")
                    .WithName("Video File")
                    .WithDataTypeId(uploadVideoDataType!.Id)
                    .Done()
                .AddPropertyType()
                    .WithAlias("thumbnailImage")
                    .WithName("Thumbnail Image")
                    .WithDataTypeId(mediaPickerDataType!.Id)
                    .Done()
                .Done()
            .Build();
        await MediaTypeService.CreateAsync(videoMedia, Constants.Security.SuperUserKey);
    }

    /// <summary>
    /// Creates a block list data type configured with the specified element type.
    /// </summary>
    private async Task<IDataType> CreateBlockListDataTypeAsync(IContentType elementType)
    {
        var blockListEditor = PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockList];
        var dataType = new DataType(blockListEditor, ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks", new BlockListConfiguration.BlockConfiguration[]
                    {
                        new() { ContentElementTypeKey = elementType.Key },
                    }
                },
            },
            Name = "Content Blocks",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow,
        };

        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        return dataType;
    }

    private static string GetExpectedContractPath(string fileName, [CallerFilePath] string callerFilePath = "")
    {
        // Use the source directory (where this file lives) so that saved files go to source control
        var sourceDirectory = Path.GetDirectoryName(callerFilePath)!;
        return Path.Combine(sourceDirectory, "ExpectedContracts", fileName);
    }
}
