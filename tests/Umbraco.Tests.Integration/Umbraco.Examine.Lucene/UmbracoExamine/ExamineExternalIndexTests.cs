using Examine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Examine.DependencyInjection;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;
using Umbraco.Cms.Web.BackOffice.Security;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Examine.Lucene.UmbracoExamine;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class ExamineExternalIndexTests : ExamineBaseTest
{
    private const string ContentName = "TestContent";

    [SetUp]
    public void Setup()
    {
        TestHelper.DeleteDirectory(GetIndexPath(Constants.UmbracoIndexes.InternalIndexName));
        TestHelper.DeleteDirectory(GetIndexPath(Constants.UmbracoIndexes.ExternalIndexName));
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = Services;
        Mock.Get(TestHelper.GetHttpContextAccessor()).Setup(x => x.HttpContext).Returns(httpContext);
    }

    [TearDown]
    public void TearDown()
    {
        // When disposing examine, it does a final write, which ends up locking the file if the indexing is not done yet. So we have this wait to circumvent that.
        Thread.Sleep(1500);
        // Sometimes we do not dispose all services in time and the test fails because the log file is locked. Resulting in all other tests failing as well
        Services.DisposeIfDisposable();
    }

    private IExamineExternalIndexSearcherTest ExamineExternalIndexSearcher =>
        GetRequiredService<IExamineExternalIndexSearcherTest>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private ContentService ContentService => (ContentService)GetRequiredService<IContentService>();

    private IUserStore<BackOfficeIdentityUser> BackOfficeUserStore =>
        GetRequiredService<IUserStore<BackOfficeIdentityUser>>();

    private IBackOfficeSignInManager BackOfficeSignInManager => GetRequiredService<IBackOfficeSignInManager>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IExamineExternalIndexSearcherTest, ExamineExternalIndexSearcherTest>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
        builder
            .AddNotificationHandler<ContentTreeChangeNotification,
                ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<ContentCacheRefresherNotification, ContentIndexingNotificationHandler>();
        builder.AddExamineIndexes();
        builder.AddBackOfficeIdentity();
        builder.Services.AddHostedService<QueuedHostedService>();
    }

    private IEnumerable<ISearchResult> ExamineExternalIndexSearch(string query, int pageSize = 20, int pageIndex = 0) =>
        ExamineExternalIndexSearcher.Search(query, UmbracoEntityTypes.Document,
            pageSize, pageIndex, out _, ignoreUserStartNodes: true);

    private async Task SetupUserIdentity(string userId)
    {
        var identity =
            await BackOfficeUserStore.FindByIdAsync(userId, CancellationToken.None);
        await BackOfficeSignInManager.SignInAsync(identity, false);
    }

    [Test]
    public async Task Search_Published_Content_With_Query_By_Content_Name()
    {
        // Arrange
        await SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        var contentType = new ContentTypeBuilder()
            .WithId(0)
            .Build();
        ContentTypeService.Save(contentType);

        var content = new ContentBuilder()
            .WithId(0)
            .WithName(ContentName)
            .WithContentType(contentType)
            .Build();
        await ExecuteAndWaitForIndexing(() => ContentService.SaveAndPublish(content), Constants.UmbracoIndexes.ExternalIndexName);

        // Act
        IEnumerable<ISearchResult> actual = ExamineExternalIndexSearch(ContentName);

        // Assert
        IEnumerable<ISearchResult> searchResults = actual.ToArray();
        Assert.AreEqual(1, searchResults.Count());
        Assert.AreEqual(searchResults.First().Values["nodeName"], ContentName);
    }

    [Test]
    public async Task Search_Unpublished_Content_With_Query_By_Content_Name()
    {
        // Arrange
        await SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        var contentType = new ContentTypeBuilder()
            .WithId(0)
            .Build();
        ContentTypeService.Save(contentType);

        var content = new ContentBuilder()
            .WithId(0)
            .WithName(ContentName)
            .WithContentType(contentType)
            .Build();
        await ExecuteAndWaitForIndexing(() => ContentService.Save(content), Constants.UmbracoIndexes.ExternalIndexName);

        // Act
        IEnumerable<ISearchResult> actual = ExamineExternalIndexSearch(ContentName);

        // Assert
        Assert.AreEqual(0, actual.Count());
    }
}
