// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.IO;

[TestFixture]
public class PhysicalFileSystemTests : AbstractFileSystemTests
{
    [SetUp]
    public void Setup()
    {
    }

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
    {
    }

    protected override string ConstructUrl(string path) => "/Media/" + path;

    private string Repeat(string pattern, int count)
    {
        var text = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            text.Append(pattern);
        }

        return text.ToString();
    }

    [Test]
    public void SaveFileTest()
    {
        var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSysTests");

        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
        {
            _fileSystem.AddFile("sub/f3.txt", ms);
        }

        Assert.IsTrue(File.Exists(Path.Combine(basePath, "sub/f3.txt")));

        var path = Repeat("bah/bah/", 50);
        Assert.Less(260, path.Length);

        Assert.Throws<PathTooLongException>(() =>
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo"));
            _fileSystem.AddFile(path + "f3.txt", ms);
        });
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
        Assert.AreEqual(Path.Combine(basePath, @"foo.tmp"), path);

        // a very long relative path, which ends up being a short path, works
        path = Repeat("bah/../", 50);
        Assert.Less(260, path.Length);
        path = _fileSystem.GetFullPath(path + "foo.tmp");
        Assert.AreEqual(Path.Combine(basePath, @"foo.tmp"), path);

        // works too
        path = _fileSystem.GetFullPath("foo/bar.tmp");
        Assert.AreEqual(Path.Combine(basePath, @$"foo{Path.DirectorySeparatorChar}bar.tmp"), path);

        // that path is invalid as it would be outside the root directory
        Assert.Throws<UnauthorizedAccessException>(() => _fileSystem.GetFullPath("../../foo.tmp"));

        // a very long path, which ends up being very long, works
        path = Repeat("bah/bah/", 50);
        Assert.Less(260, path.Length);
        Assert.Throws<PathTooLongException>(() =>
        {
            path = _fileSystem.GetFullPath(path + "foo.tmp");
            Assert.Less(260, path.Length); // gets a >260 path and it's fine (but Windows will not like it)
        });
    }
}
