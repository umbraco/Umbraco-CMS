// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.IO.MediaPathSchemes;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.IO;

[TestFixture]
[UmbracoTest]
public class FileSystemsTests : UmbracoIntegrationTest
{
    [Test]
    public void Can_Get_MediaFileManager()
    {
        var fileSystem = GetRequiredService<MediaFileManager>();
        Assert.NotNull(fileSystem);
    }

    [Test]
    public void MediaFileManager_Is_Singleton()
    {
        var fileManager1 = GetRequiredService<MediaFileManager>();
        var fileManager2 = GetRequiredService<MediaFileManager>();
        Assert.AreSame(fileManager1, fileManager2);
    }

    [Test]
    public void Can_Delete_MediaFiles()
    {
        var mediaFileManager = GetRequiredService<MediaFileManager>();
        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes("test"));
        var virtualPath = mediaFileManager.GetMediaPath("file.txt", Guid.NewGuid(), Guid.NewGuid());
        mediaFileManager.FileSystem.AddFile(virtualPath, memoryStream);

        // ~/media/1234/file.txt exists
        var hostingEnvironment = GetRequiredService<IHostingEnvironment>();
        var physPath = hostingEnvironment.MapPathWebRoot(Path.Combine("media", virtualPath));
        Assert.IsTrue(File.Exists(physPath));

        // ~/media/1234/file.txt is gone
        mediaFileManager.DeleteMediaFiles(new[] { virtualPath });
        Assert.IsFalse(File.Exists(physPath));

        var scheme = GetRequiredService<IMediaPathScheme>();
        if (scheme is UniqueMediaPathScheme)
        {
            // ~/media/1234 is *not* gone
            physPath = Path.GetDirectoryName(physPath);
            Assert.IsTrue(Directory.Exists(physPath));
        }
        else
        {
            // ~/media/1234 is gone
            physPath = Path.GetDirectoryName(physPath);
            Assert.IsFalse(Directory.Exists(physPath));
        }

        // ~/media exists
        physPath = Path.GetDirectoryName(physPath);
        Assert.IsTrue(Directory.Exists(physPath));
    }

    // FIXME: don't make sense anymore
    /*
    [Test]
    public void Cannot_Get_InvalidFileSystem()
    {
        // throws because InvalidTypedFileSystem does not have the proper attribute with an alias
        Assert.Throws<InvalidOperationException>(() => FileSystems.GetFileSystem<InvalidFileSystem>());
    }

    [Test]
    public void Cannot_Get_NonConfiguredFileSystem()
    {
        // note: we need to reset the manager between tests else the Accept_Fallback test would corrupt that one
        // throws because NonConfiguredFileSystem has the proper attribute with an alias,
        // but then the container cannot find an IFileSystem implementation for that alias
        Assert.Throws<InvalidOperationException>(() => FileSystems.GetFileSystem<NonConfiguredFileSystem>());

        // all we'd need to pass is to register something like:
        //_container.Register<IFileSystem>("noconfig", factory => new PhysicalFileSystem("~/foo"));
    }

    internal class InvalidFileSystem : FileSystemWrapper
    {
        public InvalidFileSystem(IFileSystem innerFileSystem)
            : base(innerFileSystem)
        { }
    }

    [InnerFileSystem("noconfig")]
    internal class NonConfiguredFileSystem : FileSystemWrapper
    {
        public NonConfiguredFileSystem(IFileSystem innerFileSystem)
            : base(innerFileSystem)
        { }
    }
    */
}
