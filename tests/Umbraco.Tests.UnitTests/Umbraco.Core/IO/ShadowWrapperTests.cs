using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.IO;

[TestFixture]
public class ShadowWrapperTests
{
    [Test]
    public void MoveFile_Through_IFileSystem_Moves_In_The_Inner_FileSystem()
    {
        var inner = new Mock<IFileSystem>();
        inner.Setup(x => x.FileExists("media/abc/image.jpg")).Returns(true);
        inner.Setup(x => x.OpenFile("media/abc/image.jpg")).Returns(() => new MemoryStream([1, 2, 3]));
        IFileSystem wrapper = CreateWrapper(inner.Object);

        wrapper.MoveFile("media/abc/image.jpg", "media/abc/image.deleted.jpg");

        // A move, not a copy and a delete: the inner file system's own move is atomic and retries,
        // where a copy followed by a failed delete leaves the file at both paths.
        inner.Verify(x => x.MoveFile("media/abc/image.jpg", "media/abc/image.deleted.jpg", true), Times.Once);
        inner.Verify(x => x.AddFile(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<bool>()), Times.Never);
        inner.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void MoveFile_Through_IFileSystem_Passes_On_OverrideIfExists()
    {
        var inner = new Mock<IFileSystem>();
        IFileSystem wrapper = CreateWrapper(inner.Object);

        wrapper.MoveFile("media/abc/image.jpg", "media/abc/image.deleted.jpg", false);

        inner.Verify(x => x.MoveFile("media/abc/image.jpg", "media/abc/image.deleted.jpg", false), Times.Once);
    }

    private static ShadowWrapper CreateWrapper(IFileSystem inner)
        => new(inner, Mock.Of<IIOHelper>(), Mock.Of<IHostingEnvironment>(), NullLoggerFactory.Instance, "media");
}
