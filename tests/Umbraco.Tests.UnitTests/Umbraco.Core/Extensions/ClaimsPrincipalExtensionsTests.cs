// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Linq;
using System.Security.Claims;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class ClaimsPrincipalExtensionsTests
{
    [Test]
    public void Get_Remaining_Ticket_Seconds()
    {
        var backOfficeIdentity = new ClaimsIdentity();
        backOfficeIdentity.AddRequiredClaims(
            Constants.Security.SuperUserIdAsString,
            "test",
            "test",
            Enumerable.Empty<int>(),
            Enumerable.Empty<int>(),
            "en-US",
            Guid.NewGuid().ToString(),
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>());

        var principal = new ClaimsPrincipal(backOfficeIdentity);

        var expireSeconds = 99;
        var elapsedSeconds = 3;
        var remainingSeconds = expireSeconds - elapsedSeconds;
        var now = DateTimeOffset.Now;
        var then = now.AddSeconds(elapsedSeconds);
        var expires = now.AddSeconds(expireSeconds).ToString("o");

        backOfficeIdentity.AddClaim(new Claim(
            Constants.Security.TicketExpiresClaimType,
            expires,
            ClaimValueTypes.DateTime,
            Constants.Security.BackOfficeAuthenticationType,
            Constants.Security.BackOfficeAuthenticationType,
            backOfficeIdentity));

        var ticketRemainingSeconds = principal.GetRemainingAuthSeconds(then);

        Assert.AreEqual(remainingSeconds, ticketRemainingSeconds);
    }

    [Test]
    public void AddOrUpdateClaim__Should_ensure_a_claim_is_not_added_twice()
    {
        var backOfficeIdentity = new ClaimsIdentity();
        backOfficeIdentity.AddRequiredClaims(
            Constants.Security.SuperUserIdAsString,
            "test",
            "test",
            Enumerable.Empty<int>(),
            Enumerable.Empty<int>(),
            "en-US",
            Guid.NewGuid().ToString(),
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>());

        var expireSeconds = 99;

        var now = DateTimeOffset.Now;

        var expires = now.AddSeconds(expireSeconds).ToString("o");

        var claim = new Claim(
            Constants.Security.TicketExpiresClaimType,
            expires,
            ClaimValueTypes.DateTime,
            Constants.Security.BackOfficeAuthenticationType,
            Constants.Security.BackOfficeAuthenticationType,
            backOfficeIdentity);

        backOfficeIdentity.AddOrUpdateClaim(claim);
        backOfficeIdentity.AddOrUpdateClaim(claim);

        Assert.AreEqual(1, backOfficeIdentity.Claims.Count(x => x.Type == Constants.Security.TicketExpiresClaimType));
    }
}
