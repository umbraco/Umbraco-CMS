using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;
using IUmbracoBuilder = Umbraco.Cms.Core.DependencyInjection.IUmbracoBuilder;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Search;

internal sealed class ElementReindexTriggerTests : BlockEditorWithReusableContentTestBase
{
    private static readonly ProbeSink _sink = new();

    private int ElementId(Guid key) => IdKeyMap.GetIdForKey(key, UmbracoObjectTypes.Element).Result;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        // Capture the element cache refresher payloads so we can assert publish/unpublish/trash fire it with the
        // element id and a change type the reindex handler acts on. This is the integration point the handler relies on.
        _sink.Payloads.Clear();
        builder.Services.AddUnique(_sink);

        // The binder that turns element tree changes into a distributed element cache refresh; the minimal test base
        // does not register it, so wire it here so the refresher (and our probe) actually fire.
        builder.AddNotificationHandler<ElementTreeChangeNotification, ElementTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<ElementCacheRefresherNotification, Probe>();
    }

    [Test]
    public async Task Unpublishing_Element_Refreshes_Its_Cache_With_A_Reindexable_Change()
    {
        var elementType = await CreateElementType(ContentVariation.Nothing);
        var elementKey = await CreateAndPublishInvariantReusableElement(elementType.Key);
        var elementId = ElementId(elementKey);

        _sink.Payloads.Clear();

        var result = await ElementPublishingService.UnpublishAsync(elementKey, null, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        AssertReindexableChangeFor(elementId);
    }

    [Test]
    public async Task Trashing_Element_Refreshes_Its_Cache_With_A_Reindexable_Change()
    {
        var elementType = await CreateElementType(ContentVariation.Nothing);
        var elementKey = await CreateAndPublishInvariantReusableElement(elementType.Key);
        var elementId = ElementId(elementKey);

        _sink.Payloads.Clear();

        var result = await ElementEditingService.MoveToRecycleBinAsync(elementKey, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        AssertReindexableChangeFor(elementId);
    }

    private static void AssertReindexableChangeFor(int elementId)
        => Assert.IsTrue(
            _sink.Payloads.Any(p => p.Id == elementId
                && (p.ChangeTypes.HasType(TreeChangeTypes.RefreshNode)
                    || p.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch)
                    || p.ChangeTypes.HasType(TreeChangeTypes.Remove))),
            $"Expected an element cache refresh for element {elementId} with a reindexable change type.");

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
