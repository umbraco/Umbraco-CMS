using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Testing.Search;
using IndexValue = Umbraco.Cms.Search.Core.Models.Indexing.IndexValue;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

/// <summary>
/// Verifies that the content of an externally referenced (reusable) block element is flattened into the referencing
/// document's index entry when <see cref="IndexingSettings.IndexExternalBlockElements"/> is enabled.
/// </summary>
public class ExternalBlockElementIndexingTests : PropertyValueHandlerTestsBase
{
    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    private IElementService ElementService => GetRequiredService<IElementService>();

    [SetUp]
    public void SetUp() => IndexerAndSearcher.Reset();

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);
        services.Configure<IndexingSettings>(options => options.IndexExternalBlockElements = true);
    }

    [Test]
    public async Task Published_ExternalElement_Content_Is_Flattened_Into_PublishedIndex_Only()
    {
        var (contentType, elementType) = await SetupBlockListWithElementType();

        Guid elementKey = CreateAndPublishElement(elementType, "The external element text");

        Content content = CreatePageWithExternalBlockReference(contentType, elementKey);
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        TestIndexDocument publishedDocument = IndexerAndSearcher.Dump(IndexAliases.PublishedContent).Single();
        IndexValue? publishedValue = publishedDocument.Fields.FirstOrDefault(f => f.FieldName == "blocks")?.Value;
        Assert.That(publishedValue, Is.Not.Null);
        CollectionAssert.Contains(publishedValue.Texts, "The external element text");

        // external element content is only ever flattened into the published index, never the draft one
        TestIndexDocument draftDocument = IndexerAndSearcher.Dump(IndexAliases.DraftContent).Single();
        IndexValue? draftValue = draftDocument.Fields.FirstOrDefault(f => f.FieldName == "blocks")?.Value;
        Assert.That(draftValue, Is.Null);
    }

    [Test]
    public async Task Unpublished_ExternalElement_Content_Is_Not_Flattened()
    {
        var (contentType, elementType) = await SetupBlockListWithElementType();

        // create, but do not publish, the referenced element
        Element element = new ElementBuilder()
            .WithContentType(elementType)
            .WithName("Unpublished reusable element")
            .Build();
        element.SetValue("textValue", "Should not be indexed");
        ElementService.Save(element);

        Content content = CreatePageWithExternalBlockReference(contentType, element.Key);
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        TestIndexDocument publishedDocument = IndexerAndSearcher.Dump(IndexAliases.PublishedContent).Single();
        IndexValue? publishedValue = publishedDocument.Fields.FirstOrDefault(f => f.FieldName == "blocks")?.Value;
        Assert.That(publishedValue, Is.Null);
    }

    private Guid CreateAndPublishElement(IContentType elementType, string textValue)
    {
        Element element = new ElementBuilder()
            .WithContentType(elementType)
            .WithName("Reusable element")
            .Build();
        element.SetValue("textValue", textValue);
        ElementService.Save(element);
        ElementService.Publish(element, ["*"]);
        return element.Key;
    }

    private Content CreatePageWithExternalBlockReference(IContentType contentType, Guid externalElementKey)
    {
        var blockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    [new BlockListLayoutItem { ContentKey = externalElementKey, IsExternalContent = true }]
                }
            },
            ContentData = [],
            Expose = [],
        };

        Content content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("My Page")
            .Build();
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        return content;
    }

    private async Task<(IContentType ContentType, IContentType ElementType)> SetupBlockListWithElementType()
    {
        IContentType elementType = new ContentTypeBuilder()
            .WithAlias("reusableElement")
            .WithName("Reusable Element")
            .WithIsElement(true)
            .AddPropertyType()
            .WithAlias("textValue")
            .WithName("Text")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        var blockListDataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockList], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    new BlockListConfiguration.BlockConfiguration[]
                    {
                        new() { ContentElementTypeKey = elementType.Key }
                    }
                }
            },
            Name = "My Block List",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };
        await GetRequiredService<IDataTypeService>().CreateAsync(blockListDataType, Constants.Security.SuperUserKey);

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("pageWithBlocks")
            .WithName("Page With Blocks")
            .AddPropertyType()
            .WithAlias("blocks")
            .WithName("blocks")
            .WithDataTypeId(blockListDataType.Id)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        return (contentType, elementType);
    }
}
