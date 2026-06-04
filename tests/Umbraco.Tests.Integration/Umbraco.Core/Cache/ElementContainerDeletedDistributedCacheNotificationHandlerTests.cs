using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Cache;

/// <summary>
/// Tests for <see cref="ElementContainerDeletedDistributedCacheNotificationHandler"/>.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, WithApplication = true)]
internal sealed class ElementContainerDeletedDistributedCacheNotificationHandlerTests : UmbracoIntegrationTest
{
    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IElementService ElementService => GetRequiredService<IElementService>();

    private IEntityService EntityService => GetRequiredService<IEntityService>();

    private static readonly UmbracoObjectTypes[] _treeObjectTypes =
        [UmbracoObjectTypes.ElementContainer, UmbracoObjectTypes.Element];

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        // Integration tests use a no-op server messenger and do not register the distributed cache
        // notification handlers by default, so opt in to the element handlers under test and a messenger
        // that delivers cache refreshes locally.
        builder.AddNotificationHandler<ElementTreeChangeNotification, ElementTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<EntityContainerDeletedNotification, ElementContainerDeletedDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    /// <summary>
    /// Regression test for https://github.com/umbraco/Umbraco-CMS/issues/23072: the element tree's children
    /// query resolves the container key to an id via <see cref="IIdKeyMap"/>. When a container is deleted the
    /// handler must evict its mapping, otherwise a container recreated under the same key resolves to the old
    /// (now non-existent) id and nested elements stay invisible in the tree until the application is restarted.
    /// </summary>
    [Test]
    public async Task Child_Element_Is_Returned_After_Container_Recreated_Under_Same_Key()
    {
        IContentType elementType = await CreateElementTypeAsync();
        var containerKey = Guid.NewGuid();

        // Create the container and resolve its children once, so its key->id mapping is cached in IdKeyMap.
        EntityContainer firstContainer = await CreateContainerAsync(containerKey, "Container v1");
        Attempt<int> warmResolve = IdKeyMap.GetIdForKey(containerKey, UmbracoObjectTypes.ElementContainer);
        Assert.AreEqual(firstContainer.Id, warmResolve.Result);

        // Delete and recreate under the same key - the recreated container gets a new id.
        Attempt<EntityContainer?, EntityContainerOperationStatus> deleteResult =
            await ElementContainerService.DeleteAsync(containerKey, Constants.Security.SuperUserKey);
        Assert.IsTrue(deleteResult.Success, $"Failed to delete container: {deleteResult.Status}");

        EntityContainer secondContainer = await CreateContainerAsync(containerKey, "Container v2");
        Assert.AreNotEqual(firstContainer.Id, secondContainer.Id, "Recreated container should have a new id.");

        IElement element = CreateElementUnder(secondContainer.Id, elementType);

        // Without the fix, the stale containerKey->firstContainer.Id mapping survives and the children query
        // resolves to the old (now non-existent) parent id, returning nothing.
        Attempt<int> resolvedAfter = IdKeyMap.GetIdForKey(containerKey, UmbracoObjectTypes.ElementContainer);
        Assert.AreEqual(secondContainer.Id, resolvedAfter.Result, "Container key should resolve to the recreated container id.");

        AssertChildrenContains(containerKey, element.Key);
    }

    private void AssertChildrenContains(Guid containerKey, Guid expectedElementKey)
    {
        IEntitySlim[] children = EntityService
            .GetPagedChildren(containerKey, _treeObjectTypes, _treeObjectTypes, 0, 100, false, out var total)
            .ToArray();

        Assert.AreEqual(1, total, "Expected the element tree children query to return the nested element.");
        Assert.IsTrue(children.Any(child => child.Key == expectedElementKey), "Nested element was not returned by the children query.");
    }

    private async Task<IContentType> CreateElementTypeAsync()
    {
        IContentType elementType = ContentTypeBuilder.CreateSimpleElementType();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);
        return elementType;
    }

    private async Task<EntityContainer> CreateContainerAsync(Guid key, string name)
    {
        Attempt<EntityContainer?, EntityContainerOperationStatus> result =
            await ElementContainerService.CreateAsync(key, name, null, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success, $"Failed to create container: {result.Status}");
        return result.Result!;
    }

    private IElement CreateElementUnder(int parentId, IContentType elementType)
    {
        var element = new Element($"Element {Guid.NewGuid():N}", parentId, elementType);
        OperationResult saveResult = ElementService.Save(element);
        Assert.IsTrue(saveResult.Success, "Failed to save element.");
        return element;
    }
}
