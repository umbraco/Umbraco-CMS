using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;
using IsolationLevel = System.Data.IsolationLevel;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
[Category("UnitTest")]
public class ContentCrudServiceTests
{
    private Mock<ICoreScopeProvider> _scopeProvider = null!;
    private Mock<ILoggerFactory> _loggerFactory = null!;
    private Mock<IEventMessagesFactory> _eventMessagesFactory = null!;
    private Mock<IDocumentRepository> _documentRepository = null!;
    private Mock<IEntityRepository> _entityRepository = null!;
    private Mock<IContentTypeRepository> _contentTypeRepository = null!;
    private Mock<IAuditService> _auditService = null!;
    private Mock<IUserIdKeyResolver> _userIdKeyResolver = null!;
    private Mock<ILanguageRepository> _languageRepository = null!;
    private ContentCrudService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _scopeProvider = new Mock<ICoreScopeProvider>();
        _loggerFactory = new Mock<ILoggerFactory>();
        _eventMessagesFactory = new Mock<IEventMessagesFactory>();
        _documentRepository = new Mock<IDocumentRepository>();
        _entityRepository = new Mock<IEntityRepository>();
        _contentTypeRepository = new Mock<IContentTypeRepository>();
        _auditService = new Mock<IAuditService>();
        _userIdKeyResolver = new Mock<IUserIdKeyResolver>();
        _languageRepository = new Mock<ILanguageRepository>();

        // Setup logger factory to return a mock logger
        _loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());

        // Setup event messages factory
        _eventMessagesFactory.Setup(x => x.Get())
            .Returns(new EventMessages());

        // Setup CreateQuery for Query<T>() calls
        _scopeProvider.Setup(x => x.CreateQuery<IContentType>())
            .Returns(Mock.Of<IQuery<IContentType>>());
        _scopeProvider.Setup(x => x.CreateQuery<IContent>())
            .Returns(Mock.Of<IQuery<IContent>>());

        _sut = new ContentCrudService(
            _scopeProvider.Object,
            _loggerFactory.Object,
            _eventMessagesFactory.Object,
            _documentRepository.Object,
            _entityRepository.Object,
            _contentTypeRepository.Object,
            _auditService.Object,
            _userIdKeyResolver.Object,
            _languageRepository.Object);
    }

    #region Mock Setup Helpers

    /// <summary>
    /// Creates a mock scope configured for read operations.
    /// </summary>
    private ICoreScope CreateMockScopeWithReadLock()
    {
        var scope = new Mock<ICoreScope>();
        scope.Setup(x => x.ReadLock(It.IsAny<int[]>()));
        scope.Setup(x => x.Complete());
        _scopeProvider.Setup(x => x.CreateCoreScope(
            It.IsAny<IsolationLevel>(),
            It.IsAny<RepositoryCacheMode>(),
            It.IsAny<IEventDispatcher?>(),
            It.IsAny<IScopedNotificationPublisher?>(),
            It.IsAny<bool?>(),
            It.IsAny<bool>(),
            It.IsAny<bool>()))
            .Returns(scope.Object);
        return scope.Object;
    }

    /// <summary>
    /// Creates a mock scope configured for write operations.
    /// </summary>
    private ICoreScope CreateMockScopeWithWriteLock()
    {
        var scope = CreateMockScopeWithReadLock();
        Mock.Get(scope).Setup(x => x.WriteLock(It.IsAny<int[]>()));

        var notificationPublisher = new Mock<IScopedNotificationPublisher>();
        notificationPublisher.Setup(x => x.Publish(It.IsAny<INotification>()));
        notificationPublisher.Setup(x => x.PublishCancelable(It.IsAny<ICancelableNotification>())).Returns(false);

        Mock.Get(scope).Setup(x => x.Notifications).Returns(notificationPublisher.Object);
        return scope;
    }

    #endregion

    [Test]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Assert
        Assert.That(_sut, Is.Not.Null);
        Assert.That(_sut, Is.InstanceOf<IContentCrudService>());
    }

    [Test]
    public void Create_WithInvalidParentId_ThrowsArgumentException()
    {
        // Arrange
        CreateMockScopeWithReadLock();
        var contentType = Mock.Of<IContentType>(x => x.Alias == "testType");
        _contentTypeRepository.Setup(x => x.Get(It.IsAny<IQuery<IContentType>>()))
            .Returns(new[] { contentType });
        _documentRepository.Setup(x => x.Get(999)).Returns((IContent?)null);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _sut.Create("Test", 999, "testType"));
    }

    [Test]
    public void Create_WithNonExistentContentType_ThrowsException()
    {
        // Arrange
        CreateMockScopeWithReadLock();
        _contentTypeRepository.Setup(x => x.Get(It.IsAny<IQuery<IContentType>>()))
            .Returns(Enumerable.Empty<IContentType>());

        // Act & Assert
        // Note: Throws generic Exception to match original ContentService behavior
        Assert.Throws<Exception>(() =>
            _sut.Create("Test", Constants.System.Root, "nonExistentType"));
    }

    [Test]
    public void GetById_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        CreateMockScopeWithReadLock();
        _documentRepository.Setup(x => x.Get(999)).Returns((IContent?)null);

        // Act
        var result = _sut.GetById(999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Save_WithEmptyCollection_ReturnsSuccessWithoutCallingRepository()
    {
        // Arrange
        CreateMockScopeWithWriteLock();

        // Act
        var result = _sut.Save(Enumerable.Empty<IContent>());

        // Assert
        Assert.That(result.Success, Is.True);
        _documentRepository.Verify(x => x.Save(It.IsAny<IContent>()), Times.Never);
    }

    [Test]
    public void Exists_WithExistingId_ReturnsTrue()
    {
        // Arrange
        CreateMockScopeWithReadLock();
        _documentRepository.Setup(x => x.Exists(123)).Returns(true);

        // Act
        var result = _sut.Exists(123);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void HasChildren_WithNoChildren_ReturnsFalse()
    {
        // Arrange
        CreateMockScopeWithReadLock();
        _documentRepository.Setup(x => x.Count(It.IsAny<IQuery<IContent>>())).Returns(0);

        // Act
        var result = _sut.HasChildren(123);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void Save_WithVariantContent_CallsLanguageRepository()
    {
        // Arrange
        var scope = CreateMockScopeWithWriteLock();

        var contentType = new Mock<ISimpleContentType>();
        contentType.Setup(x => x.Variations).Returns(ContentVariation.Culture);

        // Use real ContentCultureInfos instance instead of mocking
        var cultureInfo = new ContentCultureInfos("en-US");
        cultureInfo.Name = "Test Name";

        // Use real ContentCultureInfosCollection instead of mocking
        var cultureInfosCollection = new ContentCultureInfosCollection();
        cultureInfosCollection.Add(cultureInfo);

        var content = new Mock<IContent>();
        content.Setup(x => x.ContentType).Returns(contentType.Object);
        content.Setup(x => x.CultureInfos).Returns(cultureInfosCollection);
        content.Setup(x => x.HasIdentity).Returns(true);
        content.Setup(x => x.PublishedState).Returns(PublishedState.Unpublished);
        content.Setup(x => x.Name).Returns("Test");
        content.Setup(x => x.Id).Returns(123);

        // Setup language repository to return languages
        var language = new Mock<ILanguage>();
        language.Setup(x => x.IsoCode).Returns("en-US");
        _languageRepository.Setup(x => x.GetMany()).Returns(new[] { language.Object });

        // Setup async methods for audit
        _userIdKeyResolver.Setup(x => x.GetAsync(It.IsAny<int>()))
            .ReturnsAsync(Guid.NewGuid());
        _auditService.Setup(x => x.AddAsync(
            It.IsAny<AuditType>(),
            It.IsAny<Guid>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<string?>(),
            It.IsAny<string?>()))
            .ReturnsAsync(Attempt.Succeed(AuditLogOperationStatus.Success));

        // Act
        var result = _sut.Save(content.Object);

        // Assert
        Assert.That(result.Success, Is.True);
        _documentRepository.Verify(x => x.Save(content.Object), Times.Once);
        _languageRepository.Verify(x => x.GetMany(), Times.Once);
    }
}
