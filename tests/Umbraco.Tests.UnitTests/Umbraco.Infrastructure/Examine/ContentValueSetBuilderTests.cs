// Copyright (c) Umbraco.
// See LICENSE for more details.
using Examine;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Examine;

[TestFixture]
public class ContentValueSetBuilderTests
{
    private const string DefaultCulture = "en-US";
    private Mock<IUserService> _userService = null!;
    private Mock<ICoreScopeProvider> _scopeProvider = null!;
#pragma warning disable CS0618 // Type or member is obsolete
    private Mock<ILocalizationService> _localizationService = null!;
#pragma warning restore CS0618 // Type or member is obsolete
    private Mock<IContentTypeService> _contentTypeService = null!;
    private Mock<IDocumentUrlService> _documentUrlService = null!;
    private Mock<ILanguageService> _languageService = null!;
    private Mock<IShortStringHelper> _shortStringHelper = null!;
    private UrlSegmentProviderCollection _urlSegmentProviders = null!;
    private PropertyEditorCollection _propertyEditors = null!;

    [SetUp]
    public void SetUp()
    {
        _userService = new Mock<IUserService>();
        SetupUserProfiles((10, "Alice"), (20, "Bob"));
        _scopeProvider = new Mock<ICoreScopeProvider>();
        _scopeProvider
            .Setup(x => x.CreateCoreScope(
                It.IsAny<System.Data.IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher?>(),
                It.IsAny<IScopedNotificationPublisher?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(Mock.Of<ICoreScope>());
#pragma warning disable CS0618 // Type or member is obsolete
        _localizationService = new Mock<ILocalizationService>();
_localizationService.Setup(x => x.GetDefaultLanguageIsoCode()).Returns(DefaultCulture);
#pragma warning restore CS0618 // Type or member is obsolete
        _contentTypeService = new Mock<IContentTypeService>();
        _contentTypeService.Setup(x => x.GetAll()).Returns(Array.Empty<IContentType>());
        _documentUrlService = new Mock<IDocumentUrlService>();
        _documentUrlService.Setup(x => x.IsInitialized).Returns(true);
        _languageService = new Mock<ILanguageService>();
        _languageService.Setup(x => x.GetDefaultIsoCodeAsync()).ReturnsAsync(DefaultCulture);
        _shortStringHelper = new Mock<IShortStringHelper>();
        _urlSegmentProviders = new UrlSegmentProviderCollection(() => Array.Empty<IUrlSegmentProvider>());
        _propertyEditors = new PropertyEditorCollection(new DataEditorCollection(() => Array.Empty<IDataEditor>()));
    }

    private void SetupUserProfiles(params (int Id, string Name)[] users)
    {
        var userMocks = users.Select(u =>
        {
            var mock = new Mock<IUser>();
            mock.Setup(x => x.Id).Returns(u.Id);
            mock.Setup(x => x.Name).Returns(u.Name);
            return mock.Object;
        }).ToArray();
        _userService
            .Setup(x => x.GetUsersById(It.IsAny<int[]>()))
            .Returns<int[]>(ids => userMocks.Where(u => ids.Contains(u.Id)).ToArray());
    }

    private ContentValueSetBuilder CreateBuilder(bool publishedValuesOnly = false) =>
        new(
            _propertyEditors,
            _urlSegmentProviders,
            _userService.Object,
            _shortStringHelper.Object,
            _scopeProvider.Object,
            publishedValuesOnly,
            _localizationService.Object,
            _contentTypeService.Object,
            NullLogger<ContentValueSetBuilder>.Instance,
            _documentUrlService.Object,
            _languageService.Object);

    private static Mock<IContent> CreateContentMock(
        int id = 1,
        Guid? key = null,
        string name = "Test Content",
        string? publishName = "Test Content",
        bool published = true,
        int parentId = -1,
        int level = 1,
        int creatorId = 10,
        int writerId = 20,
        int sortOrder = 0,
        string path = "-1,1",
        int? templateId = null,
        string contentTypeAlias = "testType",
        int contentTypeId = 100,
        Guid? contentTypeKey = null,
        string? icon = "icon-document",
        ContentVariation variation = ContentVariation.Nothing)
    {
        var contentType = Mock.Of<ISimpleContentType>(ct =>
            ct.Id == contentTypeId &&
            ct.Key == (contentTypeKey ?? Guid.NewGuid()) &&
            ct.Alias == contentTypeAlias &&
            ct.Icon == icon &&
            ct.Variations == variation);
        var content = new Mock<IContent>();
        content.Setup(c => c.Id).Returns(id);
        content.Setup(c => c.Key).Returns(key ?? Guid.NewGuid());
        content.Setup(c => c.Name).Returns(name);
        content.Setup(c => c.PublishName).Returns(publishName);
        content.Setup(c => c.Published).Returns(published);
        content.Setup(c => c.ParentId).Returns(parentId);
        content.Setup(c => c.Level).Returns(level);
        content.Setup(c => c.CreatorId).Returns(creatorId);
        content.Setup(c => c.WriterId).Returns(writerId);
        content.Setup(c => c.SortOrder).Returns(sortOrder);
        content.Setup(c => c.CreateDate).Returns(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        content.Setup(c => c.UpdateDate).Returns(new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc));
        content.Setup(c => c.Path).Returns(path);
        content.Setup(c => c.TemplateId).Returns(templateId);
        content.Setup(c => c.ContentType).Returns(contentType);
        content.Setup(c => c.Properties).Returns(new global::Umbraco.Cms.Core.Models.PropertyCollection());
        content.Setup(c => c.AvailableCultures).Returns(Array.Empty<string>());
        return content;
    }

    private static object? GetSingleValue(ValueSet valueSet, string fieldName)
    {
        Assert.That(valueSet.Values.ContainsKey(fieldName), Is.True, $"Field '{fieldName}' not found in ValueSet");
        return valueSet.Values[fieldName].FirstOrDefault();
    }

    [Test]
    public void GetValueSets_Returns_ValueSet_For_Single_Content()
    {
        var builder = CreateBuilder();
        var content = CreateContentMock();
        var results = builder.GetValueSets(content.Object).ToList();
        Assert.That(results, Has.Count.EqualTo(1));
    }

    [Test]
    public void GetValueSets_Returns_ValueSet_Per_Content_Item()
    {
        var builder = CreateBuilder();
        var content1 = CreateContentMock(id: 1, path: "-1,1");
        var content2 = CreateContentMock(id: 2, path: "-1,2");
        var content3 = CreateContentMock(id: 3, path: "-1,3");
        var results = builder.GetValueSets(content1.Object, content2.Object, content3.Object).ToList();
        Assert.That(results, Has.Count.EqualTo(3));
    }

    [Test]
    public void GetValueSets_Sets_Correct_ValueSet_Id_And_Category()
    {
        var builder = CreateBuilder();
        var content = CreateContentMock(id: 42, contentTypeAlias: "blogPost");
        var result = builder.GetValueSets(content.Object).Single();
        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo("42"));
            Assert.That(result.Category, Is.EqualTo(IndexTypes.Content));
            Assert.That(result.ItemType, Is.EqualTo("blogPost"));
        });
    }

    [Test]
    public void GetValueSets_Populates_Basic_Fields()
    {
        var contentKey = Guid.NewGuid();
        var builder = CreateBuilder();
        var content = CreateContentMock(
            id: 10,
            key: contentKey,
            parentId: 5,
            level: 3,
            creatorId: 10,
            writerId: 20,
            sortOrder: 7,
            path: "-1,5,10",
            templateId: 99,
            icon: "icon-blog");
        var result = builder.GetValueSets(content.Object).Single();
        Assert.Multiple(() =>
        {
            Assert.That(GetSingleValue(result, "id"), Is.EqualTo(10));
            Assert.That(GetSingleValue(result, UmbracoExamineFieldNames.NodeKeyFieldName), Is.EqualTo(contentKey));
            Assert.That(GetSingleValue(result, "parentID"), Is.EqualTo(5));
            Assert.That(GetSingleValue(result, "level"), Is.EqualTo(3));
            Assert.That(GetSingleValue(result, "creatorID"), Is.EqualTo(10));
            Assert.That(GetSingleValue(result, "writerID"), Is.EqualTo(20));
            Assert.That(GetSingleValue(result, "sortOrder"), Is.EqualTo(7));
            Assert.That(GetSingleValue(result, "path"), Is.EqualTo("-1,5,10"));
            Assert.That(GetSingleValue(result, "templateID"), Is.EqualTo(99));
            Assert.That(GetSingleValue(result, "icon"), Is.EqualTo("icon-blog"));
        });
    }

    [Test]
    public void GetValueSets_Sets_ParentId_To_Minus1_For_Root_Content()
    {
        var builder = CreateBuilder();
        var content = CreateContentMock(level: 1, parentId: -1);
        var result = builder.GetValueSets(content.Object).Single();
        Assert.That(GetSingleValue(result, "parentID"), Is.EqualTo(-1));
    }

    [Test]
    public void GetValueSets_Sets_ParentId_From_Content_When_Level_Greater_Than_1()
    {
        var builder = CreateBuilder();
        var content = CreateContentMock(level: 2, parentId: 42);
        var result = builder.GetValueSets(content.Object).Single();
        Assert.That(GetSingleValue(result, "parentID"), Is.EqualTo(42));
    }
    [Test]
    public void GetValueSets_Sets_TemplateId_To_Zero_When_Null()
    {
        var builder = CreateBuilder();
        var content = CreateContentMock(templateId: null);
        var result = builder.GetValueSets(content.Object).Single();
        Assert.That(GetSingleValue(result, "templateID"), Is.EqualTo(0));
    }

    [Test]
    public void GetValueSets_Sets_Published_Yes_When_Published()
    {
        var builder = CreateBuilder();
        var content = CreateContentMock(published: true);
        var result = builder.GetValueSets(content.Object).Single();
        Assert.That(GetSingleValue(result, UmbracoExamineFieldNames.PublishedFieldName), Is.EqualTo("y"));
    }

    [Test]
    public void GetValueSets_Sets_Published_No_When_Unpublished()
    {
        var builder = CreateBuilder();
        var content = CreateContentMock(published: false);
        var result = builder.GetValueSets(content.Object).Single();
        Assert.That(GetSingleValue(result, UmbracoExamineFieldNames.PublishedFieldName), Is.EqualTo("n"));
    }

    [Test]
    public void GetValueSets_Uses_Name_When_Not_PublishedValuesOnly()
    {
        var builder = CreateBuilder(publishedValuesOnly: false);
        var content = CreateContentMock(name: "Draft Name", publishName: "Published Name");
        var result = builder.GetValueSets(content.Object).Single();
        Assert.That(GetSingleValue(result, UmbracoExamineFieldNames.NodeNameFieldName), Is.EqualTo("Draft Name"));
    }

    [Test]
    public void GetValueSets_Uses_PublishName_When_PublishedValuesOnly()
    {
        var builder = CreateBuilder(publishedValuesOnly: true);
        var content = CreateContentMock(name: "Draft Name", publishName: "Published Name");
        var result = builder.GetValueSets(content.Object).Single();
        Assert.That(GetSingleValue(result, UmbracoExamineFieldNames.NodeNameFieldName), Is.EqualTo("Published Name"));
    }

    [Test]
    public void GetValueSets_Resolves_Creator_And_Writer_Names()
    {
        SetupUserProfiles((10, "Alice"), (20, "Bob"));
        var builder = CreateBuilder();
        var content = CreateContentMock(creatorId: 10, writerId: 20);
        var result = builder.GetValueSets(content.Object).Single();
        Assert.Multiple(() =>
        {
            Assert.That(GetSingleValue(result, "creatorName"), Is.EqualTo("Alice"));
            Assert.That(GetSingleValue(result, "writerName"), Is.EqualTo("Bob"));
        });
    }

    [Test]
    public void GetValueSets_Uses_QuestionMarks_For_Unknown_User_Profiles()
    {
        _userService
            .Setup(x => x.GetUsersById(It.IsAny<int[]>()))
            .Returns(Array.Empty<IUser>());
        var builder = CreateBuilder();
        var content = CreateContentMock(creatorId: 999, writerId: 888);
        var result = builder.GetValueSets(content.Object).Single();
        Assert.Multiple(() =>
        {
            Assert.That(GetSingleValue(result, "creatorName"), Is.EqualTo("??"));
            Assert.That(GetSingleValue(result, "writerName"), Is.EqualTo("??"));
        });
    }

    [Test]
    public void GetValueSets_Sets_VariesByCulture_No_For_Invariant_Content()
    {
        var builder = CreateBuilder();
        var content = CreateContentMock(variation: ContentVariation.Nothing);
        var result = builder.GetValueSets(content.Object).Single();
        Assert.That(GetSingleValue(result, UmbracoExamineFieldNames.VariesByCultureFieldName), Is.EqualTo("n"));
    }

    [Test]
    public void GetValueSets_Sets_VariesByCulture_Yes_For_Variant_Content()
    {
        var builder = CreateBuilder();
        var content = CreateContentMock(variation: ContentVariation.Culture);
        content.Setup(c => c.AvailableCultures).Returns(new[] { "en-US" });
        content.Setup(c => c.IsCulturePublished("en-US")).Returns(true);
        content.Setup(c => c.GetCultureName("en-US")).Returns("English Name");
        var result = builder.GetValueSets(content.Object).Single();
        Assert.That(GetSingleValue(result, UmbracoExamineFieldNames.VariesByCultureFieldName), Is.EqualTo("y"));
    }

    [Test]
    public void GetValueSets_Adds_Culture_Specific_Fields_For_Variant_Content()
    {
        _documentUrlService.Setup(x => x.GetUrlSegment(It.IsAny<Guid>(), "en-US", false)).Returns("english-page");
        _documentUrlService.Setup(x => x.GetUrlSegment(It.IsAny<Guid>(), "fr-FR", false)).Returns("page-francaise");
        var builder = CreateBuilder(publishedValuesOnly: false);
        var content = CreateContentMock(variation: ContentVariation.Culture);
        content.Setup(c => c.AvailableCultures).Returns(new[] { "en-US", "fr-FR" });
        content.Setup(c => c.IsCulturePublished("en-US")).Returns(true);
        content.Setup(c => c.IsCulturePublished("fr-FR")).Returns(false);
        content.Setup(c => c.GetCultureName("en-US")).Returns("English Name");
        content.Setup(c => c.GetCultureName("fr-FR")).Returns("Nom Français");
        content.Setup(c => c.GetUpdateDate("en-US")).Returns(new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc));
        content.Setup(c => c.GetUpdateDate("fr-FR")).Returns(new DateTime(2024, 4, 1, 0, 0, 0, DateTimeKind.Utc));
        var result = builder.GetValueSets(content.Object).Single();
        Assert.Multiple(() =>
        {
            // URL segments per culture
            Assert.That(GetSingleValue(result, "urlName_en-us"), Is.EqualTo("english-page"));
            Assert.That(GetSingleValue(result, "urlName_fr-fr"), Is.EqualTo("page-francaise"));
            // Node names per culture
            Assert.That(GetSingleValue(result, "nodeName_en-us"), Is.EqualTo("English Name"));
            Assert.That(GetSingleValue(result, "nodeName_fr-fr"), Is.EqualTo("Nom Français"));
            // Published state per culture
            Assert.That(GetSingleValue(result, UmbracoExamineFieldNames.PublishedFieldName + "_en-us"), Is.EqualTo("y"));
            Assert.That(GetSingleValue(result, UmbracoExamineFieldNames.PublishedFieldName + "_fr-fr"), Is.EqualTo("n"));
            // Update dates per culture
            Assert.That(GetSingleValue(result, "updateDate_en-us"), Is.EqualTo(new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc)));
            Assert.That(GetSingleValue(result, "updateDate_fr-fr"), Is.EqualTo(new DateTime(2024, 4, 1, 0, 0, 0, DateTimeKind.Utc)));
        });
    }

    [Test]
    public void GetValueSets_Uses_PublishName_For_Variant_When_PublishedValuesOnly()
    {
        var builder = CreateBuilder(publishedValuesOnly: true);
        var content = CreateContentMock(variation: ContentVariation.Culture);
        content.Setup(c => c.AvailableCultures).Returns(new[] { "en-US" });
        content.Setup(c => c.IsCulturePublished("en-US")).Returns(true);
        content.Setup(c => c.GetCultureName("en-US")).Returns("Draft Name");
        content.Setup(c => c.GetPublishName("en-US")).Returns("Published Name");
        content.Setup(c => c.GetPublishDate("en-US")).Returns(new DateTime(2024, 5, 1, 0, 0, 0, DateTimeKind.Utc));
        var result = builder.GetValueSets(content.Object).Single();
        Assert.Multiple(() =>
        {
            Assert.That(GetSingleValue(result, "nodeName_en-us"), Is.EqualTo("Published Name"));
            Assert.That(GetSingleValue(result, "updateDate_en-us"), Is.EqualTo(new DateTime(2024, 5, 1, 0, 0, 0, DateTimeKind.Utc)));
        });
    }

    [Test]
    public void GetValueSets_Uses_DocumentUrlService_When_Initialized()
    {
        var contentKey = Guid.NewGuid();
        _documentUrlService.Setup(x => x.IsInitialized).Returns(true);
        _documentUrlService.Setup(x => x.GetUrlSegment(contentKey, DefaultCulture, false)).Returns("cached-url-segment");
        var builder = CreateBuilder();
        var content = CreateContentMock(key: contentKey);
        var result = builder.GetValueSets(content.Object).Single();
        Assert.That(GetSingleValue(result, "urlName"), Is.EqualTo("cached-url-segment"));
        _documentUrlService.Verify(x => x.GetUrlSegment(contentKey, DefaultCulture, false), Times.Once);
    }
    [Test]
    public void GetValueSets_Falls_Back_To_Content_Extension_When_DocumentUrlService_Not_Initialized()
    {
        _documentUrlService.Setup(x => x.IsInitialized).Returns(false);
        var urlSegmentProvider = new Mock<IUrlSegmentProvider>();
        urlSegmentProvider
            .Setup(x => x.GetUrlSegment(It.IsAny<IContentBase>(), It.IsAny<bool>(), It.IsAny<string?>()))
            .Returns("computed-url-segment");
        _urlSegmentProviders = new UrlSegmentProviderCollection(() => new[] { urlSegmentProvider.Object });
        var builder = CreateBuilder();
        var content = CreateContentMock(name: "Test Page");
        var result = builder.GetValueSets(content.Object).Single();
        Assert.That(GetSingleValue(result, "urlName"), Is.EqualTo("computed-url-segment"));
        _documentUrlService.Verify(x => x.GetUrlSegment(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public void GetValueSets_Does_Not_Throw_When_DocumentUrlService_Not_Initialized()
    {
        _documentUrlService.Setup(x => x.IsInitialized).Returns(false);
        _documentUrlService
            .Setup(x => x.GetUrlSegment(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Throws(new InvalidOperationException("The service needs to be initialized before it can be used."));
        var builder = CreateBuilder();
        var content = CreateContentMock();
        Assert.DoesNotThrow(() => builder.GetValueSets(content.Object).ToList());
    }

    [Test]
    public void GetValueSets_Uses_DocumentUrlService_For_Variant_Cultures_When_Initialized()
    {
        var contentKey = Guid.NewGuid();
        _documentUrlService.Setup(x => x.IsInitialized).Returns(true);
        _documentUrlService.Setup(x => x.GetUrlSegment(contentKey, "en-US", false)).Returns("english-url");
        _documentUrlService.Setup(x => x.GetUrlSegment(contentKey, "de-DE", false)).Returns("german-url");
        var builder = CreateBuilder();
        var content = CreateContentMock(key: contentKey, variation: ContentVariation.Culture);
        content.Setup(c => c.AvailableCultures).Returns(new[] { "en-US", "de-DE" });
        content.Setup(c => c.IsCulturePublished(It.IsAny<string>())).Returns(true);
        content.Setup(c => c.GetCultureName(It.IsAny<string>())).Returns("Name");
        var result = builder.GetValueSets(content.Object).Single();
        Assert.Multiple(() =>
        {
            Assert.That(GetSingleValue(result, "urlName_en-us"), Is.EqualTo("english-url"));
            Assert.That(GetSingleValue(result, "urlName_de-de"), Is.EqualTo("german-url"));
        });
        // en-US is called twice: once for the invariant urlName (using defaultCulture) and once for the variant urlName_en-us
        _documentUrlService.Verify(x => x.GetUrlSegment(contentKey, "en-US", false), Times.Exactly(2));
        _documentUrlService.Verify(x => x.GetUrlSegment(contentKey, "de-DE", false), Times.Once);
    }

    [Test]
    public void GetValueSets_Falls_Back_For_Variant_Cultures_When_DocumentUrlService_Not_Initialized()
    {
        _documentUrlService.Setup(x => x.IsInitialized).Returns(false);
        var urlSegmentProvider = new Mock<IUrlSegmentProvider>();
        urlSegmentProvider
            .Setup(x => x.GetUrlSegment(It.IsAny<IContentBase>(), It.IsAny<bool>(), "en-US"))
            .Returns("fallback-english");
        urlSegmentProvider
            .Setup(x => x.GetUrlSegment(It.IsAny<IContentBase>(), It.IsAny<bool>(), "fr-FR"))
            .Returns("fallback-french");
        _urlSegmentProviders = new UrlSegmentProviderCollection(() => new[] { urlSegmentProvider.Object });
        var builder = CreateBuilder();
        var content = CreateContentMock(variation: ContentVariation.Culture);
        content.Setup(c => c.AvailableCultures).Returns(new[] { "en-US", "fr-FR" });
        content.Setup(c => c.IsCulturePublished(It.IsAny<string>())).Returns(true);
        content.Setup(c => c.GetCultureName(It.IsAny<string>())).Returns("Name");
        var result = builder.GetValueSets(content.Object).Single();
        Assert.Multiple(() =>
        {
            Assert.That(GetSingleValue(result, "urlName_en-us"), Is.EqualTo("fallback-english"));
            Assert.That(GetSingleValue(result, "urlName_fr-fr"), Is.EqualTo("fallback-french"));
        });
        _documentUrlService.Verify(x => x.GetUrlSegment(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public void GetValueSets_Populates_CreateDate_And_UpdateDate()
    {
        var builder = CreateBuilder();
        var content = CreateContentMock();
        var result = builder.GetValueSets(content.Object).Single();
        Assert.Multiple(() =>
        {
            Assert.That(GetSingleValue(result, "createDate"), Is.EqualTo(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
            Assert.That(GetSingleValue(result, "updateDate"), Is.EqualTo(new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc)));
        });
    }

    [Test]
    public void GetValueSets_Sets_NodeType_To_ContentType_Id()
    {
        var builder = CreateBuilder();
        var content = CreateContentMock(contentTypeId: 555);
        var result = builder.GetValueSets(content.Object).Single();
        Assert.That(GetSingleValue(result, "nodeType"), Is.EqualTo("555"));
    }
}
