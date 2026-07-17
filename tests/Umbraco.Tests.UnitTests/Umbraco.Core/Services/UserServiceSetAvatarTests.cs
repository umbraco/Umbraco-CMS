using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Editors;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class UserServiceSetAvatarTests
{
    private readonly Guid _userKey = Guid.NewGuid();
    private readonly Guid _temporaryFileKey = Guid.NewGuid();

    [TestCase("avatar.png")]
    [TestCase("avatar.jpg")]
    [TestCase("avatar.webp")]
    [TestCase("AVATAR.PNG")]
    public async Task Can_Set_Avatar_With_Default_Image_Type(string fileName)
    {
        UserService userService = CreateUserService(new ContentSettings(), fileName);

        UserOperationStatus result = await userService.SetAvatarAsync(_userKey, _temporaryFileKey);

        Assert.AreEqual(UserOperationStatus.Success, result);
    }

    [TestCase("avatar.exe")]
    [TestCase("avatar")]
    // "if" is a substring of "tiff" in the default image file types but not a valid extension itself.
    [TestCase("avatar.if")]
    public async Task Cannot_Set_Avatar_With_Invalid_Extension(string fileName)
    {
        UserService userService = CreateUserService(new ContentSettings(), fileName);

        UserOperationStatus result = await userService.SetAvatarAsync(_userKey, _temporaryFileKey);

        Assert.AreEqual(UserOperationStatus.InvalidAvatar, result);
    }

    [TestCase("avatar.heic")]
    [TestCase("avatar.png")]
    public async Task Can_Set_Avatar_With_Custom_Configured_Image_Type(string fileName)
    {
        var contentSettings = new ContentSettings
        {
            Imaging = new ContentImagingSettings { ImageFileTypes = new HashSet<string> { "png", "heic" } },
        };
        UserService userService = CreateUserService(contentSettings, fileName);

        UserOperationStatus result = await userService.SetAvatarAsync(_userKey, _temporaryFileKey);

        Assert.AreEqual(UserOperationStatus.Success, result);
    }

    [TestCase("avatar.jpg")]
    [TestCase("avatar.gif")]
    public async Task Cannot_Set_Avatar_With_Type_Not_In_Custom_Configured_Image_Types(string fileName)
    {
        var contentSettings = new ContentSettings
        {
            Imaging = new ContentImagingSettings { ImageFileTypes = new HashSet<string> { "png", "heic" } },
        };
        UserService userService = CreateUserService(contentSettings, fileName);

        UserOperationStatus result = await userService.SetAvatarAsync(_userKey, _temporaryFileKey);

        Assert.AreEqual(UserOperationStatus.InvalidAvatar, result);
    }

    [Test]
    public async Task Cannot_Set_Avatar_When_Extension_Is_Disallowed()
    {
        var contentSettings = new ContentSettings
        {
            DisallowedUploadedFileExtensions = new HashSet<string> { "png" },
        };
        UserService userService = CreateUserService(contentSettings, "avatar.png");

        UserOperationStatus result = await userService.SetAvatarAsync(_userKey, _temporaryFileKey);

        Assert.AreEqual(UserOperationStatus.InvalidAvatar, result);
    }

    private UserService CreateUserService(ContentSettings contentSettings, string temporaryFileName)
    {
        var scopeProviderMock = new Mock<ICoreScopeProvider>();
        scopeProviderMock
            .Setup(x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher?>(),
                It.IsAny<IScopedNotificationPublisher?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(Mock.Of<ICoreScope>());

        var user = new User(new GlobalSettings()) { Key = _userKey, Username = "test", Name = "Test" };
        var backOfficeUserReaderMock = new Mock<IBackOfficeUserReader>();
        backOfficeUserReaderMock.Setup(x => x.GetByKey(_userKey)).Returns(user);

        var temporaryFileServiceMock = new Mock<ITemporaryFileService>();
        temporaryFileServiceMock
            .Setup(x => x.GetAsync(_temporaryFileKey))
            .ReturnsAsync(new TemporaryFileModel { FileName = temporaryFileName, Key = _temporaryFileKey, AvailableUntil = DateTime.MaxValue });

        var backOfficeUserStoreMock = new Mock<IBackOfficeUserStore>();
        backOfficeUserStoreMock.Setup(x => x.SaveAsync(It.IsAny<IUser>())).ReturnsAsync(UserOperationStatus.Success);
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IBackOfficeUserStore))).Returns(backOfficeUserStoreMock.Object);
        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);

        var shortStringHelper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig());

        var mediaFileManager = new MediaFileManager(
            Mock.Of<IFileSystem>(),
            Mock.Of<IMediaPathScheme>(),
            NullLogger<MediaFileManager>.Instance,
            shortStringHelper,
            Mock.Of<IServiceProvider>(),
            new Lazy<ICoreScopeProvider>(() => scopeProviderMock.Object));

        return new UserService(
            scopeProviderMock.Object,
            NullLoggerFactory.Instance,
            Mock.Of<IEventMessagesFactory>(),
            Mock.Of<IUserRepository>(),
            Mock.Of<IUserGroupRepository>(),
            Options.Create(new GlobalSettings()),
            Options.Create(new SecuritySettings()),
            new UserEditorAuthorizationHelper(
                Mock.Of<IContentService>(),
                Mock.Of<IMediaService>(),
                Mock.Of<IEntityService>(),
                AppCaches.Disabled),
            serviceScopeFactoryMock.Object,
            Mock.Of<IEntityService>(),
            Mock.Of<ILocalLoginSettingProvider>(),
            Mock.Of<IUserInviteSender>(),
            mediaFileManager,
            temporaryFileServiceMock.Object,
            shortStringHelper,
            Options.Create(contentSettings),
            Mock.Of<IIsoCodeValidator>(),
            Mock.Of<IUserForgotPasswordSender>(),
            Mock.Of<IUserIdKeyResolver>(),
            backOfficeUserReaderMock.Object);
    }
}
