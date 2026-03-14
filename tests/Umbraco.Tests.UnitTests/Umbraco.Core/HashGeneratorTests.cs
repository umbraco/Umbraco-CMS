// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.IO;
using System.Reflection;
using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core;

/// <summary>
/// Contains unit tests that verify the functionality of the <see cref="HashGenerator"/> class.
/// </summary>
[TestFixture]
public class HashGeneratorTests
{
    private string Generate(bool isCaseSensitive, params string[] strs)
    {
        using var generator = new HashGenerator();
        foreach (var str in strs)
        {
            if (isCaseSensitive)
            {
                generator.AddString(str);
            }
            else
            {
                generator.AddCaseInsensitiveString(str);
            }
        }

        return generator.GenerateHash();
    }

    /// <summary>
    /// Tests that generating hashes from multiple strings is case sensitive.
    /// </summary>
    [Test]
    public void Generate_Hash_Multiple_Strings_Case_Sensitive()
    {
        var hash1 = Generate(true, "hello", "world");
        var hash2 = Generate(true, "hello", "world");
        var hashFalse1 = Generate(true, "hello", "worlD");
        var hashFalse2 = Generate(true, "hEllo", "world");

        Assert.AreEqual(hash1, hash2);
        Assert.AreNotEqual(hash1, hashFalse1);
        Assert.AreNotEqual(hash1, hashFalse2);
    }

    /// <summary>
    /// Tests that the hash generation is case insensitive when generating hashes from multiple strings.
    /// </summary>
    [Test]
    public void Generate_Hash_Multiple_Strings_Case_Insensitive()
    {
        var hash1 = Generate(false, "hello", "world");
        var hash2 = Generate(false, "hello", "world");
        var hashFalse1 = Generate(false, "hello", "worlD");
        var hashFalse2 = Generate(false, "hEllo", "world");

        Assert.AreEqual(hash1, hash2);
        Assert.AreEqual(hash1, hashFalse1);
        Assert.AreEqual(hash1, hashFalse2);
    }

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

    /// <summary>
    /// Tests that the hash combiner generates the same hash for case-insensitive strings and different hashes for different strings.
    /// </summary>
    [Test]
    public void HashCombiner_Test_String()
    {
        using (var combiner1 = new HashGenerator())
        using (var combiner2 = new HashGenerator())
        {
            combiner1.AddCaseInsensitiveString("Hello");
            combiner2.AddCaseInsensitiveString("hello");
            Assert.AreEqual(combiner1.GenerateHash(), combiner2.GenerateHash());
            combiner2.AddCaseInsensitiveString("world");
            Assert.AreNotEqual(combiner1.GenerateHash(), combiner2.GenerateHash());
        }
    }

    /// <summary>
    /// Tests that the hash combiner generates consistent hashes for the same integer input and different hashes for different inputs.
    /// </summary>
    [Test]
    public void HashCombiner_Test_Int()
    {
        using (var combiner1 = new HashGenerator())
        using (var combiner2 = new HashGenerator())
        {
            combiner1.AddInt(1234);
            combiner2.AddInt(1234);
            Assert.AreEqual(combiner1.GenerateHash(), combiner2.GenerateHash());
            combiner2.AddInt(1);
            Assert.AreNotEqual(combiner1.GenerateHash(), combiner2.GenerateHash());
        }
    }

    /// <summary>
    /// Tests that the HashGenerator correctly combines DateTime values to produce consistent hashes.
    /// Verifies that identical DateTime inputs produce the same hash and different DateTime inputs produce different hashes.
    /// </summary>
    [Test]
    public void HashCombiner_Test_DateTime()
    {
        using var combiner1 = new HashGenerator();
        using var combiner2 = new HashGenerator();
        var dt = DateTime.Now;
        combiner1.AddDateTime(dt);
        combiner2.AddDateTime(dt);
        Assert.AreEqual(combiner1.GenerateHash(), combiner2.GenerateHash());
        combiner2.AddDateTime(DateTime.Now);
        Assert.AreNotEqual(combiner1.GenerateHash(), combiner2.GenerateHash());
    }

    /// <summary>
    /// Tests the HashGenerator's ability to combine file hashes and verify that
    /// identical files produce the same hash while different files produce different hashes.
    /// </summary>
    [Test]
    public void HashCombiner_Test_File()
    {
        using var combiner1 = new HashGenerator();
        using var combiner2 = new HashGenerator();
        using var combiner3 = new HashGenerator();
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

        combiner1.AddFile(new FileInfo(file1Path));

        combiner2.AddFile(new FileInfo(file1Path));

        combiner3.AddFile(new FileInfo(file2Path));

        Assert.AreEqual(combiner1.GenerateHash(), combiner2.GenerateHash());
        Assert.AreNotEqual(combiner1.GenerateHash(), combiner3.GenerateHash());

        combiner2.AddFile(new FileInfo(file2Path));

        Assert.AreNotEqual(combiner1.GenerateHash(), combiner2.GenerateHash());
    }

    /// <summary>
    /// Tests the HashGenerator's ability to combine hashes for a folder.
    /// It verifies that adding the same folder twice produces the same hash,
    /// and that adding a folder after modifying its contents produces a different hash.
    /// </summary>
    [Test]
    public void HashCombiner_Test_Folder()
    {
        using var combiner1 = new HashGenerator();
        using var combiner2 = new HashGenerator();
        using var combiner3 = new HashGenerator();
        var dir = PrepareFolder();
        var file1Path = Path.Combine(dir.FullName, "hastest1.txt");
        File.Delete(file1Path);
        using (var file1 = File.CreateText(Path.Combine(dir.FullName, "hastest1.txt")))
        {
            file1.WriteLine("hello");
        }

        // first test the whole folder
        combiner1.AddFolder(dir);

        combiner2.AddFolder(dir);

        Assert.AreEqual(combiner1.GenerateHash(), combiner2.GenerateHash());

        // now add a file to the folder
        var file2Path = Path.Combine(dir.FullName, "hastest2.txt");
        File.Delete(file2Path);
        using (var file2 = File.CreateText(Path.Combine(dir.FullName, "hastest2.txt")))
        {
            // even though files are the same, the dates are different
            file2.WriteLine("hello");
        }

        combiner3.AddFolder(dir);

        Assert.AreNotEqual(combiner1.GenerateHash(), combiner3.GenerateHash());
    }
}
