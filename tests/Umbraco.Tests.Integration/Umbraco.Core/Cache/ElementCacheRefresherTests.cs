using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;
using IUmbracoBuilder = Umbraco.Cms.Core.DependencyInjection.IUmbracoBuilder;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Cache;

[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ElementCacheRefresherTests : UmbracoIntegrationTest
{
    private static readonly ProbeSink _sink = new();

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IElementPublishingService ElementPublishingService => GetRequiredService<IElementPublishingService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private int ElementId(Guid key) => IdKeyMap.GetIdForKey(key, UmbracoObjectTypes.Element).Result;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();

        // The binder that turns element tree changes into a distributed element cache refresh, plus a probe that
        // captures the resulting payloads. External-block-element reindexing relies on this refresh being fired.
        builder.AddNotificationHandler<ElementTreeChangeNotification, ElementTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<ElementCacheRefresherNotification, Probe>();

        _sink.Payloads.Clear();
        builder.Services.AddUnique(_sink);
    }

    [Test]
    public async Task Unpublishing_Element_Refreshes_Its_Cache()
    {
        var elementType = await CreateElementType();
        var elementKey = await CreateAndPublishElement(elementType.Key);
        var elementId = ElementId(elementKey);

        _sink.Payloads.Clear();

        var result = await ElementPublishingService.UnpublishAsync(elementKey, null, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        AssertElementCacheRefreshed(elementId);
    }

    [Test]
    public async Task Trashing_Element_Refreshes_Its_Cache()
    {
        var elementType = await CreateElementType();
        var elementKey = await CreateAndPublishElement(elementType.Key);
        var elementId = ElementId(elementKey);

        _sink.Payloads.Clear();

        var result = await ElementEditingService.MoveToRecycleBinAsync(elementKey, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        AssertElementCacheRefreshed(elementId);
    }

    private async Task<IContentType> CreateElementType()
    {
        var elementType = new ContentTypeBuilder()
            .WithAlias("myElementType")
            .WithName("My Element Type")
            .WithIsElement(true)
            .WithAllowedInLibrary(true)
            .WithContentVariation(ContentVariation.Nothing)
            .AddPropertyType()
            .WithAlias("text")
            .WithName("Text")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithVariations(ContentVariation.Nothing)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);
        return elementType;
    }

    private async Task<Guid> CreateAndPublishElement(Guid elementTypeKey)
    {
        var createResult = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = elementTypeKey,
                ParentKey = null,
                Properties = [new PropertyValueModel { Alias = "text", Value = "The reusable text" }],
                Variants = [new VariantModel { Name = "Reusable element" }],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success);

        var elementKey = createResult.Result.Content!.Key;

        var publishResult = await ElementPublishingService.PublishAsync(
            elementKey,
            [new CulturePublishScheduleModel { Culture = null }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishResult.Success);

        return elementKey;
    }

    private static void AssertElementCacheRefreshed(int elementId)
        => Assert.IsTrue(
            _sink.Payloads.Any(p => p.Id == elementId
                && (p.ChangeTypes.HasType(TreeChangeTypes.RefreshNode)
                    || p.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch)
                    || p.ChangeTypes.HasType(TreeChangeTypes.Remove))),
            $"Expected an element cache refresh for element {elementId} with a node-level change type.");

    private sealed class ProbeSink
    {
        public List<ElementCacheRefresher.JsonPayload> Payloads { get; } = new();
    }

    private sealed class Probe : INotificationHandler<ElementCacheRefresherNotification>
    {
        private readonly ProbeSink _sink;

        public Probe(ProbeSink sink) => _sink = sink;

        public void Handle(ElementCacheRefresherNotification notification)
        {
            if (notification.MessageType == MessageType.RefreshByPayload)
            {
                _sink.Payloads.AddRange((ElementCacheRefresher.JsonPayload[])notification.MessageObject);
            }
        }
    }
}
