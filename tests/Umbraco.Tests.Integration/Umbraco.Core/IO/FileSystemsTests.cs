// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.IO;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class FileSystemsTests : UmbracoIntegrationTest
{
    private MediaFileManager MediaFileManager => GetRequiredService<MediaFileManager>();

    private IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();

    [Test]
    public void Can_Get_MediaFileManager()
    {
        Assert.NotNull(MediaFileManager);
    }

    [Test]
    public void MediaFileManager_Is_Singleton()
    {
        var fileManager2 = GetRequiredService<MediaFileManager>();
        Assert.AreSame(MediaFileManager, fileManager2);
    }

    [Test]
    [LongRunning]
    public void Can_Delete_MediaFiles()
    {
        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes("test"));
        var virtualPath = MediaFileManager.GetMediaPath("file.txt", Guid.NewGuid(), Guid.NewGuid());
        MediaFileManager.FileSystem.AddFile(virtualPath, memoryStream);

        // ~/media/1234/file.txt exists
        var physPath = HostingEnvironment.MapPathWebRoot(Path.Combine("media", virtualPath));
        Assert.IsTrue(File.Exists(physPath));

        // ~/media/1234/file.txt is gone
        MediaFileManager.DeleteMediaFiles(new[] { virtualPath });
        Assert.IsFalse(File.Exists(physPath));

        // ~/media exists
        physPath = Path.GetDirectoryName(physPath); // ~/media/folder
        physPath = Path.GetDirectoryName(physPath); // ~/media
        Assert.IsTrue(Directory.Exists(physPath));
    }

    [Test]
    public void Deletes_Media_Folder_When_Deleting_Last_Media_Item()
    {
        CreateMediaFile(MediaFileManager, HostingEnvironment, out string virtualPath, out string physicalPath);
        var directoryName = Path.GetDirectoryName(physicalPath);

        Assert.IsTrue(File.Exists(physicalPath));
        Assert.IsTrue(Directory.Exists(directoryName));

        MediaFileManager.DeleteMediaFiles([virtualPath]);
        Assert.IsFalse(File.Exists(physicalPath));
        Assert.IsFalse(Directory.Exists(directoryName));
    }

    [Test]
    public void Does_Not_Delete_Media_Folder_When_Folder_Is_Not_Emptied()
    {
        var memoryStream = new MemoryStream("test"u8.ToArray());

        CreateMediaFile(MediaFileManager, HostingEnvironment, out string virtualPath, out string physicalPath);

        var secondPath = $"{Path.GetDirectoryName(physicalPath)}/test2.txt";
        MediaFileManager.FileSystem.AddFile(secondPath, memoryStream);

        var directoryName = Path.GetDirectoryName(physicalPath);

        Assert.IsTrue(File.Exists(physicalPath));
        Assert.IsTrue(File.Exists(secondPath));
        Assert.IsTrue(Directory.Exists(directoryName));

        MediaFileManager.DeleteMediaFiles([virtualPath]);
        Assert.IsFalse(File.Exists(physicalPath));
        Assert.IsTrue(File.Exists(secondPath));
        Assert.True(Directory.Exists(directoryName));
    }

    [Test]
    public void Can_Add_Suffix_To_Media_Files()
    {
        CreateMediaFile(MediaFileManager, HostingEnvironment, out string virtualPath, out string physicalPath);
        Assert.IsTrue(File.Exists(physicalPath));

        MediaFileManager.SuffixMediaFiles([virtualPath], Cms.Core.Constants.Conventions.Media.TrashedMediaSuffix);
        Assert.IsFalse(File.Exists(physicalPath));

        var virtualPathWithSuffix = virtualPath.Replace("file.txt", $"file{Cms.Core.Constants.Conventions.Media.TrashedMediaSuffix}.txt");
        physicalPath = HostingEnvironment.MapPathWebRoot(Path.Combine("media", virtualPathWithSuffix));
        Assert.IsTrue(File.Exists(physicalPath));
    }

    [Test]
    public void Can_Remove_Suffix_From_Media_Files()
    {
        CreateMediaFile(MediaFileManager, HostingEnvironment, out string virtualPath, out string physicalPath);
        MediaFileManager.SuffixMediaFiles([virtualPath], Cms.Core.Constants.Conventions.Media.TrashedMediaSuffix);
        Assert.IsFalse(File.Exists(physicalPath));

        MediaFileManager.RemoveSuffixFromMediaFiles([virtualPath], Cms.Core.Constants.Conventions.Media.TrashedMediaSuffix);
        Assert.IsFalse(File.Exists(physicalPath));

        var virtualPathWithSuffix = virtualPath.Replace("file.txt", $"file{Cms.Core.Constants.Conventions.Media.TrashedMediaSuffix}.txt");
        physicalPath = HostingEnvironment.MapPathWebRoot(Path.Combine("media", virtualPathWithSuffix));
        Assert.IsTrue(File.Exists(physicalPath));
    }

    private static void CreateMediaFile(
        MediaFileManager mediaFileManager,
        IHostingEnvironment hostingEnvironment,
        out string virtualPath,
        out string physicalPath)
    {
        virtualPath = mediaFileManager.GetMediaPath("file.txt", Guid.NewGuid(), Guid.NewGuid());
        physicalPath = hostingEnvironment.MapPathWebRoot(Path.Combine("media", virtualPath));

        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes("test"));
        mediaFileManager.FileSystem.AddFile(virtualPath, memoryStream);
        Assert.IsTrue(File.Exists(physicalPath));
    }
}
