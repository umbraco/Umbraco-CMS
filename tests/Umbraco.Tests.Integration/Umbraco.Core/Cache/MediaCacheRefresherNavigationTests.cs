// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Cache;

/// <summary>
///     See <see cref="ContentCacheRefresherNavigationTests" />; media created by the fixture is not in navigation either.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MediaCacheRefresherNavigationTests : UmbracoIntegrationTest
{
    private MediaCacheRefresher MediaCacheRefresher => GetRequiredService<MediaCacheRefresher>();

    private IMediaNavigationQueryService NavigationQueryService => GetRequiredService<IMediaNavigationQueryService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    [Test]
    public async Task Refresh_Adds_The_Missing_Parent_Before_The_Node()
    {
        MediaType mediaType = MediaTypeBuilder.CreateSimpleMediaType("navigationTestFolder", "Navigation test folder");
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);
        Media folder = MediaBuilder.CreateMediaFolder(mediaType, -1);
        MediaService.Save(folder);
        Media child = MediaBuilder.CreateMediaFolder(mediaType, folder.Id);
        MediaService.Save(child);
        Assume.That(NavigationQueryService.TryGetParentKey(folder.Key, out _), Is.False, "The parent is already in navigation, so this test proves nothing.");

        MediaCacheRefresher.Refresh([new MediaCacheRefresher.JsonPayload(child.Id, child.Key, TreeChangeTypes.RefreshNode)]);

        Assert.Multiple(() =>
        {
            Assert.That(NavigationQueryService.TryGetParentKey(child.Key, out Guid? parentKey), Is.True, "The node was not added to navigation.");
            Assert.That(parentKey, Is.EqualTo(folder.Key));
            Assert.That(NavigationQueryService.TryGetParentKey(folder.Key, out Guid? grandparentKey), Is.True, "The missing parent was not added to navigation.");
            Assert.That(grandparentKey, Is.Null);
        });
    }

    [Test]
    public async Task Refresh_Moves_Node_To_A_Parent_Unknown_Locally()
    {
        MediaType mediaType = MediaTypeBuilder.CreateSimpleMediaType("navigationTestFolder", "Navigation test folder");
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);
        Media folder = MediaBuilder.CreateMediaFolder(mediaType, -1);
        MediaService.Save(folder);
        Media child = MediaBuilder.CreateMediaFolder(mediaType, folder.Id);
        MediaService.Save(child);
        MediaCacheRefresher.Refresh([Payload(folder), Payload(child)]);

        // Another server creates a new folder and moves the child into it; nothing runs against this server's structures.
        Media newParent = MediaBuilder.CreateMediaFolder(mediaType, -1);
        using (IScope scope = ScopeProvider.CreateScope(repositoryCacheMode: RepositoryCacheMode.None))
        {
            MediaService.Save(newParent);
            MediaService.Move(MediaService.GetById(child.Id)!, newParent.Id);
            scope.Complete();
        }

        Assume.That(NavigationQueryService.TryGetParentKey(newParent.Key, out _), Is.False, "The new parent is already in navigation, so this test proves nothing.");

        MediaCacheRefresher.Refresh([Payload(child)]);

        Assert.Multiple(() =>
        {
            Assert.That(NavigationQueryService.TryGetParentKey(child.Key, out Guid? parentKey), Is.True);
            Assert.That(parentKey, Is.EqualTo(newParent.Key), "The node was not moved under the parent that was unknown locally.");
            Assert.That(NavigationQueryService.TryGetParentKey(newParent.Key, out _), Is.True, "The missing parent was not added to navigation.");
            Assert.That(NavigationQueryService.TryGetChildrenKeys(folder.Key, out IEnumerable<Guid> oldSiblings), Is.True);
            Assert.That(oldSiblings, Does.Not.Contain(child.Key));
        });
    }

    private static MediaCacheRefresher.JsonPayload Payload(IMedia media)
        => new(media.Id, media.Key, TreeChangeTypes.RefreshNode);
}
