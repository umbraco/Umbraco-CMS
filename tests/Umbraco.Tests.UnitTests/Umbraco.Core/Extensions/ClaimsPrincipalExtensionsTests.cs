// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using System.Security.Claims;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

/// <summary>
/// Unit tests for the ClaimsPrincipalExtensions class.
/// </summary>
[TestFixture]
public class ClaimsPrincipalExtensionsTests
{
    /// <summary>
    /// Verifies that the <see cref="ClaimsPrincipalExtensions.GetRemainingAuthSeconds"/> extension method correctly calculates
    /// the number of seconds remaining until the authentication ticket expires, based on the claims present in the principal.
    /// </summary>
    [Test]
    public void Get_Remaining_Ticket_Seconds()
    {
        var backOfficeIdentity = new ClaimsIdentity();
        backOfficeIdentity.AddRequiredClaims(
            Constants.Security.SuperUserIdAsString,
            Constants.Security.SuperUserKey,
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

    /// <summary>
    /// Verifies that AddOrUpdateClaim does not add duplicate claims of the same type to the ClaimsIdentity.
    /// Ensures only one claim of a given type exists after multiple additions or updates.
    /// </summary>
    [Test]
    public void AddOrUpdateClaim__Should_ensure_a_claim_is_not_added_twice()
    {
        var backOfficeIdentity = new ClaimsIdentity();
        backOfficeIdentity.AddRequiredClaims(
            Constants.Security.SuperUserIdAsString,
            Constants.Security.SuperUserKey,
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
