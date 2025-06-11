using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentBlueprintEditingServiceTests
{
    public static void AddScaffoldedNotificationHandler(IUmbracoBuilder builder)
        => builder
            .AddNotificationHandler<ContentScaffoldedNotification, ContentScaffoldedNotificationHandler>()
            .AddNotificationHandler<ContentScaffoldedNotification, BlockListPropertyNotificationHandler>()
            .AddNotificationHandler<ContentScaffoldedNotification, BlockGridPropertyNotificationHandler>();

    [TestCase(true)]
    [TestCase(false)]
    [ConfigureBuilder(ActionName = nameof(AddScaffoldedNotificationHandler))]
    public async Task Can_Get_Scaffold(bool variant)
    {
        var blueprint = await (variant ? CreateVariantContentBlueprint() : CreateInvariantContentBlueprint());
        try
        {
            ContentScaffoldedNotificationHandler.ContentScaffolded = notification =>
            {
                foreach (var propertyValue in notification.Scaffold.Properties.SelectMany(property => property.Values))
                {
                    propertyValue.EditedValue += " scaffolded edited";
                    propertyValue.PublishedValue += " scaffolded published";
                }
            };
            var result = await ContentBlueprintEditingService.GetScaffoldedAsync(blueprint.Key);
            Assert.IsNotNull(result);
            Assert.AreEqual(blueprint.Key, result.Key);

            var propertyValues = result.Properties.SelectMany(property => property.Values).ToArray();
            Assert.IsNotEmpty(propertyValues);
            Assert.Multiple(() =>
            {
                Assert.IsTrue(propertyValues.All(value => value.EditedValue is string stringValue && stringValue.EndsWith(" scaffolded edited")));
                Assert.IsTrue(propertyValues.All(value => value.PublishedValue is string stringValue && stringValue.EndsWith(" scaffolded published")));
            });
        }
        finally
        {
            ContentScaffoldedNotificationHandler.ContentScaffolded = null;
        }
    }

    [Test]
    public async Task Cannot_Get_Non_Existing_Scaffold()
    {
        var result = await ContentBlueprintEditingService.GetScaffoldedAsync(Guid.NewGuid());
        Assert.IsNull(result);
    }

    [TestCase(false, Constants.PropertyEditors.Aliases.BlockList)]
    [TestCase(false, Constants.PropertyEditors.Aliases.BlockGrid)]
    [TestCase(false, Constants.PropertyEditors.Aliases.RichText)]
    [TestCase(true, Constants.PropertyEditors.Aliases.BlockList)]
    [TestCase(true, Constants.PropertyEditors.Aliases.BlockGrid)]
    [TestCase(true, Constants.PropertyEditors.Aliases.RichText)]
    [ConfigureBuilder(ActionName = nameof(AddScaffoldedNotificationHandler))]
    public async Task Get_Scaffold_With_Blocks_Generates_New_Block_Ids(bool variant, string editorAlias)
    {
        var blueprint = await CreateBlueprintWithBlocksEditor(variant, editorAlias);
        var result = await ContentBlueprintEditingService.GetScaffoldedAsync(blueprint.Content.Key);
        Assert.IsNotNull(result);
        Assert.AreNotEqual(blueprint.Content.Key, result.Key);

        List<Guid> newKeys = [];
        var newInvariantBlocklist = GetBlockValue("invariantBlocks");
        newKeys.AddRange(
            newInvariantBlocklist.Layout
            .SelectMany(x => x.Value)
            .SelectMany(v => new List<Guid> { v.ContentKey, v.SettingsKey!.Value }));

        if (variant)
        {
            foreach (var culture in result.AvailableCultures)
            {
                var newVariantBlocklist = GetBlockValue("blocks", culture);
                newKeys.AddRange(
                    newVariantBlocklist.Layout
                        .SelectMany(x => x.Value)
                        .SelectMany(v => new List<Guid> { v.ContentKey, v.SettingsKey!.Value }));
            }
        }

        foreach (var newKey in newKeys)
        {
            Assert.IsTrue(!blueprint.BlockKeys.Contains(newKey), "The blocks in a content item generated from a template should have new keys.");
        }

        return;

        BlockValue GetBlockValue(string propertyAlias, string? culture = null)
        {
            return editorAlias switch
            {
                Constants.PropertyEditors.Aliases.BlockList => JsonSerializer.Deserialize<BlockListValue>(result.GetValue<string>(propertyAlias, culture)),
                Constants.PropertyEditors.Aliases.BlockGrid => JsonSerializer.Deserialize<BlockGridValue>(result.GetValue<string>(propertyAlias, culture)),
                Constants.PropertyEditors.Aliases.RichText => JsonSerializer.Deserialize<RichTextEditorValue>(result.GetValue<string>(propertyAlias, culture)).Blocks!,
                _ => throw new NotSupportedException($"Editor alias '{editorAlias}' is not supported for block blueprints."),
            };
        }
    }

    public class ContentScaffoldedNotificationHandler : INotificationHandler<ContentScaffoldedNotification>
    {
        public static Action<ContentScaffoldedNotification>? ContentScaffolded { get; set; }

        public void Handle(ContentScaffoldedNotification notification) => ContentScaffolded?.Invoke(notification);
    }
}
