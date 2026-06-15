// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using System.Security.Claims;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.BackOffice;

[TestFixture]
public class UmbracoBackOfficeIdentityTests
{
    public const string TestIssuer = "TestIssuer";

    [Test]
    public void Create_From_Claims_Identity()
    {
        var securityStamp = Guid.NewGuid().ToString();
        var claimsIdentity = new ClaimsIdentity(new[]
        {
            // This is the id that 'identity' uses to check for the user id.
            new Claim(ClaimTypes.NameIdentifier, "1234", ClaimValueTypes.Integer32, TestIssuer, TestIssuer),

            // This is the id that 'identity' uses to check for the username.
            new Claim(ClaimTypes.Name, "testing", ClaimValueTypes.String, TestIssuer, TestIssuer),
            new Claim(ClaimTypes.GivenName, "hello world", ClaimValueTypes.String, TestIssuer, TestIssuer),
            new Claim(ClaimTypes.Locality, "en-us", ClaimValueTypes.String, TestIssuer, TestIssuer),
            new Claim(ClaimsIdentity.DefaultRoleClaimType, "admin", ClaimValueTypes.String, TestIssuer, TestIssuer),
            new Claim(Constants.Security.SecurityStampClaimType, securityStamp, ClaimValueTypes.String, TestIssuer, TestIssuer),
        });

        if (!claimsIdentity.VerifyBackOfficeIdentity(out var verifiedIdentity))
        {
            Assert.Fail();
        }

        Assert.That(verifiedIdentity.Actor, Is.Null);
        Assert.That(verifiedIdentity.GetId(), Is.EqualTo(1234));
        Assert.That(verifiedIdentity.GetSecurityStamp(), Is.EqualTo(securityStamp));
        Assert.That(verifiedIdentity.GetUsername(), Is.EqualTo("testing"));
        Assert.That(verifiedIdentity.GetRealName(), Is.EqualTo("hello world"));
        Assert.That(verifiedIdentity.GetCultureString(), Is.EqualTo("en-us"));
        Assert.That(new[] { "admin" }.SequenceEqual(verifiedIdentity.GetRoles()), Is.True);

        Assert.That(verifiedIdentity.Claims.Count(), Is.EqualTo(6));
    }

    [Test]
    public void Create_From_Claims_Identity_Missing_Required_Claim()
    {
        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1234", ClaimValueTypes.Integer32, TestIssuer, TestIssuer),
            new Claim(ClaimTypes.Name, "testing", ClaimValueTypes.String, TestIssuer, TestIssuer),
        });

        if (claimsIdentity.VerifyBackOfficeIdentity(out _))
        {
            Assert.Fail();
        }

        Assert.Pass();
    }

    [Test]
    public void Create_From_Claims_Identity_Required_Claim_Null()
    {
        var claimsIdentity = new ClaimsIdentity(new[]
        {
            // Null or empty
            new Claim(ClaimTypes.NameIdentifier, string.Empty, ClaimValueTypes.Integer32, TestIssuer, TestIssuer),
            new Claim(ClaimTypes.Name, "testing", ClaimValueTypes.String, TestIssuer, TestIssuer),
            new Claim(ClaimTypes.GivenName, "hello world", ClaimValueTypes.String, TestIssuer, TestIssuer),
            new Claim(ClaimTypes.Locality, "en-us", ClaimValueTypes.String, TestIssuer, TestIssuer),
            new Claim(ClaimsIdentity.DefaultRoleClaimType, "admin", ClaimValueTypes.String, TestIssuer, TestIssuer),
        });

        if (claimsIdentity.VerifyBackOfficeIdentity(out _))
        {
            Assert.Fail();
        }

        Assert.Pass();
    }

    [Test]
    public void Create_With_Claims_And_User_Data()
    {
        var securityStamp = Guid.NewGuid().ToString();

        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim("TestClaim1", "test", ClaimValueTypes.Integer32, TestIssuer, TestIssuer),
            new Claim("TestClaim1", "test", ClaimValueTypes.Integer32, TestIssuer, TestIssuer),
        });

        claimsIdentity.AddRequiredClaims(
            "1234",
            Guid.NewGuid(),
            "testing",
            "hello world",
            new[] { 654 },
            new[] { 654 },
            "en-us",
            securityStamp,
            new[] { "content", "media" },
            new[] { "admin" });

        Assert.That(claimsIdentity.Claims.Count(), Is.EqualTo(9));
        Assert.That(claimsIdentity.Actor, Is.Null);
    }
}
