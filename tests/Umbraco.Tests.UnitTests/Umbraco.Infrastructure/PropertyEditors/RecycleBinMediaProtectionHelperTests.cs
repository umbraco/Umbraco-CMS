using System.Collections.Concurrent;
using System.Data;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.PropertyEditors.NotificationHandlers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
public class RecycleBinMediaProtectionHelperTests
{
    [Test]
    public void DeleteContainedFilesWithProtection_WithTrashedMedia_DeletesFilesWithSuffix()
    {
        // Arrange
        var deletedFiles = new ConcurrentBag<string>();
        var fileSystemMock = new Mock<IFileSystem>();
        fileSystemMock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
        fileSystemMock
            .Setup(fs => fs.DeleteFile(It.IsAny<string>()))
            .Callback<string>(deletedFiles.Add);

        var mediaFileManager = CreateMediaFileManager(fileSystemMock.Object);

        var trashedMedia = new List<IMedia> { CreateMedia(trashed: true) };
        static IEnumerable<string> ContainedFilePaths(IEnumerable<IMedia> _) => ["media/test/image.jpg"];

        // Act
        RecycleBinMediaProtectionHelper.DeleteContainedFilesWithProtection(
            trashedMedia,
            ContainedFilePaths,
            mediaFileManager);

        // Assert
        Assert.That(deletedFiles, Has.Count.EqualTo(1));
        Assert.That(deletedFiles.Single(), Is.EqualTo("media/test/image.deleted.jpg"));
    }

    [Test]
    public void DeleteContainedFilesWithProtection_WithNonTrashedMedia_DeletesFilesWithoutSuffix()
    {
        // Arrange
        var deletedFiles = new ConcurrentBag<string>();
        var fileSystemMock = new Mock<IFileSystem>();
        fileSystemMock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
        fileSystemMock
            .Setup(fs => fs.DeleteFile(It.IsAny<string>()))
            .Callback<string>(deletedFiles.Add);

        var mediaFileManager = CreateMediaFileManager(fileSystemMock.Object);

        var nonTrashedMedia = new List<IMedia> { CreateMedia(trashed: false) };
        static IEnumerable<string> ContainedFilePaths(IEnumerable<IMedia> _) => ["media/test/image.jpg"];

        // Act
        RecycleBinMediaProtectionHelper.DeleteContainedFilesWithProtection(
            nonTrashedMedia,
            ContainedFilePaths,
            mediaFileManager);

        // Assert
        Assert.That(deletedFiles, Has.Count.EqualTo(1));
        Assert.That(deletedFiles.Single(), Is.EqualTo("media/test/image.jpg"));
    }

    [Test]
    public void DeleteContainedFilesWithProtection_WithTrashedAndNonTrashedMedia_DeletesBothCorrectly()
    {
        // Arrange
        var deletedFiles = new ConcurrentBag<string>();
        var fileSystemMock = new Mock<IFileSystem>();
        fileSystemMock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
        fileSystemMock
            .Setup(fs => fs.DeleteFile(It.IsAny<string>()))
            .Callback<string>(deletedFiles.Add);

        var mediaFileManager = CreateMediaFileManager(fileSystemMock.Object);

        var trashedMedia = CreateMedia(trashed: true);
        var nonTrashedMedia = CreateMedia(trashed: false);
        var media = new List<IMedia> { trashedMedia, nonTrashedMedia };

        // Return different paths based on the media items passed
        static IEnumerable<string> ContainedFilePaths(IEnumerable<IMedia> media)
        {
            foreach (var m in media)
            {
                yield return m.Trashed ? "media/trashed/image.jpg" : "media/normal/image.jpg";
            }
        }

        // Act
        RecycleBinMediaProtectionHelper.DeleteContainedFilesWithProtection(
            media,
            ContainedFilePaths,
            mediaFileManager);

        // Assert
        Assert.That(deletedFiles, Has.Count.EqualTo(2));
        Assert.That(deletedFiles, Does.Contain("media/trashed/image.deleted.jpg"));
        Assert.That(deletedFiles, Does.Contain("media/normal/image.jpg"));
    }

    [Test]
    public void DeleteContainedFilesWithProtection_WithEmptyCollection_DoesNotCallDelete()
    {
        // Arrange
        var fileSystemMock = new Mock<IFileSystem>();
        var mediaFileManager = CreateMediaFileManager(fileSystemMock.Object);

        var emptyMedia = new List<IMedia>();
        static IEnumerable<string> ContainedFilePaths(IEnumerable<IMedia> _) => Array.Empty<string>();

        // Act
        RecycleBinMediaProtectionHelper.DeleteContainedFilesWithProtection(
            emptyMedia,
            ContainedFilePaths,
            mediaFileManager);

        // Assert
        fileSystemMock.Verify(fs => fs.DeleteFile(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void DeleteContainedFilesWithProtection_WithMultipleFiles_DeletesAllWithCorrectSuffix()
    {
        // Arrange
        var deletedFiles = new ConcurrentBag<string>();
        var fileSystemMock = new Mock<IFileSystem>();
        fileSystemMock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
        fileSystemMock
            .Setup(fs => fs.DeleteFile(It.IsAny<string>()))
            .Callback<string>(deletedFiles.Add);

        var mediaFileManager = CreateMediaFileManager(fileSystemMock.Object);

        var trashedMedia = new List<IMedia> { CreateMedia(trashed: true) };
        static IEnumerable<string> ContainedFilePaths(IEnumerable<IMedia> _) =>
        [
            "media/test/photo.jpg",
            "media/test/document.pdf",
            "media/test/video.mp4"
        ];

        // Act
        RecycleBinMediaProtectionHelper.DeleteContainedFilesWithProtection(
            trashedMedia,
            ContainedFilePaths,
            mediaFileManager);

        // Assert
        Assert.That(deletedFiles, Has.Count.EqualTo(3));
        Assert.That(deletedFiles, Does.Contain("media/test/photo.deleted.jpg"));
        Assert.That(deletedFiles, Does.Contain("media/test/document.deleted.pdf"));
        Assert.That(deletedFiles, Does.Contain("media/test/video.deleted.mp4"));
    }

    private static MediaFileManager CreateMediaFileManager(IFileSystem fileSystem)
    {
        var scopeMock = new Mock<ICoreScope>();
        var scopeProviderMock = new Mock<ICoreScopeProvider>();
        scopeProviderMock
            .Setup(sp => sp.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher?>(),
                It.IsAny<IScopedNotificationPublisher?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(scopeMock.Object);

        return new MediaFileManager(
            fileSystem,
            Mock.Of<IMediaPathScheme>(),
            NullLogger<MediaFileManager>.Instance,
            Mock.Of<IShortStringHelper>(),
            Mock.Of<IServiceProvider>(),
            new Lazy<ICoreScopeProvider>(() => scopeProviderMock.Object));
    }

    private static IMedia CreateMedia(bool trashed)
    {
        var mediaMock = new Mock<IMedia>();
        mediaMock.SetupGet(m => m.Trashed).Returns(trashed);
        return mediaMock.Object;
    }
}
