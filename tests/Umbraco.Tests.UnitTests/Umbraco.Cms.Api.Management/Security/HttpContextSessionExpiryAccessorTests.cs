// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Security;

[TestFixture]
public class HttpContextSessionExpiryAccessorTests
{
    private static readonly DateTimeOffset Expiry = new(2030, 1, 1, 12, 0, 0, TimeSpan.Zero);

    // The claim is written with ToString("o") by ConfigureBackOfficeCookieOptions, so round-tripping
    // that exact format is the contract that matters.
    [Test]
    public void Can_Read_Expiry_From_Round_Tripped_Ticket_Claim()
    {
        DateTimeOffset? result = GetSessionExpiry(Expiry.ToString("o"));

        Assert.That(result, Is.EqualTo(Expiry));
    }

    // A non-UTC offset must resolve to the same instant rather than being reinterpreted as local time,
    // which is what DateTimeStyles.RoundtripKind buys over a plain parse.
    [Test]
    public void Can_Preserve_The_Instant_When_The_Claim_Carries_An_Offset()
    {
        DateTimeOffset expiryWithOffset = Expiry.ToOffset(TimeSpan.FromHours(5));

        DateTimeOffset? result = GetSessionExpiry(expiryWithOffset.ToString("o"));

        Assert.That(result, Is.EqualTo(Expiry));
    }

    [Test]
    public void Cannot_Read_Expiry_When_The_Claim_Is_Absent()
    {
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) };

        Assert.That(CreateSut(httpContext).GetSessionExpiry(), Is.Null);
    }

    [TestCase("")]
    [TestCase("not-a-date")]
    [TestCase("2030-13-45T99:99:99")]
    public void Cannot_Read_Expiry_When_The_Claim_Is_Unparseable(string claimValue)
    {
        Assert.That(GetSessionExpiry(claimValue), Is.Null);
    }

    // Background work and non-request scopes have no ambient context; the accessor must answer rather
    // than throw, since the response model treats null as "no readable expiry".
    [Test]
    public void Cannot_Read_Expiry_When_There_Is_No_Http_Context()
    {
        Assert.That(CreateSut(httpContext: null).GetSessionExpiry(), Is.Null);
    }

    private static DateTimeOffset? GetSessionExpiry(string claimValue)
    {
        var identity = new ClaimsIdentity([new Claim(Constants.Security.TicketExpiresClaimType, claimValue)]);
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

        return CreateSut(httpContext).GetSessionExpiry();
    }

    private static HttpContextSessionExpiryAccessor CreateSut(HttpContext? httpContext)
        => new(Mock.Of<IHttpContextAccessor>(x => x.HttpContext == httpContext));
}
