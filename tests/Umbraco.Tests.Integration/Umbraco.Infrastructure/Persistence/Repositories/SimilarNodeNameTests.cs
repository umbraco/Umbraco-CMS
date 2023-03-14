// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
public class SimilarNodeNameTests
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

    /* Original Tests - Can be deleted, as new tests cover all cases */

    [TestCase(0, "Charlie", "Charlie")]
    [TestCase(0, "Zulu", "Zulu (1)")]
    [TestCase(0, "Golf", "Golf (1)")]
    [TestCase(0, "Kilo", "Kilo (2)")]
    [TestCase(0, "Alpha", "Alpha (3)")]
    //// [TestCase(0, "Kilo (1)", "Kilo (1) (1)")] // though... we might consider "Kilo (2)"
    [TestCase(0, "Kilo (1)", "Kilo (2)")] // names[] contains "Kilo" AND "Kilo (1)", which implies that result should be "Kilo (2)"
    [TestCase(6, "Kilo (1)", "Kilo (1)")] // because of the id
    [TestCase(0, "alpha", "alpha (3)")]
    [TestCase(0, "", " (1)")]
    [TestCase(0, null, " (1)")]
    public void Test(int nodeId, string nodeName, string expected)
    {
        SimilarNodeName[] names =
        {
            new SimilarNodeName {Id = 1, Name = "Alpha (2)"}, new SimilarNodeName {Id = 2, Name = "Alpha"},
            new SimilarNodeName {Id = 3, Name = "Golf"}, new SimilarNodeName {Id = 4, Name = "Zulu"},
            new SimilarNodeName {Id = 5, Name = "Mike"}, new SimilarNodeName {Id = 6, Name = "Kilo (1)"},
            new SimilarNodeName {Id = 7, Name = "Yankee"}, new SimilarNodeName {Id = 8, Name = "Kilo"},
            new SimilarNodeName {Id = 9, Name = "Golf (2)"}, new SimilarNodeName {Id = 10, Name = "Alpha (1)"}
        };

        Assert.AreEqual(expected, SimilarNodeName.GetUniqueName(names, nodeId, nodeName));
    }

    [Test]
    [Explicit("This test fails! We need to fix up the logic")]
    public void TestMany()
    {
        SimilarNodeName[] names =
        {
            new SimilarNodeName {Id = 1, Name = "Alpha (2)"}, new SimilarNodeName {Id = 2, Name = "Test"},
            new SimilarNodeName {Id = 3, Name = "Test (1)"}, new SimilarNodeName {Id = 4, Name = "Test (2)"},
            new SimilarNodeName {Id = 22, Name = "Test (1) (1)"}
        };

        // fixme - this will yield "Test (2)" which is already in use
        Assert.AreEqual("Test (3)", SimilarNodeName.GetUniqueName(names, 0, "Test"));
    }
}
