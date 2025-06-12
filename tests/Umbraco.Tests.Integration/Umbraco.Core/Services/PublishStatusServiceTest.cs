using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Handlers;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.DependencyInjection;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.Examine.DependencyInjection;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Mock)]
public class PublishStatusServiceTest : UmbracoIntegrationTestWithContent
{
    protected IPublishStatusQueryService PublishStatusQueryService => GetRequiredService<IPublishStatusQueryService>();

    private const string DefaultCulture = "en-US";
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IServerMessenger, ScopedRepositoryTests.LocalServerMessenger>();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
    }

    [Test]
    public async Task InitializeAsync_loads_from_db()
    {
        var randomCulture = "da-DK";
        var sut = new PublishStatusService(
            GetRequiredService<ILogger<PublishStatusService>>(),
            GetRequiredService<IPublishStatusRepository>(),
            GetRequiredService<ICoreScopeProvider>());

        Assert.Multiple(() =>
        {
            Assert.IsFalse(sut.IsDocumentPublished(Textpage.Key, DefaultCulture));
            Assert.IsFalse(sut.IsDocumentPublished(Subpage2.Key, DefaultCulture));
            Assert.IsFalse(sut.IsDocumentPublished(Subpage.Key, DefaultCulture));
            Assert.IsFalse(sut.IsDocumentPublished(Subpage2.Key, DefaultCulture));
            Assert.IsFalse(sut.IsDocumentPublished(Subpage3.Key, DefaultCulture));

            Assert.IsFalse(sut.IsDocumentPublished(Trashed.Key, DefaultCulture));

            Assert.IsFalse(sut.IsDocumentPublished(Textpage.Key, randomCulture));
            Assert.IsFalse(sut.IsDocumentPublished(Subpage2.Key, randomCulture));
            Assert.IsFalse(sut.IsDocumentPublished(Subpage.Key, randomCulture));
            Assert.IsFalse(sut.IsDocumentPublished(Subpage2.Key, randomCulture));
            Assert.IsFalse(sut.IsDocumentPublished(Subpage3.Key, randomCulture));

            Assert.IsFalse(sut.IsDocumentPublished(Trashed.Key, randomCulture));
        });

        // Act
        var publishResults = ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);
        await sut.InitializeAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(publishResults.All(x=>x.Result == PublishResultType.SuccessPublish));
            Assert.IsTrue(sut.IsDocumentPublished(Textpage.Key, DefaultCulture));
            Assert.IsTrue(sut.IsDocumentPublished(Subpage2.Key, DefaultCulture));
            Assert.IsTrue(sut.IsDocumentPublished(Subpage.Key, DefaultCulture));
            Assert.IsTrue(sut.IsDocumentPublished(Subpage2.Key, DefaultCulture));
            Assert.IsTrue(sut.IsDocumentPublished(Subpage3.Key, DefaultCulture));

            Assert.IsFalse(sut.IsDocumentPublished(Trashed.Key, DefaultCulture));

            Assert.IsFalse(sut.IsDocumentPublished(Textpage.Key, randomCulture));
            Assert.IsFalse(sut.IsDocumentPublished(Subpage2.Key, randomCulture));
            Assert.IsFalse(sut.IsDocumentPublished(Subpage.Key, randomCulture));
            Assert.IsFalse(sut.IsDocumentPublished(Subpage2.Key, randomCulture));
            Assert.IsFalse(sut.IsDocumentPublished(Subpage3.Key, randomCulture));

            Assert.IsFalse(sut.IsDocumentPublished(Trashed.Key, randomCulture));
        });
    }

    [Test]
    public async Task AddOrUpdateStatusWithDescendantsAsync()
    {
        var randomCulture = "da-DK";
        var sut = new PublishStatusService(
            GetRequiredService<ILogger<PublishStatusService>>(),
            GetRequiredService<IPublishStatusRepository>(),
            GetRequiredService<ICoreScopeProvider>(),
            GetRequiredService<ILanguageService>()
            );

        Assert.IsFalse(sut.IsDocumentPublished(Textpage.Key, DefaultCulture));

        // Act
        var publishResults = ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);
        await sut.AddOrUpdateStatusWithDescendantsAsync(Textpage.Key, CancellationToken.None);
        Assert.IsTrue(sut.IsDocumentPublished(Textpage.Key, DefaultCulture));
        Assert.IsTrue(sut.IsDocumentPublished(Subpage.Key, DefaultCulture)); // Updated due to being an descendant
        Assert.IsFalse(sut.IsDocumentPublished(Textpage.Key, randomCulture)); // Do not exist
        Assert.IsFalse(sut.IsDocumentPublished(Subpage.Key, randomCulture)); // Do not exist
    }

    [Test]
    public async Task AddOrUpdateStatusAsync()
    {
        var randomCulture = "da-DK";
        var sut = new PublishStatusService(
            GetRequiredService<ILogger<PublishStatusService>>(),
            GetRequiredService<IPublishStatusRepository>(),
            GetRequiredService<ICoreScopeProvider>(),
            GetRequiredService<ILanguageService>());

        Assert.IsFalse(sut.IsDocumentPublished(Textpage.Key, DefaultCulture));

        // Act
        var publishResults = ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);
        await sut.AddOrUpdateStatusAsync(Textpage.Key, CancellationToken.None);
        Assert.IsTrue(sut.IsDocumentPublished(Textpage.Key, DefaultCulture));
        Assert.IsFalse(sut.IsDocumentPublished(Subpage.Key, DefaultCulture)); // Not updated
        Assert.IsFalse(sut.IsDocumentPublished(Textpage.Key, randomCulture)); // Do not exist
        Assert.IsFalse(sut.IsDocumentPublished(Subpage.Key, randomCulture)); // Do not exist
    }

    [Test]
    public void When_Nothing_is_publised_all_return_false()
    {
        var randomCulture = "da-DK";
        Assert.Multiple(() =>
        {
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Textpage.Key, DefaultCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Subpage2.Key, DefaultCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Subpage.Key, DefaultCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Subpage2.Key, DefaultCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Subpage3.Key, DefaultCulture));

            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Trashed.Key, DefaultCulture));

            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Textpage.Key, randomCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Subpage2.Key, randomCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Subpage.Key, randomCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Subpage2.Key, randomCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Subpage3.Key, randomCulture));

            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Trashed.Key, randomCulture));
        });

    }

    [Test]
    public void Unpublish_leads_to_unpublised_in_this_service()
    {
        var grandchild = ContentBuilder.CreateSimpleContent(ContentType, "Grandchild", Subpage2.Id);

        var contentSchedule = ContentScheduleCollection.CreateWithEntry(DateTime.UtcNow.AddMinutes(-5), null);
        ContentService.Save(grandchild, -1, contentSchedule);

        var publishResults = ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);
        var randomCulture = "da-DK";

        var subPage2FromDB = ContentService.GetById(Subpage2.Key);
        var publishResult = ContentService.Unpublish(subPage2FromDB);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(publishResults.All(x=>x.Result == PublishResultType.SuccessPublish));
            Assert.IsTrue(publishResult.Success);
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublished(Textpage.Key, DefaultCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Subpage2.Key, DefaultCulture));
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublished(grandchild.Key, DefaultCulture)); // grandchild is still published, but it will not be routable

            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Textpage.Key, randomCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Subpage2.Key, randomCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(grandchild.Key, randomCulture));
        });

    }

     [Test]
    public void When_Branch_is_publised_default_language_return_true()
    {
        var publishResults = ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);
        var randomCulture = "da-DK";
        Assert.Multiple(() =>
        {
            Assert.IsTrue(publishResults.All(x=>x.Result == PublishResultType.SuccessPublish));
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublished(Textpage.Key, DefaultCulture));
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublished(Subpage2.Key, DefaultCulture));
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublished(Subpage.Key, DefaultCulture));
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublished(Subpage2.Key, DefaultCulture));
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublished(Subpage3.Key, DefaultCulture));

            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Trashed.Key, DefaultCulture));

            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Textpage.Key, randomCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Subpage2.Key, randomCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Subpage.Key, randomCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Subpage2.Key, randomCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Subpage3.Key, randomCulture));

            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Trashed.Key, randomCulture));
        });

    }
}
