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
        Assert.That(MediaFileManager, Is.Not.Null);
    }

    [Test]
    public void MediaFileManager_Is_Singleton()
    {
        var fileManager2 = GetRequiredService<MediaFileManager>();
        Assert.That(fileManager2, Is.SameAs(MediaFileManager));
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
        Assert.That(File.Exists(physPath), Is.True);

        // ~/media/1234/file.txt is gone
        MediaFileManager.DeleteMediaFiles(new[] { virtualPath });
        Assert.That(File.Exists(physPath), Is.False);

        // ~/media exists
        physPath = Path.GetDirectoryName(physPath); // ~/media/folder
        physPath = Path.GetDirectoryName(physPath); // ~/media
        Assert.That(Directory.Exists(physPath), Is.True);
    }

    [Test]
    public void Deletes_Media_Folder_When_Deleting_Last_Media_Item()
    {
        CreateMediaFile(MediaFileManager, HostingEnvironment, out string virtualPath, out string physicalPath);
        var directoryName = Path.GetDirectoryName(physicalPath);

        Assert.That(File.Exists(physicalPath), Is.True);
        Assert.That(Directory.Exists(directoryName), Is.True);

        MediaFileManager.DeleteMediaFiles([virtualPath]);
        Assert.That(File.Exists(physicalPath), Is.False);
        Assert.That(Directory.Exists(directoryName), Is.False);
    }

    [Test]
    public void Does_Not_Delete_Media_Folder_When_Folder_Is_Not_Emptied()
    {
        var memoryStream = new MemoryStream("test"u8.ToArray());

        CreateMediaFile(MediaFileManager, HostingEnvironment, out string virtualPath, out string physicalPath);

        var secondPath = $"{Path.GetDirectoryName(physicalPath)}/test2.txt";
        MediaFileManager.FileSystem.AddFile(secondPath, memoryStream);

        var directoryName = Path.GetDirectoryName(physicalPath);

        Assert.That(File.Exists(physicalPath), Is.True);
        Assert.That(File.Exists(secondPath), Is.True);
        Assert.That(Directory.Exists(directoryName), Is.True);

        MediaFileManager.DeleteMediaFiles([virtualPath]);
        Assert.That(File.Exists(physicalPath), Is.False);
        Assert.That(File.Exists(secondPath), Is.True);
        Assert.That(Directory.Exists(directoryName), Is.True);
    }

    [Test]
    public void Can_Add_Suffix_To_Media_Files()
    {
        CreateMediaFile(MediaFileManager, HostingEnvironment, out string virtualPath, out string physicalPath);
        Assert.That(File.Exists(physicalPath), Is.True);

        MediaFileManager.SuffixMediaFiles([virtualPath], Cms.Core.Constants.Conventions.Media.TrashedMediaSuffix);
        Assert.That(File.Exists(physicalPath), Is.False);

        var virtualPathWithSuffix = virtualPath.Replace("file.txt", $"file{Cms.Core.Constants.Conventions.Media.TrashedMediaSuffix}.txt");
        physicalPath = HostingEnvironment.MapPathWebRoot(Path.Combine("media", virtualPathWithSuffix));
        Assert.That(File.Exists(physicalPath), Is.True);
    }

    [Test]
    public void Can_Remove_Suffix_From_Media_Files()
    {
        CreateMediaFile(MediaFileManager, HostingEnvironment, out string virtualPath, out string physicalPath);
        MediaFileManager.SuffixMediaFiles([virtualPath], Cms.Core.Constants.Conventions.Media.TrashedMediaSuffix);
        Assert.That(File.Exists(physicalPath), Is.False);

        MediaFileManager.RemoveSuffixFromMediaFiles([virtualPath], Cms.Core.Constants.Conventions.Media.TrashedMediaSuffix);
        Assert.That(File.Exists(physicalPath), Is.False);

        var virtualPathWithSuffix = virtualPath.Replace("file.txt", $"file{Cms.Core.Constants.Conventions.Media.TrashedMediaSuffix}.txt");
        physicalPath = HostingEnvironment.MapPathWebRoot(Path.Combine("media", virtualPathWithSuffix));
        Assert.That(File.Exists(physicalPath), Is.True);
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
        Assert.That(File.Exists(physicalPath), Is.True);
    }
}
