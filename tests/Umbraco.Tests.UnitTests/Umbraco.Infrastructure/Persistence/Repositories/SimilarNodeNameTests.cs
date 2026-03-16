// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
internal sealed class SimilarNodeNameTests
{
    public void Name_Is_Suffixed()
    {
        SimilarNodeName[] names = { new SimilarNodeName { Id = 1, Name = "Zulu" } };

        var res = SimilarNodeName.GetUniqueName(names, 0, "Zulu");
        Assert.AreEqual("Zulu (1)", res);
    }

    [Test]
    public void Suffixed_Name_Is_Incremented()
    {
        SimilarNodeName[] names =
        {
            new SimilarNodeName {Id = 1, Name = "Zulu"}, new SimilarNodeName {Id = 2, Name = "Kilo (1)"},
            new SimilarNodeName {Id = 3, Name = "Kilo"}
        };

        var res = SimilarNodeName.GetUniqueName(names, 0, "Kilo (1)");
        Assert.AreEqual("Kilo (2)", res);
    }

    [Test]
    public void Lower_Number_Suffix_Is_Inserted()
    {
        SimilarNodeName[] names =
        {
            new SimilarNodeName {Id = 1, Name = "Golf"}, new SimilarNodeName {Id = 2, Name = "Golf (2)"}
        };

        var res = SimilarNodeName.GetUniqueName(names, 0, "Golf");
        Assert.AreEqual("Golf (1)", res);
    }

    [Test]
    [TestCase(0, "Alpha", "Alpha (3)")]
    [TestCase(0, "alpha", "alpha (3)")]
    public void Case_Is_Ignored(int nodeId, string nodeName, string expected)
    {
        SimilarNodeName[] names =
        {
            new SimilarNodeName {Id = 1, Name = "Alpha"}, new SimilarNodeName {Id = 2, Name = "Alpha (1)"},
            new SimilarNodeName {Id = 3, Name = "Alpha (2)"}
        };
        var res = SimilarNodeName.GetUniqueName(names, nodeId, nodeName);

        Assert.AreEqual(expected, res);
    }

    [Test]
    public void Empty_List_Causes_Unchanged_Name()
    {
        var names = new SimilarNodeName[] { };

        var res = SimilarNodeName.GetUniqueName(names, 0, "Charlie");

        Assert.AreEqual("Charlie", res);
    }

    [Test]
    [TestCase(0, "", " (1)")]
    [TestCase(0, null, " (1)")]
    public void Empty_Name_Is_Suffixed(int nodeId, string nodeName, string expected)
    {
        var names = new SimilarNodeName[] { };

        var res = SimilarNodeName.GetUniqueName(names, nodeId, nodeName);

        Assert.AreEqual(expected, res);
    }

    [Test]
    public void Matching_NoedId_Causes_No_Change()
    {
        SimilarNodeName[] names =
        {
            new SimilarNodeName {Id = 1, Name = "Kilo (1)"}, new SimilarNodeName {Id = 2, Name = "Yankee"},
            new SimilarNodeName {Id = 3, Name = "Kilo"}
        };

        var res = SimilarNodeName.GetUniqueName(names, 1, "Kilo (1)");

        Assert.AreEqual("Kilo (1)", res);
    }

    [Test]
    public void Extra_MultiSuffixed_Name_Is_Ignored()
    {
        // Sequesnce is: Test, Test (1), Test (2)
        // Ignore: Test (1) (1)
        SimilarNodeName[] names =
        {
            new SimilarNodeName {Id = 1, Name = "Alpha (2)"}, new SimilarNodeName {Id = 2, Name = "Test"},
            new SimilarNodeName {Id = 3, Name = "Test (1)"}, new SimilarNodeName {Id = 4, Name = "Test (2)"},
            new SimilarNodeName {Id = 5, Name = "Test (1) (1)"}
        };

        var res = SimilarNodeName.GetUniqueName(names, 0, "Test");

        Assert.AreEqual("Test (3)", res);
    }

    [Test]
    public void Matched_Name_Is_Suffixed()
    {
        SimilarNodeName[] names = { new SimilarNodeName { Id = 1, Name = "Test" } };

        var res = SimilarNodeName.GetUniqueName(names, 0, "Test");

        Assert.AreEqual("Test (1)", res);
    }

    [Test]
    public void MultiSuffixed_Name_Is_Icremented()
    {
        // "Test (1)" is treated as the "original" version of the name.
        // "Test (1) (1)" is the suffixed result of a copy, and therefore is incremented
        // Hence this test result should be the same as Suffixed_Name_Is_Incremented
        SimilarNodeName[] names =
        {
            new SimilarNodeName {Id = 1, Name = "Test (1)"}, new SimilarNodeName {Id = 2, Name = "Test (1) (1)"}
        };

        var res = SimilarNodeName.GetUniqueName(names, 0, "Test (1) (1)");

        Assert.AreEqual("Test (1) (2)", res);
    }

    [Test]
    public void Suffixed_Name_Causes_Secondary_Suffix()
    {
        SimilarNodeName[] names = { new SimilarNodeName { Id = 6, Name = "Alpha (1)" } };
        var res = SimilarNodeName.GetUniqueName(names, 0, "Alpha (1)");

        Assert.AreEqual("Alpha (1) (1)", res);
    }

    [TestCase("Test (0)", "Test (0) (1)")]
    [TestCase("Test (-1)", "Test (-1) (1)")]
    [TestCase("Test (1) (-1)", "Test (1) (-1) (1)")]
    public void NonPositive_Suffix_Is_Ignored(string suffix, string expected)
    {
        SimilarNodeName[] names = { new SimilarNodeName { Id = 6, Name = suffix } };
        var res = SimilarNodeName.GetUniqueName(names, 0, suffix);

        Assert.AreEqual(expected, res);
    }

    [Test]
    public void Handles_Many_Similar_Names()
    {
        SimilarNodeName[] names =
        {
            new SimilarNodeName {Id = 1, Name = "Alpha (2)"},
            new SimilarNodeName {Id = 2, Name = "Test"},
            new SimilarNodeName {Id = 3, Name = "Test (1)"},
            new SimilarNodeName {Id = 4, Name = "Test (2)"},
            new SimilarNodeName {Id = 22, Name = "Test (1) (1)"}
        };

        var uniqueName = SimilarNodeName.GetUniqueName(names, 0, "Test");
        Assert.AreEqual("Test (3)", uniqueName);
    }

    /// <summary>
    /// Verifies that the phantom entry approach used by DocumentRepository to resolve URL
    /// segment collisions works correctly. When two different names produce the same URL
    /// segment (e.g. "Title" and "Title." both produce "title"), a phantom entry with the
    /// proposed name is added so the algorithm treats them as duplicates.
    /// </summary>
    [Test]
    public void Phantom_Entry_For_Url_Segment_Collision_Causes_Suffix()
    {
        // Sibling "Title" exists. We're saving "Title." which produces the same URL segment.
        // A phantom entry "Title." is added (with the sibling's ID) to simulate the collision.
        SimilarNodeName[] names =
        {
            new SimilarNodeName { Id = 1, Name = "Title" },
            new SimilarNodeName { Id = 1, Name = "Title." }, // phantom entry
        };

        var res = SimilarNodeName.GetUniqueName(names, 0, "Title.");

        Assert.AreEqual("Title. (1)", res);
    }

    [Test]
    public void Phantom_Entry_With_Multiple_Collisions_Increments_Suffix()
    {
        // Two existing siblings "Title" and "Title!" both produce the same URL segment as "Title.".
        // Two phantom entries are added.
        SimilarNodeName[] names =
        {
            new SimilarNodeName { Id = 1, Name = "Title" },
            new SimilarNodeName { Id = 2, Name = "Title!" },
            new SimilarNodeName { Id = 1, Name = "Title." }, // phantom for sibling 1
            new SimilarNodeName { Id = 2, Name = "Title." }, // phantom for sibling 2
        };

        var res = SimilarNodeName.GetUniqueName(names, 0, "Title.");

        Assert.AreEqual("Title. (1)", res);
    }

    [Test]
    public void Phantom_Entry_With_Existing_Suffix_Increments_Correctly()
    {
        // Sibling "Title" exists and "Title. (1)" already exists.
        // We're saving a new "Title." → should get "Title. (2)".
        SimilarNodeName[] names =
        {
            new SimilarNodeName { Id = 1, Name = "Title" },
            new SimilarNodeName { Id = 2, Name = "Title. (1)" },
            new SimilarNodeName { Id = 1, Name = "Title." }, // phantom entry
        };

        var res = SimilarNodeName.GetUniqueName(names, 0, "Title.");

        Assert.AreEqual("Title. (2)", res);
    }
}
