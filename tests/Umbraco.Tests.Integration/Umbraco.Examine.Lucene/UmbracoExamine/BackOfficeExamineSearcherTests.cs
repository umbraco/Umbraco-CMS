using Examine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.Examine.DependencyInjection;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Examine.Lucene.UmbracoExamine;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class BackOfficeExamineSearcherTests : ExamineBaseTest
{
    [SetUp]
    public void Setup()
    {
        TestHelper.DeleteDirectory(GetIndexPath(Constants.UmbracoIndexes.InternalIndexName));
        TestHelper.DeleteDirectory(GetIndexPath(Constants.UmbracoIndexes.ExternalIndexName));
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = Services;
        Mock.Get(TestHelper.GetHttpContextAccessor()).Setup(x => x.HttpContext).Returns(httpContext);

        DocumentUrlService.InitAsync(false, CancellationToken.None).GetAwaiter().GetResult();
                }

    [TearDown]
    public void TearDown()
    {
        // When disposing examine, it does a final write, which ends up locking the file if the indexing is not done yet. So we have this wait to circumvent that.
        Thread.Sleep(1500);
        // Sometimes we do not dispose all services in time and the test fails because the log file is locked. Resulting in all other tests failing as well
        (Services as IDisposable)?.Dispose();
    }

    private IDocumentUrlService DocumentUrlService => GetRequiredService<IDocumentUrlService>();
    private IBackOfficeExamineSearcher BackOfficeExamineSearcher => GetRequiredService<IBackOfficeExamineSearcher>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private ContentService ContentService => (ContentService)GetRequiredService<IContentService>();

    private IUserStore<BackOfficeIdentityUser> BackOfficeUserStore =>
        GetRequiredService<IUserStore<BackOfficeIdentityUser>>();

    private IBackOfficeSignInManager BackOfficeSignInManager => GetRequiredService<IBackOfficeSignInManager>();

    private IHttpContextAccessor HttpContextAccessor => GetRequiredService<IHttpContextAccessor>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder.Services.AddUnique<IBackOfficeExamineSearcher, BackOfficeExamineSearcher>();
        builder.AddNotificationHandler<ContentCacheRefresherNotification, ContentIndexingNotificationHandler>();
        builder.AddExamineIndexes();
        builder.Services.AddHostedService<QueuedHostedService>();
    }

    private IEnumerable<ISearchResult> BackOfficeExamineSearch(string query, int pageSize = 20, int pageIndex = 0, bool ignoreUserStartNodes = false) =>
        BackOfficeExamineSearcher.Search(
            query,
            UmbracoEntityTypes.Document,
            pageSize,
            pageIndex,
            totalFound: out _,
            contentTypeAliases: null,
            trashed: null,
            searchFrom: null,
            ignoreUserStartNodes: ignoreUserStartNodes);

    private async Task SetupUserIdentity(string userId)
    {
        var identity = await BackOfficeUserStore.FindByIdAsync(userId, CancellationToken.None);
        await BackOfficeSignInManager.SignInAsync(identity, false);
        var principal = await BackOfficeSignInManager.CreateUserPrincipalAsync(identity);
        HttpContextAccessor.HttpContext.SetPrincipalForRequest(principal);

    }

    private async Task<PublishResult> CreateDefaultPublishedContent(string contentName)
    {
        var contentType = new ContentTypeBuilder()
            .WithId(0)
            .Build();
        await ExecuteAndWaitForIndexing(
            () => ContentTypeService.Save(contentType),
            Constants.UmbracoIndexes.InternalIndexName);

        var content = new ContentBuilder()
            .WithId(0)
            .WithName(contentName)
            .WithContentType(contentType)
            .Build();

        var createdContent = await ExecuteAndWaitForIndexing(
            () =>
        {
            using var scope = ScopeProvider.CreateScope();
            ContentService.Save(content);
            var result = ContentService.Publish(content, Array.Empty<string>());
            scope.Complete();
            return result;
        },
            Constants.UmbracoIndexes.InternalIndexName);

        return createdContent;
    }

    private async Task<PublishResult> CreateDefaultPublishedContentWithTwoLanguages(string englishNodeName, string danishNodeName)
    {
        const string usIso = "en-US";
        const string dkIso = "da";

        var langDa = new LanguageBuilder()
            .WithCultureInfo(dkIso)
            .Build();
        await LanguageService.CreateAsync(langDa, Constants.Security.SuperUserKey);

        var contentType = new ContentTypeBuilder()
            .WithId(0)
            .WithContentVariation(ContentVariation.Culture)
            .Build();
        await ExecuteAndWaitForIndexing(
            () => ContentTypeService.Save(contentType),
            Constants.UmbracoIndexes.InternalIndexName);

        var content = new ContentBuilder()
            .WithId(0)
            .WithCultureName(usIso, englishNodeName)
            .WithCultureName(dkIso, danishNodeName)
            .WithContentType(contentType)
            .Build();
        var createdContent = await ExecuteAndWaitForIndexing(
            () =>
            {
                using var scope = ScopeProvider.CreateScope();
                ContentService.Save(content);
                var result = ContentService.Publish(content, Array.Empty<string>());
                scope.Complete();
                return result;
            },
            Constants.UmbracoIndexes.InternalIndexName);

        return createdContent;
    }

    [Test]
    [Retry(5)] // TODO make this test non-flaky.
    public async Task Search_Published_Content_With_Empty_Query()
    {
        await SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        // Arrange
        const string contentName = "TestContent";
        await CreateDefaultPublishedContent(contentName);

        string query = string.Empty;

        // Act
        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch(query);

        // Assert
        Assert.AreEqual(0, actual.Count());
    }

    [Test]
    [Retry(5)] // TODO make this test non-flaky.
    public async Task Search_Published_Content_With_Query_By_Content_Name()
    {
        // Arrange
        await SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        const string contentName = "TestContent";
        await CreateDefaultPublishedContent(contentName);

        string query = contentName;

        // Act
        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch(query);

        // Assert
        IEnumerable<ISearchResult> searchResults = actual.ToArray();
        Assert.AreEqual(1, searchResults.Count());
        Assert.AreEqual(searchResults.First().Values["nodeName"], contentName);
    }

    [Test]
    [Retry(5)] // TODO make this test non-flaky.
    public async Task Search_Published_Content_With_Query_By_Non_Existing_Content_Name()
    {
        // Arrange
        await SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        const string contentName = "TestContent";
        await CreateDefaultPublishedContent(contentName);

        string query = "ContentTest";
        // Act
        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch(query);

        // Assert
        Assert.AreEqual(0, actual.Count());
    }

    [Test]
    [Retry(5)] // TODO make this test non-flaky.
    public async Task Search_Published_Content_With_Query_By_Content_Id()
    {
        // Arrange
        await SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        const string contentName = "RandomContentName";
        PublishResult createdContent = await CreateDefaultPublishedContent(contentName);

        string contentId = createdContent.Content.Id.ToString();

        string query = contentId;

        // Act
        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch(query);

        // Assert
        IEnumerable<ISearchResult> searchResults = actual.ToArray();
        Assert.AreEqual(1, searchResults.Count());
        Assert.AreEqual(searchResults.First().Values["nodeName"], contentName);
        Assert.AreEqual(searchResults.First().Id, contentId);
    }

    [Test]
    [Retry(5)] // TODO make this test non-flaky.
    public async Task Search_Two_Published_Content_With_Similar_Names_By_Name()
    {
        // Arrange
        await SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        const string contentName = "TestName Original";
        const string secondContentName = "TestName Copy";

        var contentType = new ContentTypeBuilder()
            .WithId(0)
            .Build();
        await ExecuteAndWaitForIndexing(() => ContentTypeService.Save(contentType), Constants.UmbracoIndexes.InternalIndexName);

        var firstContent = new ContentBuilder()
            .WithId(0)
            .WithName(contentName)
            .WithContentType(contentType)
            .Build();
        await ExecuteAndWaitForIndexing(
            () =>
        {
            using var scope = ScopeProvider.CreateCoreScope();
            ContentService.Save(firstContent);
            var result = ContentService.Publish(firstContent, Array.Empty<string>());
            scope.Complete();
            return result;
        },
            Constants.UmbracoIndexes.InternalIndexName);

        var secondContent = new ContentBuilder()
            .WithId(0)
            .WithName(secondContentName)
            .WithContentType(contentType)
            .Build();
        await ExecuteAndWaitForIndexing(
            () =>
        {
            using var scope = ScopeProvider.CreateCoreScope();
            ContentService.Save(secondContent);
            var content = ContentService.Publish(secondContent, Array.Empty<string>());
            scope.Complete();
            return content;
        },
            Constants.UmbracoIndexes.InternalIndexName);

        string query = contentName;

        // Act
        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch(query);

        // Assert
        IEnumerable<ISearchResult> searchResults = actual.ToArray();
        Assert.AreEqual(2, searchResults.Count());
        // Checks if the first content in the search is the original content
        Assert.AreEqual(searchResults.First().Id, firstContent.Id.ToString());
        // Checks if the score for the original name is higher than the score for the copy
        Assert.Greater(searchResults.First().Score, searchResults.Last().Score);
    }

    [Test]
    [Retry(5)] // TODO make this test non-flaky.
    public async Task Search_For_Child_Published_Content_With_Query_By_Content_Name()
    {
        // Arrange
        await SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        const string contentName = "ParentTestContent";
        const string childContentName = "ChildTestContent";

        var contentType = new ContentTypeBuilder()
            .WithName("Document")
            .Build();
        await ExecuteAndWaitForIndexing(() => ContentTypeService.Save(contentType), Constants.UmbracoIndexes.InternalIndexName);

        var content = new ContentBuilder()
            .WithName(contentName)
            .WithContentType(contentType)
            .Build();
        await ExecuteAndWaitForIndexing(
            () =>
            {
                using var scope = ScopeProvider.CreateCoreScope();
                ContentService.Save(content);
                var result = ContentService.Publish(content, Array.Empty<string>());
                scope.Complete();
                return result;
            },
            Constants.UmbracoIndexes.InternalIndexName);

        var childContent = new ContentBuilder()
            .WithName(childContentName)
            .WithContentType(contentType)
            .WithParentId(content.Id)
            .Build();
        await ExecuteAndWaitForIndexing(
            () =>
            {
                using var scope = ScopeProvider.CreateCoreScope();
                ContentService.Save(childContent);
                var result = ContentService.Publish(childContent, Array.Empty<string>());
                scope.Complete();
                return result;
            },
            Constants.UmbracoIndexes.InternalIndexName);

        string parentQuery = content.Id.ToString();

        string childQuery = childContent.Id.ToString();

        // Act
        IEnumerable<ISearchResult> parentContentActual = BackOfficeExamineSearch(parentQuery);
        IEnumerable<ISearchResult> childContentActual = BackOfficeExamineSearch(childQuery);

        // Assert
        IEnumerable<ISearchResult> contentActual = parentContentActual.ToArray();
        IEnumerable<ISearchResult> searchResults = childContentActual.ToArray();
        Assert.AreEqual(1, contentActual.Count());
        Assert.AreEqual(1, searchResults.Count());
        Assert.AreEqual(contentActual.First().Values["nodeName"], contentName);
        Assert.AreEqual(searchResults.First().Values["nodeName"], childContentName);
    }

    [Test]
    [Retry(5)] // TODO make this test non-flaky.
    public async Task Search_For_Child_In_Child_Published_Content_With_Query_By_Content_Name()
    {
        // Arrange
        await SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        const string contentName = "ParentTestContent";
        const string childContentName = "ChildTestContent";
        const string childChildContentName = "ChildChildTestContent";

        var contentType = new ContentTypeBuilder()
            .WithName("Document")
            .Build();
        ContentTypeService.Save(contentType);

        var content = new ContentBuilder()
            .WithName(contentName)
            .WithContentType(contentType)
            .Build();
        await ExecuteAndWaitForIndexing(
            () =>
            {
                using var scope = ScopeProvider.CreateCoreScope();
                ContentService.Save(content);
                var result = ContentService.Publish(content, Array.Empty<string>());
                scope.Complete();
                return result;
            },
            Constants.UmbracoIndexes.InternalIndexName);

        var childContent = new ContentBuilder()
            .WithName(childContentName)
            .WithContentType(contentType)
            .WithParentId(content.Id)
            .Build();
        await ExecuteAndWaitForIndexing(
            () =>
            {
                using var scope = ScopeProvider.CreateCoreScope();
                ContentService.Save(childContent);
                var result = ContentService.Publish(childContent, Array.Empty<string>());
                scope.Complete();
                return result;
            },
            Constants.UmbracoIndexes.InternalIndexName);

        var childChildContent = new ContentBuilder()
            .WithName(childChildContentName)
            .WithContentType(contentType)
            .WithParentId(childContent.Id)
            .Build();
        await ExecuteAndWaitForIndexing(
            () =>
        {
            using var scope = ScopeProvider.CreateCoreScope();
            ContentService.Save(childChildContent);
            var result = ContentService.Publish(childChildContent, Array.Empty<string>());
            scope.Complete();
            return result;
        },
            Constants.UmbracoIndexes.InternalIndexName);

        string parentQuery = content.Id.ToString();
        string childQuery = childContent.Id.ToString();
        string childChildQuery = childChildContent.Id.ToString();

        // Act
        IEnumerable<ISearchResult> parentContentActual = BackOfficeExamineSearch(parentQuery);
        IEnumerable<ISearchResult> childContentActual = BackOfficeExamineSearch(childQuery);
        IEnumerable<ISearchResult> childChildContentActual = BackOfficeExamineSearch(childChildQuery);

        IEnumerable<ISearchResult> parentSearchResults = parentContentActual.ToArray();
        IEnumerable<ISearchResult> childSearchResults = childContentActual.ToArray();
        IEnumerable<ISearchResult> childChildSearchResults = childChildContentActual.ToArray();

        // Assert
        Assert.AreEqual(1, parentSearchResults.Count());
        Assert.AreEqual(1, childSearchResults.Count());
        Assert.AreEqual(1, childChildSearchResults.Count());
        Assert.AreEqual(parentSearchResults.First().Values["nodeName"], contentName);
        Assert.AreEqual(childSearchResults.First().Values["nodeName"], childContentName);
        Assert.AreEqual(childChildSearchResults.First().Values["nodeName"], childChildContentName);
    }

    [Test]
    [Retry(5)] // TODO make this test non-flaky.
    public async Task Search_Published_Content_With_Query_With_Content_Name_No_User_Logged_In()
    {
        // Arrange
        const string contentName = "TestContent";

        PublishResult createdContent = await CreateDefaultPublishedContent(contentName);

        string query = createdContent.Content.Id.ToString();

        // Act
        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch(query);

        // Assert
        Assert.AreEqual(0, actual.Count());
    }

    // Multiple Languages
    [Test]
    [Retry(5)] // TODO make this test non-flaky.
    public async Task Search_Published_Content_By_Content_Name_With_Two_Languages()
    {
        // Arrange
        await SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        const string usIso = "en-US";
        const string dkIso = "da";
        const string englishNodeName = "EnglishNode";
        const string danishNodeName = "DanishNode";

        var langDa = new LanguageBuilder()
            .WithCultureInfo(dkIso)
            .Build();
        await LanguageService.CreateAsync(langDa, Constants.Security.SuperUserKey);

        var contentType = new ContentTypeBuilder()
            .WithId(0)
            .WithContentVariation(ContentVariation.Culture)
            .Build();
        await ExecuteAndWaitForIndexing(() => ContentTypeService.Save(contentType), Constants.UmbracoIndexes.InternalIndexName);

        var content = new ContentBuilder()
            .WithId(0)
            .WithCultureName(usIso, englishNodeName)
            .WithCultureName(dkIso, danishNodeName)
            .WithContentType(contentType)
            .Build();
        PublishResult createdContent = await ExecuteAndWaitForIndexing(
            () =>
            {
                using var scope = ScopeProvider.CreateCoreScope();
                ContentService.Save(content);
                var result = ContentService.Publish(content, Array.Empty<string>());
                scope.Complete();
                return result;
            },
            Constants.UmbracoIndexes.InternalIndexName);

        string query = createdContent.Content.Id.ToString();

        // Act
        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch(query);

        // Assert
        IEnumerable<ISearchResult> searchResults = actual.ToArray();
        Assert.AreEqual(1, searchResults.Count());
        var nodeNameEn = searchResults.First().Values["nodeName_en-us"];
        var nodeNameDa = searchResults.First().Values["nodeName_da"];
        Assert.AreEqual(englishNodeName, nodeNameEn);
        Assert.AreEqual(danishNodeName, nodeNameDa);
    }

    [Test]
    [Retry(5)] // TODO make this test non-flaky.
    public async Task Search_For_Published_Content_Name_With_Two_Languages_By_Default_Language_Content_Name()
    {
        // Arrange
        await SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        const string englishNodeName = "EnglishNode";
        const string danishNodeName = "DanishNode";

        await CreateDefaultPublishedContentWithTwoLanguages(englishNodeName, danishNodeName);

        string query = englishNodeName;

        // Act
        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch(query);

        // Assert
        IEnumerable<ISearchResult> searchResults = actual.ToArray();
        Assert.AreEqual(1, searchResults.Count());
        var nodeNameEn = searchResults.First().Values["nodeName_en-us"];
        var nodeNameDa = searchResults.First().Values["nodeName_da"];
        Assert.AreEqual(englishNodeName, nodeNameEn);
        Assert.AreEqual(danishNodeName, nodeNameDa);
    }

    [Test]
    [Retry(5)] // TODO make this test non-flaky.
    public async Task Search_For_Published_Content_Name_With_Two_Languages_By_Non_Default_Language_Content_Name()
    {
        // Arrange
        await SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        const string englishNodeName = "EnglishNode";
        const string danishNodeName = "DanishNode";

        await CreateDefaultPublishedContentWithTwoLanguages(englishNodeName, danishNodeName);

        string query = danishNodeName;

        // Act
        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch(query);

        // Assert
        IEnumerable<ISearchResult> searchResults = actual.ToArray();
        Assert.AreEqual(1, searchResults.Count());
        var nodeNameDa = searchResults.First().Values["nodeName_da"];
        var nodeNameEn = searchResults.First().Values["nodeName_en-us"];
        Assert.AreEqual(englishNodeName, nodeNameEn);
        Assert.AreEqual(danishNodeName, nodeNameDa);
    }

    [Test]
    [Retry(5)] // TODO make this test non-flaky.
    public async Task Search_Published_Content_With_Two_Languages_By_Id()
    {
        // Arrange
        await SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        const string englishNodeName = "EnglishNode";
        const string danishNodeName = "DanishNode";

        var contentNode = await CreateDefaultPublishedContentWithTwoLanguages(englishNodeName, danishNodeName);

        string query = contentNode.Content.Id.ToString();

        // Act
        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch(query);

        // Assert
        IEnumerable<ISearchResult> searchResults = actual.ToArray();
        Assert.AreEqual(1, searchResults.Count());
        var nodeNameDa = searchResults.First().Values["nodeName_da"];
        var nodeNameEn = searchResults.First().Values["nodeName_en-us"];
        Assert.AreEqual(englishNodeName, nodeNameEn);
        Assert.AreEqual(danishNodeName, nodeNameDa);
    }

    // Check All Indexed Values
    [Test]
    [Retry(5)] // TODO make this test non-flaky.
    public async Task Check_All_Indexed_Values_For_Published_Content_With_No_Properties()
    {
        // Arrange
        await SetupUserIdentity(Constants.Security.SuperUserKey.ToString());

        const string contentName = "TestContent";

        var contentType = new ContentTypeBuilder()
            .WithId(0)
            .Build();
        ContentTypeService.Save(contentType);

        var contentNode = new ContentBuilder()
            .WithId(0)
            .WithName(contentName)
            .WithContentType(contentType)
            .Build();
        await ExecuteAndWaitForIndexing(
            () =>
            {
                using var scope = ScopeProvider.CreateCoreScope();
                ContentService.Save(contentNode);
                var result = ContentService.Publish(contentNode, Array.Empty<string>());
                scope.Complete();
                return result;
            },
            Constants.UmbracoIndexes.InternalIndexName);

        string query = contentName;

        // Act
        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch(query);

        // Assert
        IEnumerable<ISearchResult> searchResults = actual.ToArray();
        Assert.AreEqual(1, searchResults.Count());

        string contentNodePublish = string.Empty;
        if (contentNode.Published)
        {
            contentNodePublish = "y";
        }

        string contentTypeCultureVariations = string.Empty;

        if (contentType.Variations == ContentVariation.Nothing)
        {
            contentTypeCultureVariations = "n";
        }

        Assert.Multiple(() =>
        {
            Assert.AreEqual(searchResults.First().Values["__NodeId"], contentNode.Id.ToString());
            Assert.AreEqual(searchResults.First().Values["__IndexType"], "content");
            Assert.AreEqual(searchResults.First().Values["__NodeTypeAlias"], contentNode.ContentType.Alias);
            Assert.AreEqual(searchResults.First().Values["__Published"], contentNodePublish);
            Assert.AreEqual(searchResults.First().Values["id"], contentNode.Id.ToString());
            Assert.AreEqual(searchResults.First().Values["__Key"], contentNode.Key.ToString());
            Assert.AreEqual(searchResults.First().Values["parentID"], contentNode.ParentId.ToString());
            Assert.AreEqual(searchResults.First().Values["nodeName"], contentNode.Name);
            Assert.AreEqual(searchResults.First().Values["__VariesByCulture"], contentTypeCultureVariations);
            Assert.AreEqual(searchResults.First().Values["__Icon"], contentNode.ContentType.Icon);
        });
    }

    [Test]
    [Retry(5)] // TODO make this test non-flaky.
    public async Task Check_All_Indexed_Values_For_Published_Content_With_Properties()
    {
        // Arrange
        await SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        const string contentName = "TestContent";
        const string propertyEditorName = "TestBox";

        var contentType = new ContentTypeBuilder()
            .WithId(0)
            .AddPropertyType()
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithName(propertyEditorName)
            .WithAlias("testBox")
            .Done()
            .Build();
        await ExecuteAndWaitForIndexing(() => ContentTypeService.Save(contentType), Constants.UmbracoIndexes.InternalIndexName);

        var contentNode = new ContentBuilder()
            .WithId(0)
            .WithName(contentName)
            .WithContentType(contentType)
            .WithPropertyValues(new { testBox = "TestValue" })
            .Build();
        await ExecuteAndWaitForIndexing(
            () =>
            {
                using var scope = ScopeProvider.CreateCoreScope();
                ContentService.Save(contentNode);
                var result = ContentService.Publish(contentNode, Array.Empty<string>());
                scope.Complete();
                return result;
            },
            Constants.UmbracoIndexes.InternalIndexName);

        string query = contentName;

        // Act
        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch(query);

        // Assert
        IEnumerable<ISearchResult> searchResults = actual.ToArray();
        Assert.AreEqual(1, searchResults.Count());

        string contentNodePublish = string.Empty;
        string contentTypeCultureVariations = string.Empty;

        if (contentNode.Published)
        {
            contentNodePublish = "y";
        }

        if (contentType.Variations == ContentVariation.Nothing)
        {
            contentTypeCultureVariations = "n";
        }

        Assert.Multiple(() =>
        {
            Assert.AreEqual(searchResults.First().Values["__NodeId"], contentNode.Id.ToString());
            Assert.AreEqual(searchResults.First().Values["__IndexType"], "content");
            Assert.AreEqual(searchResults.First().Values["__NodeTypeAlias"], contentNode.ContentType.Alias);
            Assert.AreEqual(searchResults.First().Values["__Published"], contentNodePublish);
            Assert.AreEqual(searchResults.First().Values["id"], contentNode.Id.ToString());
            Assert.AreEqual(searchResults.First().Values["__Key"], contentNode.Key.ToString());
            Assert.AreEqual(searchResults.First().Values["parentID"], contentNode.ParentId.ToString());
            Assert.AreEqual(searchResults.First().Values["nodeName"], contentNode.Name);
            Assert.AreEqual(searchResults.First().Values["__VariesByCulture"], contentTypeCultureVariations);
            Assert.AreEqual(searchResults.First().Values["__Icon"], contentNode.ContentType.Icon);
        });
    }

    // Multi-word relevance ranking — see #22862.
    // Long, precise multi-word queries should surface their exact node-name match at position 1,
    // not be outranked by short noisy docs sharing only a common token.

    [Test]
    public async Task Multi_Word_Search_Ranks_Exact_Phrase_Match_First()
    {
        await SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        const string exactMatch = "Sirop de Grenade Pomegranate Syrup";
        await PublishContents(
            exactMatch,
            "Quinta de la Rosa Reserva 2020",
            "Delivery API Test Page",
            "Test De Mocha");

        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch(exactMatch);

        ISearchResult[] searchResults = actual.ToArray();
        Assert.GreaterOrEqual(searchResults.Length, 1);
        Assert.AreEqual(exactMatch, searchResults.First().Values["nodeName"]);
    }

    [Test]
    public async Task Multi_Word_Search_Ranks_All_Tokens_Present_Above_Partial_Match()
    {
        await SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        // Same five tokens as the query, different order — no exact phrase match.
        const string allTokensScrambled = "Pomegranate Sirop de Syrup Grenade";

        // Partial match — only the common token "de" overlaps with the query.
        const string partialMatch = "de la Rosa";

        await PublishContents(
            allTokensScrambled,
            partialMatch,
            "Pomegranate Marmalade");

        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch("Sirop de Grenade Pomegranate Syrup");

        ISearchResult[] searchResults = actual.ToArray();
        var scrambledIndex = Array.FindIndex(searchResults, r => (string)r.Values["nodeName"] == allTokensScrambled);
        var partialIndex = Array.FindIndex(searchResults, r => (string)r.Values["nodeName"] == partialMatch);

        Assert.GreaterOrEqual(scrambledIndex, 0, $"'{allTokensScrambled}' (all tokens present) must appear in results");
        Assert.IsTrue(
            partialIndex < 0 || scrambledIndex < partialIndex,
            $"'{allTokensScrambled}' (all tokens present) must rank above '{partialMatch}' (single-token overlap)");
    }

    [Test]
    public async Task Multi_Word_Search_Quoted_Phrase_Returns_Exact_Match_First()
    {
        await SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        const string exactMatch = "Sirop de Grenade Pomegranate Syrup";
        await PublishContents(
            exactMatch,
            "de la Rosa");

        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch($"\"{exactMatch}\"");

        ISearchResult[] searchResults = actual.ToArray();
        Assert.GreaterOrEqual(searchResults.Length, 1);
        Assert.AreEqual(exactMatch, searchResults.First().Values["nodeName"]);
    }

    /// <summary>
    /// Verifies that <see cref="IIndexedEntitySearchService"/> returns matches in Lucene score
    /// order. The exact-match document is published last so its nodeId is the highest of the
    /// set, meaning a consumer that surfaces results in database order would put it last —
    /// only a score-order-preserving consumer ranks it first.
    /// </summary>
    [Test]
    public async Task IndexedEntitySearchService_Preserves_Score_Order_For_Multi_Word_Search()
    {
        await SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        const string exactMatch = "Sirop de Grenade Pomegranate Syrup";
        await PublishContents(
            "Quinta de la Rosa Reserva 2020",
            "Delivery API Test Page",
            "Test De Mocha",
            exactMatch);

        var indexedEntitySearchService = GetRequiredService<IIndexedEntitySearchService>();
        PagedModel<IEntitySlim> result = await indexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Document,
            exactMatch,
            parentId: null,
            contentTypeIds: null,
            trashed: null);

        IEntitySlim[] items = result.Items.ToArray();
        Assert.GreaterOrEqual(items.Length, 1);
        Assert.AreEqual(exactMatch, items.First().Name);
    }

    [Test]
    public async Task Single_Token_Search_Returns_All_Prefix_Matches()
    {
        await SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        await PublishContents(
            "Delivery API Test Page",
            "Quinta de la Rosa",
            "Test De Mocha",
            "Pomegranate Marmalade"); // no "de" prefix anywhere

        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch("de");

        var nodeNames = actual.Select(r => (string)r.Values["nodeName"]).ToArray();
        Assert.Contains("Delivery API Test Page", nodeNames);
        Assert.Contains("Quinta de la Rosa", nodeNames);
        Assert.Contains("Test De Mocha", nodeNames);
    }

    private async Task PublishContents(params string[] contentNames)
    {
        var contentType = new ContentTypeBuilder()
            .WithId(0)
            .Build();
        await ExecuteAndWaitForIndexing(
            () => ContentTypeService.Save(contentType),
            Constants.UmbracoIndexes.InternalIndexName);

        foreach (var name in contentNames)
        {
            var content = new ContentBuilder()
                .WithId(0)
                .WithName(name)
                .WithContentType(contentType)
                .Build();
            await ExecuteAndWaitForIndexing(
                () =>
                {
                    using var scope = ScopeProvider.CreateCoreScope();
                    ContentService.Save(content);
                    var result = ContentService.Publish(content, Array.Empty<string>());
                    scope.Complete();
                    return result;
                },
                Constants.UmbracoIndexes.InternalIndexName);
        }
    }
}
