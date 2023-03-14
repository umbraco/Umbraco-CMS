// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core;

[TestFixture]
public class AttemptTests
{
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
