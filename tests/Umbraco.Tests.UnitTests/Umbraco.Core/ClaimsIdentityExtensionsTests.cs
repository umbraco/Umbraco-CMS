// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core;

public class ClaimsIdentityExtensionsTests
{
    [Test]
    public void FindFirstValue_WhenIdentityIsNull_ExpectArgumentNullException()
    {
        ClaimsIdentity identity = null;
        Assert.Throws<ArgumentNullException>(() => identity.FindFirstValue("test"));
    }

    [Test]
    public void FindFirstValue_WhenClaimNotPresent_ExpectNull()
    {
        var identity = new ClaimsIdentity(new List<Claim>());
        var value = identity.FindFirstValue("test");
        Assert.IsNull(value);
    }

    [Test]
    public void FindFirstValue_WhenMatchingClaimPresent_ExpectCorrectValue()
    {
        var expectedClaim = new Claim("test", "123", "string", "Umbraco");
        var identity = new ClaimsIdentity(new List<Claim> { expectedClaim });

        var value = identity.FindFirstValue("test");

        Assert.AreEqual(expectedClaim.Value, value);
    }

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
