// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Security.Claims;
using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core;

/// <summary>
/// Contains unit tests for the <see cref="ClaimsIdentityExtensions"/> class, verifying its extension methods and expected behaviors.
/// </summary>
public class ClaimsIdentityExtensionsTests
{
    /// <summary>
    /// Tests that FindFirstValue throws an ArgumentNullException when the identity is null.
    /// </summary>
    [Test]
    public void FindFirstValue_WhenIdentityIsNull_ExpectArgumentNullException()
    {
        ClaimsIdentity identity = null;
        Assert.Throws<ArgumentNullException>(() => identity.FindFirstValue("test"));
    }

    /// <summary>
    /// Tests that FindFirstValue returns null when the specified claim is not present.
    /// </summary>
    [Test]
    public void FindFirstValue_WhenClaimNotPresent_ExpectNull()
    {
        var identity = new ClaimsIdentity(new List<Claim>());
        var value = identity.FindFirstValue("test");
        Assert.IsNull(value);
    }

    /// <summary>
    /// Tests that FindFirstValue returns the correct claim value when a matching claim is present.
    /// </summary>
    [Test]
    public void FindFirstValue_WhenMatchingClaimPresent_ExpectCorrectValue()
    {
        var expectedClaim = new Claim("test", "123", "string", "Umbraco");
        var identity = new ClaimsIdentity(new List<Claim> { expectedClaim });

        var value = identity.FindFirstValue("test");

        Assert.AreEqual(expectedClaim.Value, value);
    }

    /// <summary>
    /// Tests that when multiple claims with the same type are present, the first claim's value is returned.
    /// </summary>
    [Test]
    public void FindFirstValue_WhenMultipleMatchingClaimsPresent_ExpectFirstValue()
    {
        var expectedClaim = new Claim("test", "123", "string", "Umbraco");
        var dupeClaim = new Claim(expectedClaim.Type, Guid.NewGuid().ToString());
        var identity = new ClaimsIdentity(new List<Claim> { expectedClaim, dupeClaim });

        var value = identity.FindFirstValue("test");

        Assert.AreEqual(expectedClaim.Value, value);
    }
}
