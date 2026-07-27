// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
internal sealed class SimilarNodeNameTests
{
    [Test]
    public void Name_Is_Suffixed()
    {
        SimilarNodeName[] names = [new() { Id = 1, Name = "Zulu" }];

        var res = SimilarNodeName.GetUniqueName(names, 0, "Zulu");
        Assert.AreEqual("Zulu (1)", res);
    }

    [Test]
    public void Suffixed_Name_Is_Incremented()
    {
        SimilarNodeName[] names =
        [
            new() { Id = 1, Name = "Zulu" },
            new() { Id = 2, Name = "Kilo (1)" },
            new() { Id = 3, Name = "Kilo" },
        ];

        var res = SimilarNodeName.GetUniqueName(names, 0, "Kilo (1)");
        Assert.AreEqual("Kilo (2)", res);
    }

    [Test]
    public void Lower_Number_Suffix_Is_Inserted()
    {
        SimilarNodeName[] names =
        [
            new() { Id = 1, Name = "Golf" },
            new() { Id = 2, Name = "Golf (2)" },
        ];

        var res = SimilarNodeName.GetUniqueName(names, 0, "Golf");
        Assert.AreEqual("Golf (1)", res);
    }

    [Test]
    [TestCase(0, "Alpha", "Alpha (3)")]
    [TestCase(0, "alpha", "alpha (3)")]
    public void Case_Is_Ignored(int nodeId, string nodeName, string expected)
    {
        SimilarNodeName[] names =
        [
            new() { Id = 1, Name = "Alpha" },
            new() { Id = 2, Name = "Alpha (1)" },
            new() { Id = 3, Name = "Alpha (2)" },
        ];
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
        var names = Array.Empty<SimilarNodeName>();

        var res = SimilarNodeName.GetUniqueName(names, nodeId, nodeName);

        Assert.AreEqual(expected, res);
    }

    [Test]
    public void Matching_NoedId_Causes_No_Change()
    {
        SimilarNodeName[] names =
        [
            new() { Id = 1, Name = "Kilo (1)" },
            new() { Id = 2, Name = "Yankee" },
            new() { Id = 3, Name = "Kilo" },
        ];

        var res = SimilarNodeName.GetUniqueName(names, 1, "Kilo (1)");

        Assert.AreEqual("Kilo (1)", res);
    }

    [Test]
    public void Extra_MultiSuffixed_Name_Is_Ignored()
    {
        // Sequence is: Test, Test (1), Test (2)
        // Ignore: Test (1) (1)
        SimilarNodeName[] names =
        [
            new() { Id = 1, Name = "Alpha (2)" },
            new() { Id = 2, Name = "Test" },
            new() { Id = 3, Name = "Test (1)" },
            new() { Id = 4, Name = "Test (2)" },
            new() { Id = 5, Name = "Test (1) (1)" },
        ];

        var res = SimilarNodeName.GetUniqueName(names, 0, "Test");

        Assert.AreEqual("Test (3)", res);
    }

    [Test]
    public void Matched_Name_Is_Suffixed()
    {
        SimilarNodeName[] names = [new() { Id = 1, Name = "Test" }];

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
        [
            new() { Id = 1, Name = "Test (1)" }, new() { Id = 2, Name = "Test (1) (1)" }
        ];

        var res = SimilarNodeName.GetUniqueName(names, 0, "Test (1) (1)");

        Assert.AreEqual("Test (1) (2)", res);
    }

    [Test]
    public void Suffixed_Name_Causes_Secondary_Suffix()
    {
        SimilarNodeName[] names = [new() { Id = 6, Name = "Alpha (1)" }];
        var res = SimilarNodeName.GetUniqueName(names, 0, "Alpha (1)");

        Assert.AreEqual("Alpha (1) (1)", res);
    }

    [TestCase("Test (0)", "Test (0) (1)")]
    [TestCase("Test (-1)", "Test (-1) (1)")]
    [TestCase("Test (1) (-1)", "Test (1) (-1) (1)")]
    public void NonPositive_Suffix_Is_Ignored(string suffix, string expected)
    {
        SimilarNodeName[] names = [new() { Id = 6, Name = suffix }];
        var res = SimilarNodeName.GetUniqueName(names, 0, suffix);

        Assert.AreEqual(expected, res);
    }

    [Test]
    public void Handles_Many_Similar_Names()
    {
        SimilarNodeName[] names =
        [
            new() { Id = 1, Name = "Alpha (2)" },
            new() { Id = 2, Name = "Test" },
            new() { Id = 3, Name = "Test (1)" },
            new() { Id = 4, Name = "Test (2)" },
            new() { Id = 22, Name = "Test (1) (1)" },
        ];

        var uniqueName = SimilarNodeName.GetUniqueName(names, 0, "Test");
        Assert.AreEqual("Test (3)", uniqueName);
    }

    [Test]
    public void Missing_Suffix_In_Sequence_Is_Filled()
    {
        // Suffixes 1 and 3 exist, so the gap at 2 should be used rather than appending 4.
        SimilarNodeName[] names =
        [
            new() { Id = 1, Name = "Delta" },
            new() { Id = 2, Name = "Delta (1)" },
            new() { Id = 3, Name = "Delta (3)" },
        ];

        var res = SimilarNodeName.GetUniqueName(names, 0, "Delta");

        Assert.AreEqual("Delta (2)", res);
    }

    [Test]
    public void Name_With_Longer_Text_Does_Not_Cause_A_Suffix()
    {
        // "Testing" and "Test Case" share a prefix with "Test" but are different names,
        // so "Test" itself is still available and must be returned unchanged.
        SimilarNodeName[] names =
        [
            new() { Id = 1, Name = "Testing" },
            new() { Id = 2, Name = "Test Case" },
        ];

        var res = SimilarNodeName.GetUniqueName(names, 0, "Test");

        Assert.AreEqual("Test", res);
    }

    [Test]
    public void Free_Name_Is_Used_Even_When_Suffixed_Names_Exist()
    {
        // The plain "Foxtrot" is free, so it must be returned as-is rather than jumping to a suffix
        // just because numbered variants happen to exist.
        SimilarNodeName[] names =
        [
            new() { Id = 1, Name = "Foxtrot (1)" },
            new() { Id = 2, Name = "Foxtrot (2)" },
        ];

        var res = SimilarNodeName.GetUniqueName(names, 0, "Foxtrot");

        Assert.AreEqual("Foxtrot", res);
    }

    [Test]
    public void Empty_Name_With_Suffixed_Siblings_Is_Numbered()
    {
        // A sibling beginning with a space (" (1)") parses to an empty base name with suffix 1, so
        // resolving an empty name against it must continue the numbering.
        SimilarNodeName[] names = [new() { Id = 1, Name = " (1)" }];

        var res = SimilarNodeName.GetUniqueName(names, 0, string.Empty);

        Assert.AreEqual(" (2)", res);
    }

    [Test]
    public void Empty_Name_With_No_Suffixed_Siblings_Is_Empty()
    {
        // Empty name, and the only sibling (though it starts with a space) carries no suffix, so
        // there is nothing to number against and the empty name is returned.
        SimilarNodeName[] names = [new() { Id = 1, Name = " leading-space" }];

        var res = SimilarNodeName.GetUniqueName(names, 0, string.Empty);

        Assert.AreEqual(string.Empty, res);
    }

    [TestCase("Echo", "Echo")]
    [TestCase("Echo (2)", "Echo")]
    [TestCase("Echo (1) (2)", "Echo (1)")]
    [TestCase("Echo (0)", "Echo (0)")]
    [TestCase("50%", "50%")]
    public void GetBaseText_Removes_Trailing_Suffix(string name, string expected)
        => Assert.AreEqual(expected, SimilarNodeName.GetBaseText(name));

    [TestCase("")]
    [TestCase(null)]
    [TestCase("   ")]
    public void GetBaseText_Returns_Space_For_Empty_Name(string? name) =>

        // An empty name has no base text; it is represented as a single space so that a unique
        // name can still be produced (e.g. " (1)").
        Assert.AreEqual(" ", SimilarNodeName.GetBaseText(name));
}
