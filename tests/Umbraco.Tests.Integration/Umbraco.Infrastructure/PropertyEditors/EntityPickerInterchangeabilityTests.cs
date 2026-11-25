using System.Text.Json.Nodes;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

/// <summary>
/// Tests that a data type can move onto the document pickers from another entity picker,
/// without its stored values being converted first: they keep rendering, and are stored in the picker's own format
/// the next time they are saved.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class EntityPickerInterchangeabilityTests : UmbracoIntegrationTest
{
    private const string PickerAlias = "picker";

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private IDocumentCacheService DocumentCacheService => GetRequiredService<IDocumentCacheService>();

    private IPublishedContentCache PublishedContentCache => GetRequiredService<IPublishedContentCache>();

    // The cache refreshers must run, so the published content picks up the data type's new editor. With the harness'
    // no-op messenger it would keep the previous editor, and these tests would pass whatever the pickers can read.
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    [TestCase(Constants.PropertyEditors.Aliases.MultiNodeTreePicker, Constants.PropertyEditors.Aliases.ContentPicker)]
    [TestCase(Constants.PropertyEditors.Aliases.MultiNodeTreePicker, Constants.PropertyEditors.Aliases.MultipleDocumentPicker)]
    [TestCase(Constants.PropertyEditors.Aliases.MultipleDocumentPicker, Constants.PropertyEditors.Aliases.ContentPicker)]
    public async Task Stored_Value_Renders_After_The_Data_Type_Editor_Changes(string storedByEditorAlias, string propertyEditorAlias)
    {
        (Guid first, Guid second, IContent page) = await CreatePageStoredBy(storedByEditorAlias);

        await SwitchPropertyEditorAsync(page.ContentType.Id, propertyEditorAlias);

        Assert.That(PublishedKeys(page.Key), Is.EqualTo(ExpectedKeys(propertyEditorAlias, first, second)));
    }

    [TestCase(Constants.PropertyEditors.Aliases.MultiNodeTreePicker, Constants.PropertyEditors.Aliases.ContentPicker)]
    [TestCase(Constants.PropertyEditors.Aliases.MultiNodeTreePicker, Constants.PropertyEditors.Aliases.MultipleDocumentPicker)]
    [TestCase(Constants.PropertyEditors.Aliases.MultipleDocumentPicker, Constants.PropertyEditors.Aliases.ContentPicker)]
    public async Task Saving_Stores_The_Value_In_The_Format_Of_The_Data_Type_Editor(string storedByEditorAlias, string propertyEditorAlias)
    {
        (Guid first, Guid second, IContent page) = await CreatePageStoredBy(storedByEditorAlias);
        await SwitchPropertyEditorAsync(page.ContentType.Id, propertyEditorAlias);

        await SaveAndPublishTheEditorValue(page.Key, propertyEditorAlias);

        var storedValue = ContentService.GetById(page.Key)!.GetValue<string>(PickerAlias);
        Assert.Multiple(() =>
        {
            if (propertyEditorAlias == Constants.PropertyEditors.Aliases.ContentPicker)
            {
                Assert.AreEqual(new GuidUdi(Constants.UdiEntityType.Document, first).ToString(), storedValue);
            }
            else
            {
                Assert.That(JsonSerializer.Deserialize<Guid[]>(storedValue!), Is.EqualTo(new[] { first, second }));
            }

            Assert.That(PublishedKeys(page.Key), Is.EqualTo(ExpectedKeys(propertyEditorAlias, first, second)));
        });
    }

    // A single picker presents the first of the stored documents.
    private static Guid[] ExpectedKeys(string propertyEditorAlias, Guid first, Guid second)
        => propertyEditorAlias == Constants.PropertyEditors.Aliases.ContentPicker ? [first] : [first, second];

    private Guid[] PublishedKeys(Guid pageKey)
        => PublishedContentCache.GetById(pageKey)?.Value(PickerAlias) switch
        {
            IPublishedContent single => [single.Key],
            IEnumerable<IPublishedContent> many => many.Select(x => x.Key).ToArray(),
            _ => [],
        };

    // The multi node tree picker stores a comma-separated list of UDIs, the multiple document picker a JSON array of keys.
    private async Task<(Guid First, Guid Second, IContent Page)> CreatePageStoredBy(string editorAlias)
    {
        IContentType targetType = new ContentTypeBuilder().WithAlias("target").WithName("Target").Build();
        await ContentTypeService.CreateAsync(targetType, Constants.Security.SuperUserKey);
        IContent firstTarget = CreateAndPublish(targetType, "First target");
        IContent secondTarget = CreateAndPublish(targetType, "Second target");

        var dataType = new DataType(
            PropertyEditorCollection[editorAlias],
            ConfigurationEditorJsonSerializer)
        {
            Name = "Picker",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
        };
        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);

        IContentType pageType = new ContentTypeBuilder()
            .WithAlias("pickerPage")
            .WithName("Picker Page")
            .AddPropertyType()
            .WithAlias(PickerAlias)
            .WithName("Picker")
            .WithDataTypeId(dataType.Id)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(pageType, Constants.Security.SuperUserKey);

        var storedValue = editorAlias == Constants.PropertyEditors.Aliases.MultiNodeTreePicker
            ? string.Join(
                ',',
                new GuidUdi(Constants.UdiEntityType.Document, firstTarget.Key),
                new GuidUdi(Constants.UdiEntityType.Document, secondTarget.Key))
            : JsonSerializer.Serialize(new[] { firstTarget.Key, secondTarget.Key });
        IContent page = CreateAndPublish(pageType, "Picker Page", new { picker = storedValue });

        return (firstTarget.Key, secondTarget.Key, page);
    }

    private IContent CreateAndPublish(IContentType contentType, string name, object? propertyValues = null)
    {
        ContentBuilder builder = new ContentBuilder().WithContentType(contentType).WithName(name);
        if (propertyValues is not null)
        {
            builder.WithPropertyValues(propertyValues);
        }

        IContent content = builder.Build();
        ContentService.Save(content);
        Publish(content);

        return content;
    }

    private void Publish(IContent content)
    {
        Assert.IsTrue(ContentService.Publish(content, ["*"]).Success);
        DocumentCacheService.RefreshContentAsync(content).GetAwaiter().GetResult();
    }

    // Mirrors the backoffice, which stores the editor's storage type along with the editor.
    private async Task SwitchPropertyEditorAsync(int contentTypeId, string propertyEditorAlias)
    {
        IDataType dataType = (await DataTypeService.GetAsync("Picker"))!;
        IDataEditor editor = PropertyEditorCollection[propertyEditorAlias];
        dataType.Editor = editor;
        dataType.DatabaseType = ValueTypes.ToStorageType(editor.GetValueEditor().ValueType);

        Attempt<IDataType, DataTypeOperationStatus> result = await DataTypeService.UpdateAsync(dataType, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        DocumentCacheService.Rebuild([contentTypeId]);
    }

    // Saves the value the editor presents, as the backoffice posts it back.
    private async Task SaveAndPublishTheEditorValue(Guid pageKey, string propertyEditorAlias)
    {
        IProperty property = ContentService.GetById(pageKey)!.Properties[PickerAlias]!;
        var editorValue = PropertyEditorCollection[propertyEditorAlias].GetValueEditor().ToEditor(property);
        object? postedValue = editorValue is string[]
            ? JsonNode.Parse(JsonSerializer.Serialize(editorValue))
            : editorValue?.ToString();

        Attempt<ContentUpdateResult, ContentEditingOperationStatus> result = await ContentEditingService.UpdateAsync(
            pageKey,
            new ContentUpdateModel
            {
                Variants = [new VariantModel { Name = "Picker Page" }],
                Properties = [new PropertyValueModel { Alias = PickerAlias, Value = postedValue }],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success, $"Saving failed with status {result.Status}.");

        Publish(ContentService.GetById(pageKey)!);
    }
}
