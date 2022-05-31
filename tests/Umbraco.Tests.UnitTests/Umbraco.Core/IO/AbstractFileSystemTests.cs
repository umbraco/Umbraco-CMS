// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.IO;

[TestFixture]
public abstract class AbstractFileSystemTests
{
    protected IFileSystem _fileSystem;

    protected AbstractFileSystemTests(IFileSystem fileSystem) => _fileSystem = fileSystem;

    [Test]
    public void Can_Create_And_Delete_Files()
    {
        _fileSystem.AddFile("test.txt", CreateStream());

        Assert.IsTrue(_fileSystem.FileExists("test.txt"));

        _fileSystem.DeleteFile("test.txt");

        Assert.IsFalse(_fileSystem.FileExists("test.txt"));
    }

    [Test]
    public void Can_Overwrite_File()
    {
        _fileSystem.AddFile("test/test.txt", CreateStream());
        _fileSystem.AddFile("test/test.txt", CreateStream());

        var files = _fileSystem.GetFiles("test").ToArray();

        Assert.AreEqual(1, files.Count());

        foreach (var file in files)
        {
            _fileSystem.DeleteFile(file);
        }

        _fileSystem.DeleteDirectory("test", true);
    }

    [Test]
    public void Cant_Overwrite_File() =>
        Assert.Throws<InvalidOperationException>(() =>
        {
            _fileSystem.AddFile("test.txt", CreateStream());
            _fileSystem.AddFile("test.txt", CreateStream(), false);

            _fileSystem.DeleteFile("test.txt");
        });

    [Test]
    public void Can_Get_Files()
    {
        _fileSystem.AddFile("test/test1.txt", CreateStream());
        _fileSystem.AddFile("test/test2.txt", CreateStream());
        _fileSystem.AddFile("test/test3.txt", CreateStream());
        _fileSystem.AddFile("test/test4.bak", CreateStream());

        var files = _fileSystem.GetFiles("test");

        Assert.AreEqual(4, files.Count());

        files = _fileSystem.GetFiles("test", "*.txt");

        Assert.AreEqual(3, files.Count());

        _fileSystem.DeleteDirectory("test", true);
    }

    [Test]
    public void Can_Read_File()
    {
        _fileSystem.AddFile("test.txt", CreateStream("hello world"));

        var stream = _fileSystem.OpenFile("test.txt");
        var reader = new StreamReader(stream);
        var contents = reader.ReadToEnd();
        reader.Close();

        Assert.AreEqual("hello world", contents);

        _fileSystem.DeleteFile("test.txt");
    }

    [Test]
    public void Can_Get_Directories()
    {
        _fileSystem.AddFile("test/sub1/test.txt", CreateStream());
        _fileSystem.AddFile("test/sub2/test.txt", CreateStream());
        _fileSystem.AddFile("test/sub3/test.txt", CreateStream());

        var dirs = _fileSystem.GetDirectories("test");

        Assert.AreEqual(3, dirs.Count());
        Assert.IsTrue(_fileSystem.DirectoryExists("test/sub1"));
        Assert.IsTrue(_fileSystem.DirectoryExists("test/sub2"));
        Assert.IsTrue(_fileSystem.DirectoryExists("test/sub3"));

        _fileSystem.DeleteDirectory("test", true);
    }

    [Test]
    public void Can_Get_File_Dates()
    {
        _fileSystem.DeleteFile("test.txt");

        _fileSystem.AddFile("test.txt", CreateStream());

        var created = _fileSystem.GetCreated("test.txt");
        var modified = _fileSystem.GetLastModified("test.txt");

        Assert.AreEqual(DateTime.UtcNow.Year, created.Year);
        Assert.AreEqual(DateTime.UtcNow.Month, created.Month);
        Assert.AreEqual(DateTime.UtcNow.Date, created.Date);

        Assert.AreEqual(DateTime.UtcNow.Year, modified.Year);
        Assert.AreEqual(DateTime.UtcNow.Month, modified.Month);
        Assert.AreEqual(DateTime.UtcNow.Date, modified.Date);

        _fileSystem.DeleteFile("test.txt");
    }

    [Test]
    public void Can_Get_File_Url()
    {
        _fileSystem.AddFile("test.txt", CreateStream());

        var url = _fileSystem.GetUrl("test.txt");

        Assert.AreEqual(ConstructUrl("test.txt"), url);

        _fileSystem.DeleteFile("test.txt");
    }

    [Test]
    public void Can_Convert_Full_Path_And_Url_To_Relative_Path()
    {
        _fileSystem.AddFile("test.txt", CreateStream());

        var url = _fileSystem.GetUrl("test.txt");
        var fullPath = _fileSystem.GetFullPath("test.txt");

        Assert.AreNotEqual("test.txt", url);
        Assert.AreNotEqual("test.txt", fullPath);

        Assert.AreEqual("test.txt", _fileSystem.GetRelativePath(url));
        Assert.AreEqual("test.txt", _fileSystem.GetRelativePath(fullPath));

        _fileSystem.DeleteFile("test.txt");
    }

    [Test]
    public void Can_Get_Size()
    {
        var stream = CreateStream();
        var streamLength = stream.Length;
        _fileSystem.AddFile("test.txt", stream);

        Assert.AreEqual(streamLength, _fileSystem.GetSize("test.txt"));

        _fileSystem.DeleteFile("test.txt");
    }

    protected Stream CreateStream(string contents = null)
    {
        if (string.IsNullOrEmpty(contents))
        {
            contents = "test";
        }

        var bytes = Encoding.UTF8.GetBytes(contents);
        var stream = new MemoryStream(bytes);

        return stream;
    }

    protected abstract string ConstructUrl(string path);
}
