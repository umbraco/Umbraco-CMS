using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Api.Delivery.OpenApi;

/// <summary>
/// Base class for OpenAPI contract tests with shared test logic.
/// </summary>
internal abstract class OpenApiContractTestBase : OpenApiTestBase
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

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
}
