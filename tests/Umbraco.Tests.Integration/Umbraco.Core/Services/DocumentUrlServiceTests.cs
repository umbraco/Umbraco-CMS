using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Mock)]
internal sealed class DocumentUrlServiceTests : UmbracoIntegrationTestWithContent
{
    private const string SubSubPage2Key = "48AE405E-5142-4EBE-929F-55EB616F51F2";
    private const string SubSubPage3Key = "AACF2979-3F53-4184-B071-BA34D3338497";

    private IDocumentUrlService DocumentUrlService => GetRequiredService<IDocumentUrlService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IDomainService DomainService => GetRequiredService<IDomainService>();

    private IDocumentUrlRepository DocumentUrlRepository => GetRequiredService<IDocumentUrlRepository>();

    private ICoreScopeProvider CoreScopeProvider => GetRequiredService<ICoreScopeProvider>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IServerMessenger, ScopedRepositoryTests.LocalServerMessenger>();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();

        builder.Services.AddNotificationAsyncHandler<UmbracoApplicationStartingNotification, DocumentUrlServiceInitializerNotificationHandler>();

        builder.UrlSegmentProviders().Insert<CustomUrlSegmentProvider1>();
        builder.UrlSegmentProviders().Insert<CustomUrlSegmentProvider2>();
    }

    private abstract class CustomUrlSegmentProviderBase
    {
        private readonly IUrlSegmentProvider _defaultProvider;

        public CustomUrlSegmentProviderBase(IShortStringHelper stringHelper) => _defaultProvider = new DefaultUrlSegmentProvider(stringHelper);

        protected string? GetUrlSegment(IContentBase content, string? culture, params Guid[] pageKeys)
        {
            if (pageKeys.Contains(content.Key) is false)
            {
                return null;
            }

            var segment = _defaultProvider.GetUrlSegment(content, culture);
            return segment is not null ? segment + "-custom" : null;
        }
    }

    /// <summary>
    /// A test implementation of <see cref="IUrlSegmentProvider"/> that provides a custom URL segment for a specific page
    /// and allows for additional providers to provide segments too.
    /// </summary>
    private class CustomUrlSegmentProvider1 : CustomUrlSegmentProviderBase, IUrlSegmentProvider
    {
        public CustomUrlSegmentProvider1(IShortStringHelper stringHelper)
            : base(stringHelper)
        {
        }

        public bool AllowAdditionalSegments => true;

        public string? GetUrlSegment(IContentBase content, string? culture = null)
            => GetUrlSegment(content, culture, Guid.Parse(SubPageKey), Guid.Parse(SubSubPage3Key));
    }

    /// <summary>
    /// A test implementation of <see cref="IUrlSegmentProvider"/> that provides a custom URL segment for a specific page
    /// and terminates, not allowing additional providers to provide segments too.
    /// </summary>
    private class CustomUrlSegmentProvider2 : CustomUrlSegmentProviderBase, IUrlSegmentProvider
    {
        public CustomUrlSegmentProvider2(IShortStringHelper stringHelper)
            : base(stringHelper)
        {
        }

        public string? GetUrlSegment(IContentBase content, string? culture = null)
            => GetUrlSegment(content, culture, Guid.Parse(SubPage2Key), Guid.Parse(SubSubPage2Key));
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
    public async Task GetUrlSegment_For_Deleted_Document_Does_Not_Have_Url_Segment()
    {
        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        Assert.IsNull(DocumentUrlService.GetUrlSegment(Trashed.Key, isoCode, true));
        Assert.IsNull(DocumentUrlService.GetUrlSegment(Trashed.Key, isoCode, false));

    }

    //TODO test with the urlsegment property value!

    [Test]
    public async Task GetUrlSegment_For_Document_With_Parent_Deleted_Does_Not_Have_Url_Segment()
    {
        ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);

        ContentService.Delete(Textpage);

        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        var actual = DocumentUrlService.GetUrlSegment(Subpage2.Key, isoCode, false);

        Assert.IsNull(actual);
    }

    [Test]
    public async Task GetUrlSegment_For_Published_Then_Deleted_Document_Does_Not_Have_Url_Segment()
    {
        ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);

        ContentService.Delete(Subpage2);

        var isoCode = (await LanguageService.GetDefaultLanguageAsync()).IsoCode;

        var actual = DocumentUrlService.GetUrlSegment(Subpage2.Key, isoCode, false);

        Assert.IsNull(actual);
    }

    [TestCase("/", ExpectedResult = TextpageKey)]
    [TestCase("/text-page-1", ExpectedResult = SubPageKey)]
    public string? GetDocumentKeyByUri_Without_Domains_Returns_Expected_DocumentKey(string path)
    {
        ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);

        var uri = new Uri("http://example.com" + path);
        return DocumentUrlService.GetDocumentKeyByUri(uri, false)?.ToString()?.ToUpper();
    }

    private const string VariantRootPageKey = "1D3283C7-64FD-4F4D-A741-442BDA487B71";
    private const string VariantChildPageKey = "1D3283C7-64FD-4F4D-A741-442BDA487B72";

    [TestCase("/", "/en", "http://example.com/en/", ExpectedResult = VariantRootPageKey)]
    [TestCase("/child-page", "/en", "http://example.com/en/", ExpectedResult = VariantChildPageKey)]
    [TestCase("/", "example.com", "http://example.com/", ExpectedResult = VariantRootPageKey)]
    [TestCase("/child-page", "example.com", "http://example.com/", ExpectedResult = VariantChildPageKey)]
    public async Task<string?> GetDocumentKeyByUri_With_Domains_Returns_Expected_DocumentKey(string path, string domain, string rootUrl)
    {
        var template = TemplateBuilder.CreateTextPageTemplate("variantPageTemplate", "Variant Page Template");
        FileService.SaveTemplate(template);

        var contentType = new ContentTypeBuilder()
            .WithAlias("variantPage")
            .WithName("Variant Page")
            .WithContentVariation(ContentVariation.Culture)
            .WithAllowAsRoot(true)
            .WithDefaultTemplateId(template.Id)
            .Build();
        ContentTypeService.Save(contentType);

        var rootPage = new ContentBuilder()
            .WithKey(Guid.Parse(VariantRootPageKey))
            .WithContentType(contentType)
            .WithCultureName("en-US", $"Root Page")
            .Build();
        var childPage = new ContentBuilder()
            .WithKey(Guid.Parse(VariantChildPageKey))
            .WithContentType(contentType)
            .WithCultureName("en-US", $"Child Page")
            .WithParent(rootPage)
            .Build();
        ContentService.Save(rootPage, -1);
        ContentService.Save(childPage, -1);
        ContentService.PublishBranch(rootPage, PublishBranchFilter.IncludeUnpublished, ["*"]);

        var updateDomainResult = await DomainService.UpdateDomainsAsync(
            rootPage.Key,
            new DomainsUpdateModel
            {
                Domains = [new DomainModel { DomainName = domain, IsoCode = "en-US" }],
            });
        Assert.IsTrue(updateDomainResult.Success);

        var uri = new Uri(rootUrl + path);
        return DocumentUrlService.GetDocumentKeyByUri(uri, false)?.ToString()?.ToUpper();
    }

    [TestCase("/", "en-US", true, ExpectedResult = TextpageKey)]
    [TestCase("/text-page-1", "en-US", true, ExpectedResult = SubPageKey)]
    [TestCase("/text-page-1-custom", "en-US", true, ExpectedResult = SubPageKey)] // Uses the segment registered by the custom IIUrlSegmentProvider that allows for more than one segment per document.
    [TestCase("/text-page-2", "en-US", true, ExpectedResult = null)]
    [TestCase("/text-page-2-custom", "en-US", true, ExpectedResult = SubPage2Key)] // Uses the segment registered by the custom IIUrlSegmentProvider that does not allow for more than one segment per document.
    [TestCase("/text-page-3", "en-US", true, ExpectedResult = SubPage3Key)]
    [TestCase("/", "en-US", false, ExpectedResult = TextpageKey)]
    [TestCase("/text-page-1", "en-US", false, ExpectedResult = SubPageKey)]
    [TestCase("/text-page-1-custom", "en-US", false, ExpectedResult = SubPageKey)] // Uses the segment registered by the custom IIUrlSegmentProvider that allows for more than one segment per document.
    [TestCase("/text-page-2", "en-US", false, ExpectedResult = null)]
    [TestCase("/text-page-2-custom", "en-US", false, ExpectedResult = SubPage2Key)] // Uses the segment registered by the custom IIUrlSegmentProvider that does not allow for more than one segment per document.
    [TestCase("/text-page-3", "en-US", false, ExpectedResult = SubPage3Key)]
    public string? GetDocumentKeyByRoute_Returns_Expected_DocumentKey(string route, string isoCode, bool loadDraft)
    {
        if (loadDraft is false)
        {
            ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);
        }

        return DocumentUrlService.GetDocumentKeyByRoute(route, isoCode,  null, loadDraft)?.ToString()?.ToUpper();
    }

    [Test]
    public void GetDocumentKeyByRoute_UnPublished_Documents_Have_No_Published_Route()
    {
        Assert.IsNotNull(DocumentUrlService.GetDocumentKeyByRoute("/text-page-1", "en-US", null, true));
        Assert.IsNull(DocumentUrlService.GetDocumentKeyByRoute("/text-page-1", "en-US", null, false));
    }

    [Test]
    public void GetDocumentKeyByRoute_Published_Then_Unpublished_Documents_Have_No_Published_Route()
    {
        // Arrange
        ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(DocumentUrlService.GetDocumentKeyByRoute("/", "en-US", null, true));
            Assert.IsNotNull(DocumentUrlService.GetDocumentKeyByRoute("/", "en-US", null, false));
            Assert.IsNotNull(DocumentUrlService.GetDocumentKeyByRoute("/text-page-1", "en-US", null, true));
            Assert.IsNotNull(DocumentUrlService.GetDocumentKeyByRoute("/text-page-1", "en-US", null, false));
        });

        // Act
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

    [TestCase("/text-page-1/sub-page-1", "en-US", true, ExpectedResult = "DF49F477-12F2-4E33-8563-91A7CC1DCDBB")]
    [TestCase("/text-page-1/sub-page-1", "en-US", false, ExpectedResult = "DF49F477-12F2-4E33-8563-91A7CC1DCDBB")]
    public string? GetDocumentKeyByRoute_Returns_Expected_Route_For_SubPage(string route, string isoCode, bool loadDraft)
        => ExecuteSubPageTest("DF49F477-12F2-4E33-8563-91A7CC1DCDBB", "Sub Page 1", route, isoCode, loadDraft);

    [TestCase("/text-page-1/sub-page-2-custom", "en-US", true, ExpectedResult = SubSubPage2Key)]
    [TestCase("/text-page-1/sub-page-2-custom", "en-US", false, ExpectedResult = SubSubPage2Key)]
    [TestCase("/text-page-1/sub-page-2", "en-US", true, ExpectedResult = null)]
    [TestCase("/text-page-1/sub-page-2", "en-US", false, ExpectedResult = null)]
    public string? GetDocumentKeyByRoute_Returns_Expected_Route_For_SubPage_With_Terminating_Custom_Url_Provider(string route, string isoCode, bool loadDraft)
        => ExecuteSubPageTest(SubSubPage2Key, "Sub Page 2", route, isoCode, loadDraft);

    [TestCase("/text-page-1/sub-page-3-custom", "en-US", true, ExpectedResult = SubSubPage3Key)]
    [TestCase("/text-page-1/sub-page-3-custom", "en-US", false, ExpectedResult = SubSubPage3Key)]
    [TestCase("/text-page-1/sub-page-3", "en-US", true, ExpectedResult = SubSubPage3Key)]
    [TestCase("/text-page-1/sub-page-3", "en-US", false, ExpectedResult = SubSubPage3Key)]
    public string? GetDocumentKeyByRoute_Returns_Expected_Route_For_SubPage_With_Non_Terminating_Custom_Url_Provider(string route, string isoCode, bool loadDraft)
        => ExecuteSubPageTest(SubSubPage3Key, "Sub Page 3", route, isoCode, loadDraft);

    private string? ExecuteSubPageTest(string documentKey, string documentName, string route, string isoCode, bool loadDraft)
    {
        // Create a subpage
        var subsubpage = ContentBuilder.CreateSimpleContent(ContentType, documentName, Subpage.Id);
        subsubpage.Key = Guid.Parse(documentKey);
        var contentSchedule = ContentScheduleCollection.CreateWithEntry(DateTime.UtcNow.AddMinutes(-5), null);
        ContentService.Save(subsubpage, -1, contentSchedule);

        if (loadDraft is false)
        {
            ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);
        }

        return DocumentUrlService.GetDocumentKeyByRoute(route, isoCode, null, loadDraft)?.ToString()?.ToUpper();
    }

    [TestCase("/second-root", "en-US", true, ExpectedResult = "8E21BCD4-02CA-483D-84B0-1FC92702E198")]
    [TestCase("/second-root", "en-US", false, ExpectedResult = "8E21BCD4-02CA-483D-84B0-1FC92702E198")]
    public string? GetDocumentKeyByRoute_Second_Root_Does_Not_Hide_Url(string route, string isoCode, bool loadDraft)
    {
        // Create a second root
        var secondRoot = ContentBuilder.CreateSimpleContent(ContentType, "Second Root", null);
        secondRoot.Key = new Guid("8E21BCD4-02CA-483D-84B0-1FC92702E198");
        var contentSchedule = ContentScheduleCollection.CreateWithEntry(DateTime.UtcNow.AddMinutes(-5), null);
        ContentService.Save(secondRoot, -1, contentSchedule);

        if (loadDraft is false)
        {
            ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);
            ContentService.PublishBranch(secondRoot, PublishBranchFilter.IncludeUnpublished, ["*"]);
        }

        return DocumentUrlService.GetDocumentKeyByRoute(route, isoCode,  null, loadDraft)?.ToString()?.ToUpper();
    }

    [TestCase("/child-of-second-root", "en-US", true, ExpectedResult = "FF6654FB-BC68-4A65-8C6C-135567F50BD6")]
    [TestCase("/child-of-second-root", "en-US", false, ExpectedResult = "FF6654FB-BC68-4A65-8C6C-135567F50BD6")]
    public string? GetDocumentKeyByRoute_Child_Of_Second_Root_Does_Not_Have_Parents_Url_As_Prefix(string route, string isoCode, bool loadDraft)
    {
        // Create a second root
        var secondRoot = ContentBuilder.CreateSimpleContent(ContentType, "Second Root", null);
        var contentSchedule = ContentScheduleCollection.CreateWithEntry(DateTime.UtcNow.AddMinutes(-5), null);
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

    [TestCase(TextpageKey, "en-US", ExpectedResult = "/")]
    [TestCase(SubPageKey, "en-US", ExpectedResult = "/text-page-1-custom")]  // Has non-terminating custom URL segment provider.
    [TestCase(SubPage2Key, "en-US", ExpectedResult = "/text-page-2-custom")] // Has terminating custom URL segment provider.
    [TestCase(SubPage3Key, "en-US", ExpectedResult = "/text-page-3")]
    public string? GetLegacyRouteFormat_Returns_Expected_Route(string documentKey, string culture)
    {
        ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);
        return DocumentUrlService.GetLegacyRouteFormat(Guid.Parse(documentKey), culture, false);
    }


    [Test]
    public async Task CreateOrUpdateUrlSegmentsWithDescendantsAsync_Does_Not_Throw_When_Content_Does_Not_Exist()
    {
        // Arrange - use a random key that doesn't exist
        var nonExistentKey = Guid.NewGuid();

        // Act & Assert - should not throw, just return gracefully
        Assert.DoesNotThrowAsync(async () =>
            await DocumentUrlService.CreateOrUpdateUrlSegmentsWithDescendantsAsync(nonExistentKey));
    }

    [Test]
    public async Task CreateOrUpdateUrlSegmentsAsync_Does_Not_Throw_When_Content_Does_Not_Exist()
    {
        // Arrange - use a random key that doesn't exist
        var nonExistentKey = Guid.NewGuid();

        // Act & Assert - should not throw, just return gracefully
        Assert.DoesNotThrowAsync(async () =>
            await DocumentUrlService.CreateOrUpdateUrlSegmentsAsync(nonExistentKey));
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

    #region Invariant vs Variant Content Tests

    [Test]
    public async Task GetUrlSegment_For_Invariant_Content_Works_With_Any_Culture()
    {
        // Arrange - add second language
        var frenchLanguage = new LanguageBuilder().WithCultureInfo("fr-FR").Build();
        await LanguageService.CreateAsync(frenchLanguage, Constants.Security.SuperUserKey);

        // Publish the content
        ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);

        // Act - query with French culture for invariant content (stored with NULL languageId)
        var urlSegment = DocumentUrlService.GetUrlSegment(Subpage.Key, "fr-FR", false);

        // Assert - should find invariant content even though queried with non-default culture
        Assert.That(urlSegment, Is.EqualTo("text-page-1-custom"), "Invariant content URL segment should be accessible with any culture");
    }

    [Test]
    public async Task GetDocumentKeyByRoute_For_Invariant_Content_Works_With_Any_Culture()
    {
        // Arrange - add second language
        var frenchLanguage = new LanguageBuilder().WithCultureInfo("fr-FR").Build();
        await LanguageService.CreateAsync(frenchLanguage, Constants.Security.SuperUserKey);

        // Publish the content
        ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);

        // Act - query with French culture for invariant content (stored with NULL languageId)
        var documentKey = DocumentUrlService.GetDocumentKeyByRoute("/text-page-1-custom", "fr-FR", null, false);

        // Assert - should find invariant content even though queried with non-default culture
        Assert.That(documentKey, Is.EqualTo(Subpage.Key), "Invariant content should be found by route with any culture");
    }

    [Test]
    public async Task Invariant_Content_Stores_Null_LanguageId_In_Database()
    {
        // Arrange - publish content to create URL records
        ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);

        // Act - check stored URL segments in database
        List<PublishedDocumentUrlSegment> storedSegments;
        using (ICoreScope scope = CoreScopeProvider.CreateCoreScope(autoComplete: true))
        {
            storedSegments = DocumentUrlRepository.GetAll()
                .Where(s => s.DocumentKey == Subpage.Key)
                .ToList();
        }

        // Assert - invariant content should have NULL languageId (not a language-specific ID)
        Assert.That(storedSegments, Has.Count.GreaterThan(0), "Should have stored URL segments");

        foreach (var segment in storedSegments)
        {
            Assert.That(segment.LanguageId, Is.Null, "Invariant content should have NULL LanguageId in database");
        }
    }

    #endregion
}
