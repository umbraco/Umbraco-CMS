using Examine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
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
public class BackOfficeExamineSearcherTests : ExamineBaseTest
{
    // TODO: Find a way to remove all the timeouts
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
        // Sometimes we do not dispose all services in time and the test fails because the log file is locked. Resulting in all other tests failing aswell
        Services.DisposeIfDisposable();
        Thread.Sleep(1000);
        TestHelper.DeleteDirectory(_examinePath + "InternalIndex");
        TestHelper.DeleteDirectory(_examinePath + "ExternalIndex");
    }

    private IBackOfficeExamineSearcher BackOfficeExamineSearcher => GetRequiredService<IBackOfficeExamineSearcher>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IIndexRebuilder IndexRebuilder => GetRequiredService<IIndexRebuilder>();

    private ILocalizationService LocalizationService => GetRequiredService<ILocalizationService>();

    private ContentService ContentService => (ContentService)GetRequiredService<IContentService>();

    private IUserStore<BackOfficeIdentityUser> BackOfficeUserStore =>
        GetRequiredService<IUserStore<BackOfficeIdentityUser>>();

    private IBackOfficeSignInManager BackOfficeSignInManager => GetRequiredService<IBackOfficeSignInManager>();

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.AddUnique<IBackOfficeExamineSearcher, BackOfficeExamineSearcher>();
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

    private IEnumerable<ISearchResult> BackOfficeExamineSearch(string query, int pageSize = 20, int pageIndex = 0)
    {
        long totalFound = long.MinValue;
        return BackOfficeExamineSearcher.Search(query, UmbracoEntityTypes.Document,
            pageSize, pageIndex, out totalFound, ignoreUserStartNodes: true);
    }

    private async Task SetupUserIdentity(string userId)
    {
        var identity =
            await BackOfficeUserStore.FindByIdAsync(userId, CancellationToken.None);
        await BackOfficeSignInManager.SignInAsync(identity, false);
    }

    private PublishResult CreateDefaultPublishedContent(string contentName)
    {
        var contentType = new ContentTypeBuilder()
            .WithId(0)
            .Build();
        ContentTypeService.Save(contentType);

        var content = new ContentBuilder()
            .WithId(0)
            .WithName(contentName)
            .WithContentType(contentType)
            .Build();
        var createdContent = ContentService.SaveAndPublish(content);

        // It seems like we need this timeout for the moment, otherwise our indexes would not be built in time and the test could fail.
        Thread.Sleep(1000);

        return createdContent;
    }

    private PublishResult CreateDefaultPublishedContentWithTwoLanguages(string englishNodeName, string danishNodeName)
    {
        string usIso = "en-US";
        string dkIso = "da";

        var langDa = new LanguageBuilder()
            .WithCultureInfo(dkIso)
            .Build();
        LocalizationService.Save(langDa);

        var contentType = new ContentTypeBuilder()
            .WithId(0)
            .WithContentVariation(ContentVariation.Culture)
            .Build();
        ContentTypeService.Save(contentType);

        var content = new ContentBuilder()
            .WithId(0)
            .WithCultureName(usIso, englishNodeName)
            .WithCultureName(dkIso, danishNodeName)
            .WithContentType(contentType)
            .Build();
        var createdContent = ContentService.SaveAndPublish(content);
        Thread.Sleep(1000);

        return createdContent;
    }

    [Test]
    public void Search_Published_Content_With_Empty_Query()
    {
        SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        // Arrange
        var contentName = "TestContent";
        CreateDefaultPublishedContent(contentName);

        string query = string.Empty;

        // Act
        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch(query);

        // Assert
        Assert.AreEqual(0, actual.Count());
    }

    [Test]
    public void Search_Published_Content_With_Query_By_Content_Name()
    {
        // Arrange
        SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        string contentName = "TestContent";
        CreateDefaultPublishedContent(contentName);

        string query = contentName;

        // Act
        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch(query);

        // Assert
        Assert.AreEqual(1, actual.Count());
        Assert.AreEqual(actual.First().Values["nodeName"], contentName);
    }

    [Test]
    public void Search_Published_Content_With_Query_By_Non_Existing_Content_Name()
    {
        // Arrange
        SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        string contentName = "TestContent";
        CreateDefaultPublishedContent(contentName);

        string query = "ContentTest";
        // Act
        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch(query);

        // Assert
        Assert.AreEqual(0, actual.Count());
    }

    [Test]
    public void Search_Published_Content_With_Query_By_Content_Id()
    {
        // Arrange
        SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        string contentName = "RandomContentName";
        PublishResult createdContent = CreateDefaultPublishedContent(contentName);

        string contentId = createdContent.Content.Id.ToString();

        string query = contentId;

        // Act
        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch(query);

        // Assert
        Assert.AreEqual(1, actual.Count());
        Assert.AreEqual(actual.First().Values["nodeName"], contentName);
        Assert.AreEqual(actual.First().Id, contentId);
    }

    [Test]
    public void Search_Two_Published_Content_With_Similar_Names_By_Name()
    {
        // Arrange
        SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        string contentName = "TestName Original";
        string secondContentName = "TestName Copy";

        var contentType = new ContentTypeBuilder()
            .WithId(0)
            .Build();
        ContentTypeService.Save(contentType);

        var firstContent = new ContentBuilder()
            .WithId(0)
            .WithName(contentName)
            .WithContentType(contentType)
            .Build();
        ContentService.SaveAndPublish(firstContent);
        Thread.Sleep(1000);

        var secondContent = new ContentBuilder()
            .WithId(0)
            .WithName(secondContentName)
            .WithContentType(contentType)
            .Build();
        ContentService.SaveAndPublish(secondContent);
        Thread.Sleep(1000);

        // IndexRebuilder.RebuildIndex("InternalIndex");
        string query = contentName;

        // Act
        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch(query);

        // Assert
        Assert.AreEqual(2, actual.Count());
        // Checks if the first content in the search is the original content
        Assert.AreEqual(actual.First().Id, firstContent.Id.ToString());
        // Checks if the score for the original name is higher than the score for the copy
        Assert.Greater(actual.First().Score, actual.Last().Score);

        Thread.Sleep(3000);
    }

    [Test]
    public void Search_For_Child_Published_Content_With_Query_By_Content_Name()
    {
        // Arrange
        SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        string contentName = "ParentTestContent";
        string childContentName = "ChildTestContent";

        var contentType = new ContentTypeBuilder()
            .WithName("Document")
            .Build();
        ContentTypeService.Save(contentType);

        var content = new ContentBuilder()
            .WithName(contentName)
            .WithContentType(contentType)
            .Build();
        ContentService.SaveAndPublish(content);

        Thread.Sleep(1000);

        var childContent = new ContentBuilder()
            .WithName(childContentName)
            .WithContentType(contentType)
            .WithParentId(content.Id)
            .Build();
        ContentService.SaveAndPublish(childContent);

        Thread.Sleep(1000);

        IndexRebuilder.RebuildIndex("InternalIndex");

        string parentQuery = content.Id.ToString();

        string childQuery = childContent.Id.ToString();

        // Act
        IEnumerable<ISearchResult> parentContentActual = BackOfficeExamineSearch(parentQuery);

        IEnumerable<ISearchResult> childContentActual = BackOfficeExamineSearch(childQuery);

        // Assert
        Assert.AreEqual(1, parentContentActual.Count());
        Assert.AreEqual(1, childContentActual.Count());

        Assert.AreEqual(parentContentActual.First().Values["nodeName"], contentName);
        Assert.AreEqual(childContentActual.First().Values["nodeName"], childContentName);

        Thread.Sleep(1000);
    }

    [Test]
    public void Search_For_Child_In_Child_Published_Content_With_Query_By_Content_Name()
    {
        // Arrange
        SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        string contentName = "ParentTestContent";
        string childContentName = "ChildTestContent";
        string childChildContentName = "ChildChildTestContent";

        var contentType = new ContentTypeBuilder()
            .WithName("Document")
            .Build();
        ContentTypeService.Save(contentType);

        var content = new ContentBuilder()
            .WithName(contentName)
            .WithContentType(contentType)
            .Build();
        ContentService.SaveAndPublish(content);
        Thread.Sleep(1000);

        var childContent = new ContentBuilder()
            .WithName(childContentName)
            .WithContentType(contentType)
            .WithParentId(content.Id)
            .Build();
        ContentService.SaveAndPublish(childContent);
        Thread.Sleep(1000);

        var childChildContent = new ContentBuilder()
            .WithName(childChildContentName)
            .WithContentType(contentType)
            .WithParentId(childContent.Id)
            .Build();
        ContentService.SaveAndPublish(childChildContent);
        Thread.Sleep(1000);

        IndexRebuilder.RebuildIndex("InternalIndex");

        string parentQuery = content.Id.ToString();

        string childQuery = childContent.Id.ToString();

        string childChildQuery = childChildContent.Id.ToString();

        // Act
        IEnumerable<ISearchResult> parentContentActual = BackOfficeExamineSearch(parentQuery);

        IEnumerable<ISearchResult> childContentActual = BackOfficeExamineSearch(childQuery);

        IEnumerable<ISearchResult> childChildContentActual = BackOfficeExamineSearch(childChildQuery);

        // Assert
        Assert.AreEqual(1, parentContentActual.Count());
        Assert.AreEqual(1, childContentActual.Count());
        Assert.AreEqual(1, childChildContentActual.Count());

        Assert.AreEqual(parentContentActual.First().Values["nodeName"], contentName);
        Assert.AreEqual(childContentActual.First().Values["nodeName"], childContentName);
        Assert.AreEqual(childChildContentActual.First().Values["nodeName"], childChildContentName);

        // If we do not have this wait the IndexFile will be locked
        Thread.Sleep(5000);
    }

    [Test]
    public void Search_Published_Content_With_Query_With_Content_Name_No_User_Logged_In()
    {
        // Arrange
        string contentName = "TestContent";

        PublishResult createdContent = CreateDefaultPublishedContent(contentName);

        string query = createdContent.Content.Id.ToString();

        // Act
        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch(query);

        // Assert
        Assert.AreEqual(0, actual.Count());

        Thread.Sleep(1000);
    }

    // Multiple Languages
    [Test]
    public void Search_Published_Content_By_Content_Name_With_Two_Languages()
    {
        // Arrange
        SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        string usIso = "en-US";
        string dkIso = "da";
        string englishNodeName = "EnglishNode";
        string danishNodeName = "DanishNode";

        var langDa = new LanguageBuilder()
            .WithCultureInfo(dkIso)
            .Build();
        LocalizationService.Save(langDa);

        PublishResult createdContent;
        var contentType = new ContentTypeBuilder()
            .WithId(0)
            .WithContentVariation(ContentVariation.Culture)
            .Build();
        ContentTypeService.Save(contentType);

        var content = new ContentBuilder()
            .WithId(0)
            .WithCultureName(usIso, englishNodeName)
            .WithCultureName(dkIso, danishNodeName)
            .WithContentType(contentType)
            .Build();
        createdContent = ContentService.SaveAndPublish(content);
        Thread.Sleep(1000);

        string query = createdContent.Content.Id.ToString();

        // Act
        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch(query);

        // Assert
        Assert.AreEqual(1, actual.Count());
        var nodeNameEn = actual.First().Values["nodeName_en-us"];
        var nodeNameDa = actual.First().Values["nodeName_da"];
        Assert.AreEqual(nodeNameEn, englishNodeName);
        Assert.AreEqual(nodeNameDa, danishNodeName);

        // Thread.Sleep(3000);
    }

    [Test]
    public void Search_For_Published_Content_Name_With_Two_Languages_By_Default_Language_Content_Name()
    {
        // Arrange
        SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        string usIso = "en-US";
        string dkIso = "da";
        string englishNodeName = "EnglishNode";
        string danishNodeName = "DanishNode";

        CreateDefaultPublishedContentWithTwoLanguages(englishNodeName, danishNodeName);

        string query = englishNodeName;

        // Act
        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch(query);

        // Assert
        Assert.AreEqual(1, actual.Count());
        var nodeNameEn = actual.First().Values["nodeName_en-us"];
        var nodeNameDa = actual.First().Values["nodeName_da"];
        Assert.AreEqual(nodeNameEn, englishNodeName);
        Assert.AreEqual(nodeNameDa, danishNodeName);

        Thread.Sleep(2000);
    }

    [Test]
    public void Search_For_Published_Content_Name_With_Two_Languages_By_Non_Default_Language_Content_Name()
    {
        // Arrange
        SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        string usIso = "en-US";
        string dkIso = "da";
        string englishNodeName = "EnglishNode";
        string danishNodeName = "DanishNode";

        CreateDefaultPublishedContentWithTwoLanguages(englishNodeName, danishNodeName);

        string query = danishNodeName;

        // Act
        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch(query);

        // Assert
        Assert.AreEqual(1, actual.Count());
        var nodeNameDa = actual.First().Values["nodeName_da"];
        var nodeNameEn = actual.First().Values["nodeName_en-us"];
        Assert.AreEqual(nodeNameEn, englishNodeName);
        Assert.AreEqual(nodeNameDa, danishNodeName);
    }

    [Test]
    public void Search_Published_Content_With_Two_Languages_By_Id()
    {
        // Arrange
        SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        string usIso = "en-US";
        string dkIso = "da";
        string englishNodeName = "EnglishNode";
        string danishNodeName = "DanishNode";

        var contentNode = CreateDefaultPublishedContentWithTwoLanguages(englishNodeName, danishNodeName);

        string query = contentNode.Content.Id.ToString();

        // Act
        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch(query);

        // Assert
        Assert.AreEqual(1, actual.Count());
        var nodeNameDa = actual.First().Values["nodeName_da"];
        var nodeNameEn = actual.First().Values["nodeName_en-us"];
        Assert.AreEqual(nodeNameEn, englishNodeName);
        Assert.AreEqual(nodeNameDa, danishNodeName);
    }

    // Check All Indexed Values
    [Test]
    public void Check_All_Indexed_Values_For_Published_Content_With_No_Properties()
    {
        // Arrange
        SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        string contentName = "TestContent";
        var contentType = new ContentTypeBuilder()
            .WithId(0)
            .Build();
        ContentTypeService.Save(contentType);

        var contentNode = new ContentBuilder()
            .WithId(0)
            .WithName(contentName)
            .WithContentType(contentType)
            .Build();
        ContentService.SaveAndPublish(contentNode);

        // It seems like we need this timeout for the moment, otherwise our indexes would not be built in time and the test could fail.
        Thread.Sleep(1000);

        string query = contentName;

        // Act
        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch(query);

        // Assert
        Assert.AreEqual(1, actual.Count());

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
            Assert.AreEqual(actual.First().Values["__NodeId"], contentNode.Id.ToString());
            Assert.AreEqual(actual.First().Values["__IndexType"], "content");
            Assert.AreEqual(actual.First().Values["__NodeTypeAlias"], contentNode.ContentType.Alias);
            Assert.AreEqual(actual.First().Values["__Published"], contentNodePublish);
            Assert.AreEqual(actual.First().Values["id"], contentNode.Id.ToString());
            Assert.AreEqual(actual.First().Values["__Key"], contentNode.Key.ToString());
            Assert.AreEqual(actual.First().Values["parentID"], contentNode.ParentId.ToString());
            Assert.AreEqual(actual.First().Values["nodeName"], contentNode.Name);
            Assert.AreEqual(actual.First().Values["__VariesByCulture"], contentTypeCultureVariations);
            Assert.AreEqual(actual.First().Values["__Icon"], contentNode.ContentType.Icon);
        });
        Thread.Sleep(1000);
    }

    [Test]
    public void Check_All_Indexed_Values_For_Published_Content_With_Properties()
    {
        // Arrange
        SetupUserIdentity(Constants.Security.SuperUserIdAsString);

        string contentName = "TestContent";
        string propertyEditorName = "TestBox";

        var contentType = new ContentTypeBuilder()
            .WithId(0)
            .AddPropertyType()
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithName(propertyEditorName)
            .WithAlias("testBox")
            .Done()
            .Build();
        ContentTypeService.Save(contentType);

        var contentNode = new ContentBuilder()
            .WithId(0)
            .WithName(contentName)
            .WithContentType(contentType)
            .WithPropertyValues(new { testBox = "TestValue" })
            .Build();
        ContentService.SaveAndPublish(contentNode);

        // It seems like we need this timeout for the moment, otherwise our indexes would not be built in time and the test could fail.
        Thread.Sleep(1000);

        string query = contentName;

        // Act
        IEnumerable<ISearchResult> actual = BackOfficeExamineSearch(query);

        // Assert
        Assert.AreEqual(1, actual.Count());

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
            Assert.AreEqual(actual.First().Values["__NodeId"], contentNode.Id.ToString());
            Assert.AreEqual(actual.First().Values["__IndexType"], "content");
            Assert.AreEqual(actual.First().Values["__NodeTypeAlias"], contentNode.ContentType.Alias);
            Assert.AreEqual(actual.First().Values["__Published"], contentNodePublish);
            Assert.AreEqual(actual.First().Values["id"], contentNode.Id.ToString());
            Assert.AreEqual(actual.First().Values["__Key"], contentNode.Key.ToString());
            Assert.AreEqual(actual.First().Values["parentID"], contentNode.ParentId.ToString());
            Assert.AreEqual(actual.First().Values["nodeName"], contentNode.Name);
            Assert.AreEqual(actual.First().Values["__VariesByCulture"], contentTypeCultureVariations);
            Assert.AreEqual(actual.First().Values["__Icon"], contentNode.ContentType.Icon);
        });
        Thread.Sleep(1000);
    }
}
