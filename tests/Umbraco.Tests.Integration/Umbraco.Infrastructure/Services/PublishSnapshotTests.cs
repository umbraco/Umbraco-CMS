using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.HybridCache.Snapshot;
using Umbraco.Cms.Persistence.EFCore.Locking;
using Umbraco.Cms.Persistence.Sqlite.Interceptors;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.DbContext;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class PublishSnapshotTests : UmbracoIntegrationTestWithContent
{
    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

    private IPublishedSnapshotService PublishedSnapshotService => GetRequiredService<IPublishedSnapshotService>();

    private IUmbracoContextFactory UmbracoContextFactory => GetRequiredService<IUmbracoContextFactory>();

    protected override void ConfigureTestServices(IServiceCollection services)
    {;
    }


    [Test]
    public async Task Can_Create_Published_Snapshot()
    {
        // Act
        var snapshot = PublishedSnapshotService.CreatePublishedSnapshot(null);
        var textPage = snapshot.Content.GetById(true, Textpage.Key);

        // Assert
        AssertTextPage(textPage);
    }

    private void AssertTextPage(IPublishedContent textPage)
    {
        Assert.Multiple(() =>
        {
            Assert.IsNotNull(textPage);
            Assert.AreEqual(Textpage.Id, textPage.Id);
            Assert.AreEqual(Textpage.Key, textPage.Key);
            Assert.AreEqual(Textpage.CreatorId, textPage.CreatorId);
            Assert.AreEqual(Textpage.CreateDate, textPage.CreateDate);
            Assert.AreEqual(Textpage.Name, textPage.Name);
        });
    }
}
