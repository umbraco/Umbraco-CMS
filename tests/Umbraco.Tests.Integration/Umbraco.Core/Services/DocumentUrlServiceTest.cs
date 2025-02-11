using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Mock)]
public class DocumentUrlServiceTest : UmbracoIntegrationTestWithContent
{
    protected IDocumentUrlService DocumentUrlService => GetRequiredService<IDocumentUrlService>();
    protected ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IServerMessenger, ScopedRepositoryTests.LocalServerMessenger>();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();

        builder.Services.AddNotificationAsyncHandler<UmbracoApplicationStartingNotification, DocumentUrlServiceInitializerNotificationHandler>();
    }


    public override void Setup()
    {
        DocumentUrlService.InitAsync(false, CancellationToken.None).GetAwaiter().GetResult();
        base.Setup();
    }
    //
    // [Test]
    // [LongRunning]
    // public async Task InitAsync()
    // {
    //     // ContentService.PublishBranch(Textpage, true, []);
    //     //
    //     // for (int i = 3; i < 10; i++)
    //     // {
    //     //     var unusedSubPage = ContentBuilder.CreateSimpleContent(ContentType, "Text Page " + i, Textpage.Id);
    //     //     unusedSubPage.Key = Guid.NewGuid();
    //     //     ContentService.Save(unusedSubPage);
    //     //     ContentService.Publish(unusedSubPage, new string[0]);
    //     // }
    //     //
    //     // await DocumentUrlService.InitAsync(CancellationToken.None);
    //
    // }

    [Test]
    public async Task Trashed_documents_do_not_have_a_url_segment()
    {
        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        Assert.IsNull(DocumentUrlService.GetUrlSegment(Trashed.Key, isoCode, true));
        Assert.IsNull(DocumentUrlService.GetUrlSegment(Trashed.Key, isoCode, false));

    }

    //TODO test with the urlsegment property value!

    [Test]
    public async Task Deleted_documents_do_not_have_a_url_segment__Parent_deleted()
    {
        ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);

        ContentService.Delete(Textpage);

        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        var actual = DocumentUrlService.GetUrlSegment(Subpage2.Key, isoCode, false);

        Assert.IsNull(actual);
    }

    [Test]
    public async Task Deleted_documents_do_not_have_a_url_segment()
    {
        ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);

        ContentService.Delete(Subpage2);

        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        var actual = DocumentUrlService.GetUrlSegment(Subpage2.Key, isoCode, false);

        Assert.IsNull(actual);
    }

    [Test]
    [TestCase("/", "en-US", true, ExpectedResult = TextpageKey)]
    [TestCase("/text-page-1", "en-US", true, ExpectedResult = SubPageKey)]
    [TestCase("/text-page-2", "en-US", true, ExpectedResult = SubPage2Key)]
    [TestCase("/text-page-3", "en-US", true, ExpectedResult = SubPage3Key)]
    [TestCase("/", "en-US", false, ExpectedResult = TextpageKey)]
    [TestCase("/text-page-1", "en-US", false, ExpectedResult = SubPageKey)]
    [TestCase("/text-page-2", "en-US", false, ExpectedResult = SubPage2Key)]
    [TestCase("/text-page-3", "en-US", false, ExpectedResult = SubPage3Key)]
    public string? Expected_Routes(string route, string isoCode, bool loadDraft)
    {
        if (loadDraft is false)
        {
            ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);
        }

        return DocumentUrlService.GetDocumentKeyByRoute(route, isoCode,  null, loadDraft)?.ToString()?.ToUpper();
    }

    [Test]
    public void No_Published_Route_when_not_published()
    {
        Assert.IsNotNull(DocumentUrlService.GetDocumentKeyByRoute("/text-page-1", "en-US", null, true));
        Assert.IsNull(DocumentUrlService.GetDocumentKeyByRoute("/text-page-1", "en-US", null, false));
    }

    [Test]
    public void Unpublished_Pages_Are_not_available()
    {
        //Arrange
        ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(DocumentUrlService.GetDocumentKeyByRoute("/", "en-US", null, true));
            Assert.IsNotNull(DocumentUrlService.GetDocumentKeyByRoute("/", "en-US", null, false));
            Assert.IsNotNull(DocumentUrlService.GetDocumentKeyByRoute("/text-page-1", "en-US", null, true));
            Assert.IsNotNull(DocumentUrlService.GetDocumentKeyByRoute("/text-page-1", "en-US", null, false));
        });

        //Act
        ContentService.Unpublish(Textpage );

        Assert.Multiple(() =>
        {
            //The unpublished page self
            Assert.IsNotNull(DocumentUrlService.GetDocumentKeyByRoute("/", "en-US", null, true));
            Assert.IsNull(DocumentUrlService.GetDocumentKeyByRoute("/", "en-US", null, false));

            //A descendant of the unpublished page
            Assert.IsNotNull(DocumentUrlService.GetDocumentKeyByRoute("/text-page-1", "en-US", null, true));
            Assert.IsNull(DocumentUrlService.GetDocumentKeyByRoute("/text-page-1", "en-US", null, false));
        });

    }


    [Test]
    [TestCase("/text-page-1/sub-page-1", "en-US", true, ExpectedResult = "DF49F477-12F2-4E33-8563-91A7CC1DCDBB")]
    [TestCase("/text-page-1/sub-page-1", "en-US", false, ExpectedResult = "DF49F477-12F2-4E33-8563-91A7CC1DCDBB")]
    public string? Expected_Routes_with_subpages(string route, string isoCode, bool loadDraft)
    {
        // Create a subpage
        var subsubpage = ContentBuilder.CreateSimpleContent(ContentType, "Sub Page 1", Subpage.Id);
        subsubpage.Key = new Guid("DF49F477-12F2-4E33-8563-91A7CC1DCDBB");
        var contentSchedule = ContentScheduleCollection.CreateWithEntry(DateTime.Now.AddMinutes(-5), null);
        ContentService.Save(subsubpage, -1, contentSchedule);

        if (loadDraft is false)
        {
            ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);
        }

        return DocumentUrlService.GetDocumentKeyByRoute(route, isoCode,  null, loadDraft)?.ToString()?.ToUpper();
    }

    [Test]
    [TestCase("/second-root", "en-US", true, ExpectedResult = "8E21BCD4-02CA-483D-84B0-1FC92702E198")]
    [TestCase("/second-root", "en-US", false, ExpectedResult = "8E21BCD4-02CA-483D-84B0-1FC92702E198")]
    public string? Second_root_cannot_hide_url(string route, string isoCode, bool loadDraft)
    {
        // Create a second root
        var secondRoot = ContentBuilder.CreateSimpleContent(ContentType, "Second Root", null);
        secondRoot.Key = new Guid("8E21BCD4-02CA-483D-84B0-1FC92702E198");
        var contentSchedule = ContentScheduleCollection.CreateWithEntry(DateTime.Now.AddMinutes(-5), null);
        ContentService.Save(secondRoot, -1, contentSchedule);

        if (loadDraft is false)
        {
            ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);
            ContentService.PublishBranch(secondRoot, PublishBranchFilter.IncludeUnpublished, ["*"]);
        }

        return DocumentUrlService.GetDocumentKeyByRoute(route, isoCode,  null, loadDraft)?.ToString()?.ToUpper();
    }

    [Test]
    [TestCase("/child-of-second-root", "en-US", true, ExpectedResult = "FF6654FB-BC68-4A65-8C6C-135567F50BD6")]
    [TestCase("/child-of-second-root", "en-US", false, ExpectedResult = "FF6654FB-BC68-4A65-8C6C-135567F50BD6")]
    public string? Child_of_second_root_do_not_have_parents_url_as_prefix(string route, string isoCode, bool loadDraft)
    {
        // Create a second root
        var secondRoot = ContentBuilder.CreateSimpleContent(ContentType, "Second Root", null);
        var contentSchedule = ContentScheduleCollection.CreateWithEntry(DateTime.Now.AddMinutes(-5), null);
        ContentService.Save(secondRoot, -1, contentSchedule);

        // Create a child of second root
        var childOfSecondRoot = ContentBuilder.CreateSimpleContent(ContentType, "Child of Second Root", secondRoot);
        childOfSecondRoot.Key = new Guid("FF6654FB-BC68-4A65-8C6C-135567F50BD6");
        ContentService.Save(childOfSecondRoot, -1, contentSchedule);

        // Publish both the main root and the second root with descendants
        if (loadDraft is false)
        {

            ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);
            ContentService.PublishBranch(secondRoot, PublishBranchFilter.IncludeUnpublished, ["*"]);
        }

        return DocumentUrlService.GetDocumentKeyByRoute(route, isoCode,  null, loadDraft)?.ToString()?.ToUpper();
    }


    //TODO test cases:
    // - Find the root, when a domain is set
    // - Find a nested child, when a domain is set

    // - Find the root when no domain is set and hideTopLevelNodeFromPath is true
    // - Find a nested child of item in the root top when no domain is set and hideTopLevelNodeFromPath is true
    // - Find a nested child of item in the root bottom when no domain is set and hideTopLevelNodeFromPath is true
    // - Find the root when no domain is set and hideTopLevelNodeFromPath is false
    // - Find a nested child of item in the root top when no domain is set and hideTopLevelNodeFromPath is false
    // - Find a nested child of item in the root bottom when no domain is set and hideTopLevelNodeFromPath is false

    // - All of the above when having Constants.Conventions.Content.UrlName set to a value
}
