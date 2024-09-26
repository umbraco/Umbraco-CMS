using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class MediaTest : UmbracoIntegrationTestWithMediaEditing
{

    private IPublishedMediaCache PublishedMediaHybridCache => GetRequiredService<IPublishedMediaCache>();

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();


    [Test]
    public async Task Can_Get_Root_Media_By_Id()
    {
        // Act
        var media = await PublishedMediaHybridCache.GetByIdAsync(RootFolderId);

        // Assert
        Assert.IsNotNull(media);
        Assert.AreEqual("RootFolder", media.Name);
        Assert.AreEqual(RootFolder.ContentTypeKey, media.ContentType.Key);
    }

    [Test]
    public async Task Can_Get_Root_Media_By_Key()
    {
        var media = await PublishedMediaHybridCache.GetByIdAsync(RootFolder.Key.Value);

        // Assert
        Assert.IsNotNull(media);
        Assert.AreEqual("RootFolder", media.Name);
        Assert.AreEqual(RootFolder.ContentTypeKey, media.ContentType.Key);
    }

    [Test]
    public async Task Can_Get_Child_Media_By_Id()
    {
        // Act
        var media = await PublishedMediaHybridCache.GetByIdAsync(SubFolder1Id);

        // Assert
        Assert.IsNotNull(media);
        Assert.AreEqual("SubFolder1", media.Name);
        Assert.AreEqual(SubFolder1.ContentTypeKey, media.ContentType.Key);
        Assert.AreEqual(SubFolder1.ParentKey, RootFolder.Key);
    }

    [Test]
    public async Task Can_Get_Child_Media_By_Key()
    {
        var media = await PublishedMediaHybridCache.GetByIdAsync(SubFolder1.Key.Value);

        // Assert
        Assert.IsNotNull(media);
        Assert.AreEqual("SubFolder1", media.Name);
        Assert.AreEqual(SubFolder1.ContentTypeKey, media.ContentType.Key);
        Assert.AreEqual(SubFolder1.ParentKey, RootFolder.Key);
    }

}
