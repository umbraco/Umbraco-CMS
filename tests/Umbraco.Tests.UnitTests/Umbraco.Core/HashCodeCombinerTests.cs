// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.IO;
using System.Reflection;
using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core;

[TestFixture]
public class HashCodeCombinerTests
{
    private DirectoryInfo PrepareFolder()
    {
        var assDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
        var dir = Directory.CreateDirectory(
            Path.Combine(assDir.FullName, "HashCombiner", Guid.NewGuid().ToString("N")));
        foreach (var f in dir.GetFiles())
        {
            f.Delete();
        }

        return dir;
    }

    [Test]
    public void HashCombiner_Test_String()
    {
        var combiner1 = new HashCodeCombiner();
        combiner1.AddCaseInsensitiveString("Hello");

        var combiner2 = new HashCodeCombiner();
        combiner2.AddCaseInsensitiveString("hello");

        Assert.That(combiner2.GetCombinedHashCode(), Is.EqualTo(combiner1.GetCombinedHashCode()));

        combiner2.AddCaseInsensitiveString("world");

        Assert.That(combiner2.GetCombinedHashCode(), Is.Not.EqualTo(combiner1.GetCombinedHashCode()));
    }

    [Test]
    public void HashCombiner_Test_Int()
    {
        var combiner1 = new HashCodeCombiner();
        combiner1.AddInt(1234);

        var combiner2 = new HashCodeCombiner();
        combiner2.AddInt(1234);

        Assert.That(combiner2.GetCombinedHashCode(), Is.EqualTo(combiner1.GetCombinedHashCode()));

        combiner2.AddInt(1);

        Assert.That(combiner2.GetCombinedHashCode(), Is.Not.EqualTo(combiner1.GetCombinedHashCode()));
    }

    [Test]
    public void HashCombiner_Test_DateTime()
    {
        var dt = DateTime.Now;
        var combiner1 = new HashCodeCombiner();
        combiner1.AddDateTime(dt);

        var combiner2 = new HashCodeCombiner();
        combiner2.AddDateTime(dt);

        Assert.That(combiner2.GetCombinedHashCode(), Is.EqualTo(combiner1.GetCombinedHashCode()));

        combiner2.AddDateTime(DateTime.Now);

        Assert.That(combiner2.GetCombinedHashCode(), Is.Not.EqualTo(combiner1.GetCombinedHashCode()));
    }

    [Test]
    public void HashCombiner_Test_File()
    {
        var dir = PrepareFolder();
        var file1Path = Path.Combine(dir.FullName, "hastest1.txt");
        File.Delete(file1Path);
        using (var file1 = File.CreateText(Path.Combine(dir.FullName, "hastest1.txt")))
        {
            file1.WriteLine("hello");
        }

        var file2Path = Path.Combine(dir.FullName, "hastest2.txt");
        File.Delete(file2Path);
        using (var file2 = File.CreateText(Path.Combine(dir.FullName, "hastest2.txt")))
        {
            // even though files are the same, the dates are different
            file2.WriteLine("hello");
        }

        var combiner1 = new HashCodeCombiner();
        combiner1.AddFile(new FileInfo(file1Path));

        var combiner2 = new HashCodeCombiner();
        combiner2.AddFile(new FileInfo(file1Path));

        var combiner3 = new HashCodeCombiner();
        combiner3.AddFile(new FileInfo(file2Path));

        Assert.That(combiner2.GetCombinedHashCode(), Is.EqualTo(combiner1.GetCombinedHashCode()));
        Assert.That(combiner3.GetCombinedHashCode(), Is.Not.EqualTo(combiner1.GetCombinedHashCode()));

        combiner2.AddFile(new FileInfo(file2Path));

        Assert.That(combiner2.GetCombinedHashCode(), Is.Not.EqualTo(combiner1.GetCombinedHashCode()));
    }

    [Test]
    public void HashCombiner_Test_Folder()
    {
        var dir = PrepareFolder();
        var file1Path = Path.Combine(dir.FullName, "hastest1.txt");
        File.Delete(file1Path);
        using (var file1 = File.CreateText(Path.Combine(dir.FullName, "hastest1.txt")))
        {
            file1.WriteLine("hello");
        }

        // first test the whole folder
        var combiner1 = new HashCodeCombiner();
        combiner1.AddFolder(dir);

        var combiner2 = new HashCodeCombiner();
        combiner2.AddFolder(dir);

        Assert.That(combiner2.GetCombinedHashCode(), Is.EqualTo(combiner1.GetCombinedHashCode()));

        // now add a file to the folder
        var file2Path = Path.Combine(dir.FullName, "hastest2.txt");
        File.Delete(file2Path);
        using (var file2 = File.CreateText(Path.Combine(dir.FullName, "hastest2.txt")))
        {
            // even though files are the same, the dates are different
            file2.WriteLine("hello");
        }

        var combiner3 = new HashCodeCombiner();
        combiner3.AddFolder(dir);

        Assert.That(combiner3.GetCombinedHashCode(), Is.Not.EqualTo(combiner1.GetCombinedHashCode()));
    }
}
