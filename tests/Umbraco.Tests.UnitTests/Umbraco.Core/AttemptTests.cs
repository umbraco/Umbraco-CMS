// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core;

/// <summary>
/// Contains unit tests for the <see cref="Attempt"/> class in the Umbraco Core.
/// </summary>
[TestFixture]
public class AttemptTests
{
    /// <summary>
    /// Verifies that the <c>Attempt.If</c> method correctly interprets the result of <c>int.TryParse</c> as a condition,
    /// and that it sets the <c>Success</c> and <c>Result</c> properties appropriately for both successful and unsuccessful parses.
    /// </summary>
    [Test]
    public void AttemptIf()
    {
        // Just making sure that it is ok to use TryParse as a condition.
        var attempt = Attempt.If(int.TryParse("1234", out var value), value);
        Assert.IsTrue(attempt.Success);
        Assert.AreEqual(1234, attempt.Result);

        attempt = Attempt.If(int.TryParse("12xxx34", out value), value);
        Assert.IsFalse(attempt.Success);
    }
}
