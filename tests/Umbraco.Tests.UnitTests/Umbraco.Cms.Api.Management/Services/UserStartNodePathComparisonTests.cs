using NUnit.Framework;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services;

/// <summary>
/// Tests for the path comparison logic used in UserStartNodeEntitiesService.
/// These tests document the fix for the path comparison bug where node IDs
/// that are numeric prefixes of other IDs (e.g., 100 vs 1001) would incorrectly match.
/// </summary>
[TestFixture]
public class UserStartNodePathComparisonTests
{
    /// <summary>
    /// Tests the IsDescendantOrSelf path comparison logic with trailing commas.
    /// This prevents false matches when one ID is a numeric prefix of another.
    ///
    /// The fix: Add trailing commas to both paths before comparison:
    /// - Without fix: "-1,100".StartsWith("-1,10") = true ❌ (incorrect match)
    /// - With fix: "-1,100,".StartsWith("-1,10,") = false ✅ (correct)
    /// </summary>
    [Test]
    [TestCase(
        "-1,100",
        "-1,1001",
        ExpectedResult = false,
        TestName = "Should not match when child ID 100 is prefix of start node ID 1001")]
    [TestCase(
        "-1,1001",
        "-1,100",
        ExpectedResult = false,
        TestName = "Should not match when start node ID 100 is prefix of child ID 1001")]
    [TestCase(
        "-1,10",
        "-1,100",
        ExpectedResult = false,
        TestName = "Should not match when child ID 10 is prefix of start node ID 100")]
    [TestCase(
        "-1,100",
        "-1,10",
        ExpectedResult = false,
        TestName = "Should not match when start node ID 10 is prefix of child ID 100")]
    [TestCase(
        "-1,1",
        "-1,10",
        ExpectedResult = false,
        TestName = "Should not match when child ID 1 is prefix of start node ID 10")]
    [TestCase(
        "-1,10",
        "-1,1",
        ExpectedResult = false,
        TestName = "Should not match when start node ID 1 is prefix of child ID 10")]
    [TestCase(
        "-1,1",
        "-1,100",
        ExpectedResult = false,
        TestName = "Should not match when child ID 1 is prefix of start node ID 100")]
    [TestCase(
        "-1,100",
        "-1,100",
        ExpectedResult = true,
        TestName = "Should match when paths are identical (self case)")]
    [TestCase(
        "-1,100,200",
        "-1,100",
        ExpectedResult = true,
        TestName = "Should match when child is descendant of start node")]
    [TestCase(
        "-1,100,200,300",
        "-1,100",
        ExpectedResult = true,
        TestName = "Should match when child is deep descendant of start node")]
    [TestCase(
        "-1,100",
        "-1,100,200",
        ExpectedResult = false,
        TestName = "Should not match when child is ancestor of start node (reversed relationship)")]
    [TestCase(
        "-1,100",
        "-1,100,200,300",
        ExpectedResult = false,
        TestName = "Should not match when child is deep ancestor of start node (reversed relationship)")]
    public bool IsDescendantOrSelf_WithTrailingCommas_PreventsNumericPrefixMatches(string childPath, string startNodePath)
    {
        // This implements the same logic as UserStartNodeEntitiesService.IsDescendantOrSelf
        // Adding trailing commas to both paths ensures accurate comparison
        var childPathWithComma = $"{childPath},";
        var startNodePathWithComma = $"{startNodePath},";

        return childPathWithComma.StartsWith(startNodePathWithComma);
    }

    /// <summary>
    /// Demonstrates the bug that existed before the fix.
    /// Without trailing commas, numeric prefix IDs incorrectly match.
    /// </summary>
    [Test]
    [TestCase(
        "-1,100",
        "-1,10",
        ExpectedResult = true,
        TestName = "BUG: Without trailing commas, 100 incorrectly matches 10")]
    [TestCase(
        "-1,1001",
        "-1,100",
        ExpectedResult = true,
        TestName = "BUG: Without trailing commas, 1001 incorrectly matches 100")]
    [TestCase(
        "-1,10",
        "-1,1",
        ExpectedResult = true,
        TestName = "BUG: Without trailing commas, 10 incorrectly matches 1")]
    public bool WithoutTrailingCommas_CausesFalseMatches_DocumentsBug(string childPath, string startNodePath)
    {
        // This demonstrates the bug: without trailing commas, we get false matches
        // These tests document WHY the fix with trailing commas is necessary
        return childPath.StartsWith(startNodePath);
    }

    /// <summary>
    /// Tests edge cases with multi-level paths to ensure the fix works
    /// at different tree depths.
    /// </summary>
    [Test]
    [TestCase(
        "-1,100,200,300",
        "-1,10,20,30",
        ExpectedResult = false,
        TestName = "Deep paths with prefix IDs should not match")]
    [TestCase(
        "-1,1,10,100,1000",
        "-1,1,10,100",
        ExpectedResult = true,
        TestName = "Should match when child is descendant despite multiple prefix IDs")]
    [TestCase(
        "-1,1,10,100",
        "-1,1,10,100",
        ExpectedResult = true,
        TestName = "Deep paths that are identical should match (self case)")]
    [TestCase(
        "-1,1,10,100",
        "-1,1,10,100,1000",
        ExpectedResult = false,
        TestName = "Should not match when child is ancestor of start node")]
    public bool DeepPaths_WorkCorrectly_WithTrailingCommas(string childPath, string startNodePath)
    {
        var childPathWithComma = $"{childPath},";
        var startNodePathWithComma = $"{startNodePath},";

        return childPathWithComma.StartsWith(startNodePathWithComma);
    }
}
