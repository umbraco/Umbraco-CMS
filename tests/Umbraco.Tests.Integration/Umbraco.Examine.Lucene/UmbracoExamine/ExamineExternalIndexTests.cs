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
using Umbraco.Cms.Infrastructure.Examine;
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
    private string _examinePath = "../../../umbraco/Data/TEMP/ExamineIndexes/";

    [SetUp]
    public void Setup()
    {
        TestHelper.DeleteDirectory(_examinePath + "InternalIndex");
        TestHelper.DeleteDirectory(_examinePath + "ExternalIndex");
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = Services;
        Mock.Get(TestHelper.GetHttpContextAccessor()).Setup(x => x.HttpContext).Returns(httpContext);
    }

    [TearDown]
    public void TearDown()
    {
        Services.DisposeIfDisposable();
        Thread.Sleep(1000);
        TestHelper.DeleteDirectory(_examinePath + "InternalIndex");
        TestHelper.DeleteDirectory(_examinePath + "ExternalIndex");
    }


    private IExamineExternalIndexSearcherTest ExamineExternalIndexSearcher =>
        GetRequiredService<IExamineExternalIndexSearcherTest>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IndexInitializer IndexInitializer => Services.GetRequiredService<IndexInitializer>();

    private IIndexRebuilder IndexRebuilder => GetRequiredService<IIndexRebuilder>();

    private ILocalizationService LocalizationService => GetRequiredService<ILocalizationService>();

    private ContentService ContentService => (ContentService)GetRequiredService<IContentService>();

    private IUserStore<BackOfficeIdentityUser> BackOfficeUserStore =>
        GetRequiredService<IUserStore<BackOfficeIdentityUser>>();

    private IBackOfficeSignInManager BackOfficeSignInManager => GetRequiredService<IBackOfficeSignInManager>();

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.AddUnique<IExamineExternalIndexSearcherTest, ExamineExternalIndexSearcherTest>();
    }

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
        builder
            .AddNotificationHandler<ContentTreeChangeNotification,
                ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<ContentCacheRefresherNotification, ContentIndexingNotificationHandler>();
        builder.AddExamineIndexes();
        builder.AddBackOfficeIdentity();
        builder.Services.AddHostedService<QueuedHostedService>();
    }

    private IEnumerable<ISearchResult> ExamineExternalIndexSearch(string query, int pageSize = 20, int pageIndex = 0)
    {
        long totalFound = long.MinValue;
        IEnumerable<ISearchResult> actual = ExamineExternalIndexSearcher.Search(query, UmbracoEntityTypes.Document,
            pageSize, pageIndex, out totalFound, ignoreUserStartNodes: true);

        return actual;
    }

    [Test]
    public async Task Search_Published_Content_With_Query_By_Content_Name()
    {
        // Arrange
        var identity =
            await BackOfficeUserStore.FindByIdAsync(Constants.Security.SuperUserIdAsString, CancellationToken.None);
        await BackOfficeSignInManager.SignInAsync(identity, false);

        string contentName = "TestContent";

        var contentType = new ContentTypeBuilder()
            .WithId(0)
            .Build();
        ContentTypeService.Save(contentType);

        var content = new ContentBuilder()
            .WithId(0)
            .WithName(contentName)
            .WithContentType(contentType)
            .Build();
        ContentService.SaveAndPublish(content);

        // It seems like we need this timeout for the moment, otherwise our indexes would not be built in time and the test could fail.
        Thread.Sleep(1000);
        string query = contentName;

        // Act
        IEnumerable<ISearchResult> actual = ExamineExternalIndexSearch(query);

        // Assert
        Assert.AreEqual(1, actual.Count());
        Assert.AreEqual(actual.First().Values["nodeName"], contentName);
    }

    [Test]
    public async Task Search_Unpublished_Content_With_Query_By_Content_Name()
    {
        // Arrange
        var identity =
            await BackOfficeUserStore.FindByIdAsync(Constants.Security.SuperUserIdAsString, CancellationToken.None);
        await BackOfficeSignInManager.SignInAsync(identity, false);

        string contentName = "TestContent";

            var contentType = new ContentTypeBuilder()
                .WithId(0)
                .Build();
            ContentTypeService.Save(contentType);

            var content = new ContentBuilder()
                .WithId(0)
                .WithName(contentName)
                .WithContentType(contentType)
                .Build();
            ContentService.Save(content);

        Thread.Sleep(1000);
        string query = contentName;

        // Act
        IEnumerable<ISearchResult> actual = ExamineExternalIndexSearch(query);

        // Assert
        Assert.AreEqual(0, actual.Count());
    }
}
