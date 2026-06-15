// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.IO;

[TestFixture]
public class PhysicalFileSystemTests : AbstractFileSystemTests
{
    [TearDown]
    public void TearDown()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSysTests");
        if (Directory.Exists(path) == false)
        {
            return;
        }

        var files = Directory.GetFiles(path);
        foreach (var f in files)
        {
            File.Delete(f);
        }

        Directory.Delete(path, true);
    }

    public PhysicalFileSystemTests()
        : base(new PhysicalFileSystem(
            TestHelper.IOHelper,
            TestHelper.GetHostingEnvironment(),
            Mock.Of<ILogger<PhysicalFileSystem>>(),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSysTests"),
            "/Media/"))
    { }

    protected override string ConstructUrl(string path) => "/Media/" + path;

    [Test]
    public void SaveFileTest()
    {
        var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSysTests");

        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            _fileSystem.AddFile("sub/f3.txt", ms);
        }

        Assert.That(File.Exists(Path.Combine(basePath, "sub", "f3.txt")), Is.True);
    }

    [Test]
    public void MoveFileTest()
    {
        var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSysTests");

        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            _fileSystem.AddFile("sub/f3.txt", ms);
        }

        Assert.That(File.Exists(Path.Combine(basePath, "sub/f3.txt")), Is.True);

        _fileSystem.MoveFile("sub/f3.txt", "sub2/f4.txt");

        Assert.That(File.Exists(Path.Combine(basePath, "sub/f3.txt")), Is.False);
        Assert.That(File.Exists(Path.Combine(basePath, "sub2/f4.txt")), Is.True);
    }

    [Test]
    public void GetFullPathTest()
    {
        // outside of tests, one initializes the PhysicalFileSystem with eg ~/Dir
        // and then, rootPath = /path/to/Dir and rootUrl = /Dir/
        // here we initialize the PhysicalFileSystem with
        // rootPath = /path/to/FileSysTests
        // rootUrl = /Media/
        var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSysTests");

        // ensure that GetFullPath
        // - does return the proper full path
        // - does properly normalize separators
        // - does throw on invalid paths
        // works
        var path = _fileSystem.GetFullPath("foo.tmp");
        Assert.That(path, Is.EqualTo(Path.Combine(basePath, "foo.tmp")));

        // normalize path with parent directory references
        path = "foo/../bar/../bah/../";
        path = _fileSystem.GetFullPath(path + "foo.tmp");
        Assert.That(path, Is.EqualTo(Path.Combine(basePath, "foo.tmp")));

        // works too
        path = _fileSystem.GetFullPath("foo/bar.tmp");
        Assert.That(path, Is.EqualTo(Path.Combine(basePath, "foo", "bar.tmp")));

        // that path is invalid as it would be outside the root directory
        Assert.Throws<UnauthorizedAccessException>(() => _fileSystem.GetFullPath("../../foo.tmp"));
    }

    [Test]
    public void CreateFileProviderCreatesDirectoryIfMissing()
    {
        var nonExistentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSysTests", "NonExistent");
        if (Directory.Exists(nonExistentPath))
        {
            Directory.Delete(nonExistentPath, true);
        }

        var fs = new PhysicalFileSystem(
            TestHelper.IOHelper,
            TestHelper.GetHostingEnvironment(),
            Mock.Of<ILogger<PhysicalFileSystem>>(),
            nonExistentPath,
            "/Media/");

        IFileProvider fileProvider = ((IFileProviderFactory)fs).Create();
        try
        {
            Assert.That(fileProvider, Is.Not.Null);
            Assert.That(Directory.Exists(nonExistentPath), Is.True);
        }
        finally
        {
            (fileProvider as IDisposable)?.Dispose();
        }
    }
}
