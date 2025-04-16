using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
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
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console)]
public class DocumentUrlServiceTests_HideTopLevel_False : UmbracoIntegrationTestWithContent
{
    protected IDocumentUrlService DocumentUrlService => GetRequiredService<IDocumentUrlService>();
    protected ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.Configure<GlobalSettings>(x => x.HideTopLevelNodeFromPath = false);

        builder.Services.AddUnique<IServerMessenger, ScopedRepositoryTests.LocalServerMessenger>();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
    }

    public override void Setup()
    {
        DocumentUrlService.InitAsync(false, CancellationToken.None).GetAwaiter().GetResult();
        base.Setup();
    }

    [Test]
    [TestCase("/textpage/", "en-US", true, ExpectedResult = TextpageKey)]
    [TestCase("/textpage/text-page-1", "en-US", true, ExpectedResult = SubPageKey)]
    [TestCase("/textpage/text-page-2", "en-US", true, ExpectedResult = SubPage2Key)]
    [TestCase("/textpage/text-page-3", "en-US", true, ExpectedResult = SubPage3Key)]
    [TestCase("/textpage/", "en-US", false, ExpectedResult = TextpageKey)]
    [TestCase("/textpage/text-page-1", "en-US", false, ExpectedResult = SubPageKey)]
    [TestCase("/textpage/text-page-2", "en-US", false, ExpectedResult = SubPage2Key)]
    [TestCase("/textpage/text-page-3", "en-US", false, ExpectedResult = SubPage3Key)]
    public string? GetDocumentKeyByRoute_Returns_Expected_Route(string route, string isoCode, bool loadDraft)
    {
        if (loadDraft is false)
        {
            ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);
        }


        return DocumentUrlService.GetDocumentKeyByRoute(route, isoCode,  null, loadDraft)?.ToString()?.ToUpper();
    }

    [Test]
    [TestCase("/textpage/text-page-1/sub-page-1", "en-US", true, ExpectedResult = "DF49F477-12F2-4E33-8563-91A7CC1DCDBB")]
    [TestCase("/textpage/text-page-1/sub-page-1", "en-US", false, ExpectedResult = "DF49F477-12F2-4E33-8563-91A7CC1DCDBB")]
    public string? GetDocumentKeyByRoute_Returns_Expected_Route_For_SubPage(string route, string isoCode, bool loadDraft)
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
    public string? GetDocumentKeyByRoute_Second_Root_Does_Not_Hide_Url(string route, string isoCode, bool loadDraft)
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
    [TestCase("/second-root/child-of-second-root", "en-US", true, ExpectedResult = "FF6654FB-BC68-4A65-8C6C-135567F50BD6")]
    [TestCase("/second-root/child-of-second-root", "en-US", false, ExpectedResult = "FF6654FB-BC68-4A65-8C6C-135567F50BD6")]
    public string? GetDocumentKeyByRoute_Child_Of_Second_Root_Does_Not_Have_Parents_Url_As_Prefix(string route, string isoCode, bool loadDraft)
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
}
