// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
internal sealed class SimilarNodeNameTests
{
    /// <summary>
    /// Tests that the name is correctly suffixed when a duplicate name exists.
    /// </summary>
    public void Name_Is_Suffixed()
    {
        SimilarNodeName[] names = { new SimilarNodeName { Id = 1, Name = "Zulu" } };

        var res = SimilarNodeName.GetUniqueName(names, 0, "Zulu");
        Assert.AreEqual("Zulu (1)", res);
    }

    /// <summary>
    /// Tests that suffixed names are incremented correctly to ensure uniqueness.
    /// </summary>
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

    /// <summary>
    /// Tests that a lower number suffix is correctly inserted when generating a unique name.
    /// </summary>
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

    /// <summary>
    /// Tests that when an empty list of similar node names is provided,
    /// the original name remains unchanged.
    /// </summary>
    [Test]
    public void Empty_List_Causes_Unchanged_Name()
    {
        var names = new SimilarNodeName[] { };

        var res = SimilarNodeName.GetUniqueName(names, 0, "Charlie");

        Assert.AreEqual("Charlie", res);
    }

    /// <summary>
    /// Tests that an empty or null node name is correctly suffixed to ensure uniqueness.
    /// </summary>
    /// <param name="nodeId">The ID of the node.</param>
    /// <param name="nodeName">The name of the node, which may be empty or null.</param>
    /// <param name="expected">The expected unique name result after suffixing.</param>
    [Test]
    [TestCase(0, "", " (1)")]
    [TestCase(0, null, " (1)")]
    public void Empty_Name_Is_Suffixed(int nodeId, string nodeName, string expected)
    {
        var names = new SimilarNodeName[] { };

        var res = SimilarNodeName.GetUniqueName(names, nodeId, nodeName);

        Assert.AreEqual(expected, res);
    }

    /// <summary>
    /// Tests that when the matching node ID is provided, the name remains unchanged.
    /// </summary>
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

    /// <summary>
    /// Tests that extra multi-suffixed names are ignored when generating a unique name.
    /// </summary>
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

    /// <summary>
    /// Tests that a matched name is correctly suffixed to ensure uniqueness.
    /// </summary>
    [Test]
    public void Matched_Name_Is_Suffixed()
    {
        SimilarNodeName[] names = { new SimilarNodeName { Id = 1, Name = "Test" } };

        var res = SimilarNodeName.GetUniqueName(names, 0, "Test");

        Assert.AreEqual("Test (1)", res);
    }

    /// <summary>
    /// Tests that a multi-suffixed name is correctly incremented to ensure uniqueness.
    /// </summary>
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

    /// <summary>
    /// Tests that a suffixed name causes a secondary suffix to be appended to ensure uniqueness.
    /// </summary>
    [Test]
    public void Suffixed_Name_Causes_Secondary_Suffix()
    {
        SimilarNodeName[] names = { new SimilarNodeName { Id = 6, Name = "Alpha (1)" } };
        var res = SimilarNodeName.GetUniqueName(names, 0, "Alpha (1)");

        Assert.AreEqual("Alpha (1) (1)", res);
    }

    /// <summary>
    /// Verifies that when generating a unique node name, any suffix containing a non-positive number (such as 0 or negative values) is ignored for uniqueness purposes, and a new positive numeric suffix is appended instead.
    /// </summary>
    /// <param name="suffix">The input node name containing a non-positive numeric suffix.</param>
    /// <param name="expected">The expected unique node name result after processing.</param>
    [TestCase("Test (0)", "Test (0) (1)")]
    [TestCase("Test (-1)", "Test (-1) (1)")]
    [TestCase("Test (1) (-1)", "Test (1) (-1) (1)")]
    public void NonPositive_Suffix_Is_Ignored(string suffix, string expected)
    {
        SimilarNodeName[] names = { new SimilarNodeName { Id = 6, Name = suffix } };
        var res = SimilarNodeName.GetUniqueName(names, 0, suffix);

        Assert.AreEqual(expected, res);
    }

    /// <summary>
    /// Tests that the method correctly handles many similar node names and generates a unique name.
    /// </summary>
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
}
