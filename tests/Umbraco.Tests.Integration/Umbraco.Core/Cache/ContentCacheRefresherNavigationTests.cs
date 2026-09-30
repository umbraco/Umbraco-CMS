// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Cache;

/// <summary>
///     Navigation is rebuilt once at startup and afterwards only updated through the cache refresher. The test host's
///     messenger delivers nothing, so content created by the fixture is not in navigation: the same state a server is in
///     when another server created the parent and the instruction has not been processed here yet.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ContentCacheRefresherNavigationTests : UmbracoIntegrationTestWithContent
{
    private ContentCacheRefresher ContentCacheRefresher => GetRequiredService<ContentCacheRefresher>();

    private IDocumentNavigationQueryService NavigationQueryService => GetRequiredService<IDocumentNavigationQueryService>();

    [Test]
    public void Refresh_Adds_The_Missing_Parent_Before_The_Node()
    {
        Assume.That(NavigationQueryService.TryGetParentKey(Textpage.Key, out _), Is.False, "The parent is already in navigation, so this test proves nothing.");

        ContentCacheRefresher.Refresh([RefreshNode(Subpage)]);

        Assert.Multiple(() =>
        {
            Assert.That(NavigationQueryService.TryGetParentKey(Subpage.Key, out Guid? parentKey), Is.True, "The node was not added to navigation.");
            Assert.That(parentKey, Is.EqualTo(Textpage.Key));
            Assert.That(NavigationQueryService.TryGetParentKey(Textpage.Key, out Guid? grandparentKey), Is.True, "The missing parent was not added to navigation.");
            Assert.That(grandparentKey, Is.Null);
        });
    }

    [Test]
    public void Refresh_Adds_All_Missing_Ancestors_Root_First()
    {
        Content grandchild = ContentBuilder.CreateSimpleContent(ContentType, "Grandchild", Subpage.Id);
        ContentService.Save(grandchild);
        Assume.That(NavigationQueryService.TryGetParentKey(Subpage.Key, out _), Is.False, "The ancestors are already in navigation, so this test proves nothing.");

        ContentCacheRefresher.Refresh([RefreshNode(grandchild)]);

        Assert.Multiple(() =>
        {
            Assert.That(NavigationQueryService.TryGetParentKey(grandchild.Key, out Guid? parentKey), Is.True);
            Assert.That(parentKey, Is.EqualTo(Subpage.Key));
            Assert.That(NavigationQueryService.TryGetParentKey(Subpage.Key, out Guid? grandparentKey), Is.True);
            Assert.That(grandparentKey, Is.EqualTo(Textpage.Key));
            Assert.That(NavigationQueryService.TryGetParentKey(Textpage.Key, out Guid? rootKey), Is.True);
            Assert.That(rootKey, Is.Null);
            Assert.That(NavigationQueryService.TryGetChildrenKeys(Textpage.Key, out IEnumerable<Guid> children), Is.True);
            Assert.That(children.Count(x => x == Subpage.Key), Is.EqualTo(1));
        });
    }

    [Test]
    public void Refresh_Leaves_Navigation_Unchanged_When_The_Node_Is_Already_Known()
    {
        ContentCacheRefresher.Refresh([RefreshNode(Textpage)]);
        ContentCacheRefresher.Refresh([RefreshNode(Subpage)]);

        ContentCacheRefresher.Refresh([RefreshNode(Subpage)]);

        Assert.Multiple(() =>
        {
            Assert.That(NavigationQueryService.TryGetParentKey(Subpage.Key, out Guid? parentKey), Is.True);
            Assert.That(parentKey, Is.EqualTo(Textpage.Key));
            Assert.That(NavigationQueryService.TryGetChildrenKeys(Textpage.Key, out IEnumerable<Guid> children), Is.True);
            Assert.That(children.Count(x => x == Subpage.Key), Is.EqualTo(1));
        });
    }

    private static ContentCacheRefresher.JsonPayload RefreshNode(IContent content)
        => new() { Id = content.Id, Key = content.Key, ChangeTypes = TreeChangeTypes.RefreshNode };
}
