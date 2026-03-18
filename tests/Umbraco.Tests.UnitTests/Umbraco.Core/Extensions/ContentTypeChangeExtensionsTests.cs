// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

/// <summary>
/// Contains unit tests for the <see cref="ContentTypeChangeExtensions"/> extension methods related to content type changes.
/// </summary>
[TestFixture]
public class ContentTypeChangeExtensionsTests
{
    /// <summary>
    /// Unit test for <see cref="ContentTypeChangeExtensions.IsStructuralChange"/>.
    /// Verifies whether the specified <paramref name="change"/> value is correctly identified as a structural change.
    /// </summary>
    /// <param name="change">The <see cref="ContentTypeChangeTypes"/> value to test.</param>
    /// <param name="expected">True if the change is expected to be structural; otherwise, false.</param>
    [TestCase(ContentTypeChangeTypes.RefreshMain, true)]
    [TestCase(ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther, true)]
    [TestCase(ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.Create, true)]
    [TestCase(ContentTypeChangeTypes.RefreshOther, false)]
    [TestCase(ContentTypeChangeTypes.None, false)]
    [TestCase(ContentTypeChangeTypes.Create, false)]
    [TestCase(ContentTypeChangeTypes.Remove, false)]
    [TestCase(ContentTypeChangeTypes.RefreshOther | ContentTypeChangeTypes.Remove, false)]
    public void IsStructuralChange(ContentTypeChangeTypes change, bool expected) =>
        Assert.AreEqual(expected, change.IsStructuralChange());

    /// <summary>
    /// Verifies that the <see cref="ContentTypeChangeExtensions.IsNonStructuralChange"/> extension method correctly identifies whether the specified <paramref name="change"/> value represents a non-structural content type change.
    /// </summary>
    /// <param name="change">The <see cref="ContentTypeChangeTypes"/> flag(s) to evaluate.</param>
    /// <param name="expected">True if the change is expected to be non-structural; otherwise, false.</param>
    /// <remarks>
    /// A non-structural change is one that does not affect the structure of the content type, such as certain refresh operations.
    /// This test asserts that the extension method returns the expected result for various combinations of change types.
    /// </remarks>
    [TestCase(ContentTypeChangeTypes.RefreshOther, true)]
    [TestCase(ContentTypeChangeTypes.RefreshOther | ContentTypeChangeTypes.Create, true)]
    [TestCase(ContentTypeChangeTypes.RefreshOther | ContentTypeChangeTypes.Remove, true)]
    [TestCase(ContentTypeChangeTypes.RefreshMain, false)]
    [TestCase(ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther, false)]
    [TestCase(ContentTypeChangeTypes.None, false)]
    [TestCase(ContentTypeChangeTypes.Create, false)]
    [TestCase(ContentTypeChangeTypes.Remove, false)]
    public void IsNonStructuralChange(ContentTypeChangeTypes change, bool expected) =>
        Assert.AreEqual(expected, change.IsNonStructuralChange());
}
