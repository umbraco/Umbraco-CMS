using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PublishedCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class VisualEditorContentFactoryTests : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    private IVisualEditorContentFactory VisualEditorContentFactory => GetRequiredService<IVisualEditorContentFactory>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    [Test]
    public async Task CreateWithOverrides_Returns_Converted_Unsaved_Values()
    {
        var elementType = ContentTypeBuilder.CreateAllTypesContentType("spikeElement", "Spike Element");
        elementType.IsElement = true;
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        var blockListDataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockList], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    new BlockListConfiguration.BlockConfiguration[] { new() { ContentElementTypeKey = elementType.Key } }
                }
            },
            Name = "Spike Block List",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow,
        };
        await DataTypeService.CreateAsync(blockListDataType, Constants.Security.SuperUserKey);

        var contentType = new ContentTypeBuilder()
            .WithAlias("spikePage")
            .WithName("Spike Page")
            .AddPropertyGroup()
                .WithAlias("content")
                .WithName("Content")
                .WithSupportsPublishing(true)
                .AddPropertyType()
                    .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                    .WithDataTypeId(Constants.DataTypes.Textbox)
                    .WithValueStorageType(ValueStorageType.Nvarchar)
                    .WithAlias("title").WithName("Title").Done()
                .AddPropertyType()
                    .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.RichText)
                    .WithDataTypeId(Constants.DataTypes.RichtextEditor)
                    .WithValueStorageType(ValueStorageType.Ntext)
                    .WithAlias("rte").WithName("RTE").Done()
                .AddPropertyType()
                    .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.BlockList)
                    .WithDataTypeId(blockListDataType.Id)
                    .WithValueStorageType(ValueStorageType.Ntext)
                    .WithAlias("blocks").WithName("Blocks").Done()
                .Done()
            .Build();
        Assert.IsTrue((await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey)).Success);

        var content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Spike Doc")
            .WithPropertyValues(new { title = "Original title" })
            .Build();
        Assert.IsTrue(ContentService.Save(content).Success);
        Assert.IsTrue(ContentService.Publish(content, []).Success);

        var blockContentKey = Guid.NewGuid();
        var blockListEditorValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                { Constants.PropertyEditors.Aliases.BlockList, new IBlockLayoutItem[] { new BlockListLayoutItem { ContentKey = blockContentKey } } }
            },
            ContentData =
            [
                new()
                {
                    Key = blockContentKey,
                    ContentTypeAlias = elementType.Alias,
                    ContentTypeKey = elementType.Key,
                    Values = [ new() { Alias = "singleLineText", Value = "Block text value" } ]
                }
            ],
            Expose = [ new(blockContentKey, null, null) ]
        };

        var overrides = new[]
        {
            new VisualEditorPropertyOverride("title", "Overridden title", null, null),
            new VisualEditorPropertyOverride("rte", new RichTextEditorValue { Markup = "<p>Overridden rich text</p>", Blocks = null }, null, null),
            new VisualEditorPropertyOverride("blocks", blockListEditorValue, null, null),
        };

        IPublishedContent? result = await VisualEditorContentFactory.CreateWithOverridesAsync(content.Key, overrides);

        Assert.IsNotNull(result);
        Assert.AreEqual("Overridden title", result!.Value("title"));
        StringAssert.Contains("Overridden rich text", result.Value("rte")!.ToString());

        var blockListModel = result.Value("blocks") as BlockListModel;
        Assert.IsNotNull(blockListModel, "Block List should convert to a BlockListModel");
        Assert.AreEqual(1, blockListModel!.Count);
        Assert.AreEqual("Block text value", blockListModel.First().Content.Value("singleLineText"));
    }
}
