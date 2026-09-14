// Copyright (c) Umbraco.
// See LICENSE for more details.

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

        Assert.That(_fileSystem.FileExists("test.txt"), Is.True);

        _fileSystem.DeleteFile("test.txt");

        Assert.That(_fileSystem.FileExists("test.txt"), Is.False);
    }

    [Test]
    public void Can_Overwrite_File()
    {
        _fileSystem.AddFile("test/test.txt", CreateStream());
        _fileSystem.AddFile("test/test.txt", CreateStream());

        var files = _fileSystem.GetFiles("test").ToArray();

        Assert.That(files.Count(), Is.EqualTo(1));

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

        Assert.That(files.Count(), Is.EqualTo(4));

        files = _fileSystem.GetFiles("test", "*.txt");

        Assert.That(files.Count(), Is.EqualTo(3));

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

        Assert.That(contents, Is.EqualTo("hello world"));

        _fileSystem.DeleteFile("test.txt");
    }

    [Test]
    public void Can_Get_Directories()
    {
        _fileSystem.AddFile("test/sub1/test.txt", CreateStream());
        _fileSystem.AddFile("test/sub2/test.txt", CreateStream());
        _fileSystem.AddFile("test/sub3/test.txt", CreateStream());

        var dirs = _fileSystem.GetDirectories("test");

        Assert.That(dirs.Count(), Is.EqualTo(3));
        Assert.That(_fileSystem.DirectoryExists("test/sub1"), Is.True);
        Assert.That(_fileSystem.DirectoryExists("test/sub2"), Is.True);
        Assert.That(_fileSystem.DirectoryExists("test/sub3"), Is.True);

        _fileSystem.DeleteDirectory("test", true);
    }

    [Test]
    public void Can_Get_File_Dates()
    {
        _fileSystem.DeleteFile("test.txt");

        _fileSystem.AddFile("test.txt", CreateStream());

        var created = _fileSystem.GetCreated("test.txt");
        var modified = _fileSystem.GetLastModified("test.txt");

        Assert.That(created.Year, Is.EqualTo(DateTime.UtcNow.Year));
        Assert.That(created.Month, Is.EqualTo(DateTime.UtcNow.Month));
        Assert.That(created.Date, Is.EqualTo(DateTime.UtcNow.Date));

        Assert.That(modified.Year, Is.EqualTo(DateTime.UtcNow.Year));
        Assert.That(modified.Month, Is.EqualTo(DateTime.UtcNow.Month));
        Assert.That(modified.Date, Is.EqualTo(DateTime.UtcNow.Date));

        _fileSystem.DeleteFile("test.txt");
    }

    [Test]
    public void Can_Get_File_Url()
    {
        _fileSystem.AddFile("test.txt", CreateStream());

        var url = _fileSystem.GetUrl("test.txt");

        Assert.That(url, Is.EqualTo(ConstructUrl("test.txt")));

        _fileSystem.DeleteFile("test.txt");
    }

    [Test]
    public void Can_Convert_Full_Path_And_Url_To_Relative_Path()
    {
        _fileSystem.AddFile("test.txt", CreateStream());

        var url = _fileSystem.GetUrl("test.txt");
        var fullPath = _fileSystem.GetFullPath("test.txt");

        Assert.That(url, Is.Not.EqualTo("test.txt"));
        Assert.That(fullPath, Is.Not.EqualTo("test.txt"));

        Assert.That(_fileSystem.GetRelativePath(url), Is.EqualTo("test.txt"));
        Assert.That(_fileSystem.GetRelativePath(fullPath), Is.EqualTo("test.txt"));

        _fileSystem.DeleteFile("test.txt");
    }

    [Test]
    public void Can_Get_Size()
    {
        var stream = CreateStream();
        var streamLength = stream.Length;
        _fileSystem.AddFile("test.txt", stream);

        Assert.That(_fileSystem.GetSize("test.txt"), Is.EqualTo(streamLength));

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
