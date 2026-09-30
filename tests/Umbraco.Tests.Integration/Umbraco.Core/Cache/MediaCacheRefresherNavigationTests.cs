// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

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
    public void Refresh_Adds_The_Missing_Parent_Before_The_Node()
    {
        MediaType mediaType = MediaTypeBuilder.CreateSimpleMediaType("navigationTestFolder", "Navigation test folder");
        MediaTypeService.Save(mediaType);
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
}
