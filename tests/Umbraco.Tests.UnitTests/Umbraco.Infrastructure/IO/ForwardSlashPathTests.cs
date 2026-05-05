// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.IO;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.IO;

[TestFixture]
public class ForwardSlashPathTests
{
    [Test]
    public void Split_File_At_Root_Returns_Name_And_Null_Parent()
    {
        (var name, var parentPath) = ForwardSlashPath.Split("Bar.css");

        Assert.Multiple(() =>
        {
            Assert.That(name, Is.EqualTo("Bar.css"));
            Assert.That(parentPath, Is.Null);
        });
    }

    [Test]
    public void Split_File_With_Single_Parent_Returns_Name_And_Parent()
    {
        (var name, var parentPath) = ForwardSlashPath.Split("Foo/Bar.css");

        Assert.Multiple(() =>
        {
            Assert.That(name, Is.EqualTo("Bar.css"));
            Assert.That(parentPath, Is.EqualTo("Foo"));
        });
    }

    [Test]
    public void Split_File_With_Nested_Parent_Returns_Name_And_Full_Parent_Path()
    {
        (var name, var parentPath) = ForwardSlashPath.Split("Foo/Bar/Baz.css");

        Assert.Multiple(() =>
        {
            Assert.That(name, Is.EqualTo("Baz.css"));
            Assert.That(parentPath, Is.EqualTo("Foo/Bar"));
        });
    }

    [Test]
    public void Split_File_With_Leading_Slash_Returns_Name_And_Null_Parent()
    {
        (var name, var parentPath) = ForwardSlashPath.Split("/Bar.css");

        Assert.Multiple(() =>
        {
            Assert.That(name, Is.EqualTo("Bar.css"));
            Assert.That(parentPath, Is.Null);
        });
    }

    [Test]
    public void Split_Preserves_Forward_Slash_Regardless_Of_Host_Platform()
    {
        // Verifies the implementation does not rely on Path.GetDirectoryName / Path.GetFileName,
        // which on Windows normalise '/' to '\' and would break round-tripping through the
        // file-system services that store and look up files using the '/'-convention.
        (var name, var parentPath) = ForwardSlashPath.Split("Foo/Bar/Baz.css");

        Assert.Multiple(() =>
        {
            Assert.That(parentPath, Does.Not.Contain("\\"));
            Assert.That(parentPath, Is.EqualTo("Foo/Bar"));
            Assert.That(name, Is.EqualTo("Baz.css"));
        });
    }

    [TestCase("Foo/Bar.css", "Bar.css", "Foo")]
    [TestCase("Foo/Bar/Baz.css", "Baz.css", "Foo/Bar")]
    [TestCase("Foo/Bar/Baz/Qux.css", "Qux.css", "Foo/Bar/Baz")]
    [TestCase("Bar.css", "Bar.css", null)]
    [TestCase("/Bar.css", "Bar.css", null)]
    [TestCase("/Foo/Bar.css", "Bar.css", "/Foo")]
    public void Split_TestCases(string input, string expectedName, string? expectedParentPath)
    {
        (var name, var parentPath) = ForwardSlashPath.Split(input);

        Assert.Multiple(() =>
        {
            Assert.That(name, Is.EqualTo(expectedName));
            Assert.That(parentPath, Is.EqualTo(expectedParentPath));
        });
    }
}
