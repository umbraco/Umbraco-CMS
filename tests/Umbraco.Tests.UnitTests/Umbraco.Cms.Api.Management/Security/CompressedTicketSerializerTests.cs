// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Security;

[TestFixture]
public class CompressedTicketSerializerTests
{
    /// <summary>
    ///     The chunk size the default <see cref="Microsoft.AspNetCore.Internal.ChunkingCookieManager" /> splits at.
    /// </summary>
    private const int CookieChunkSize = 4090;

    private static CompressedTicketSerializer CreateSerializer(bool compress = true)
        => new(TicketSerializer.Default, compress);

    [Test]
    public void Serialize_Roundtrips_All_Claim_Detail()
    {
        AuthenticationTicket ticket = CreateTicket(startContentNodeCount: 40, startMediaNodeCount: 20, roleCount: 10);
        CompressedTicketSerializer sut = CreateSerializer();

        AuthenticationTicket? result = sut.Deserialize(sut.Serialize(ticket));

        Assert.That(result, Is.Not.Null);

        Claim[] expected = ticket.Principal.Claims.ToArray();
        Claim[] actual = result!.Principal.Claims.ToArray();

        Assert.That(actual, Has.Length.EqualTo(expected.Length));
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Multiple(() =>
            {
                Assert.That(actual[i].Type, Is.EqualTo(expected[i].Type));
                Assert.That(actual[i].Value, Is.EqualTo(expected[i].Value));
                Assert.That(actual[i].ValueType, Is.EqualTo(expected[i].ValueType));
                Assert.That(actual[i].Issuer, Is.EqualTo(expected[i].Issuer));
                Assert.That(actual[i].OriginalIssuer, Is.EqualTo(expected[i].OriginalIssuer));
            });
        }

        Assert.Multiple(() =>
        {
            Assert.That(result.AuthenticationScheme, Is.EqualTo(ticket.AuthenticationScheme));
            Assert.That(result.Principal.Identity!.AuthenticationType, Is.EqualTo(ticket.Principal.Identity!.AuthenticationType));
            Assert.That(result.Properties.ExpiresUtc, Is.EqualTo(ticket.Properties.ExpiresUtc));
        });
    }

    [Test]
    public void Deserialize_Reads_Payload_Written_Without_Compression()
    {
        // Cookies issued before compression was introduced have to keep working, so an uncompressed payload must be
        // passed through. This also verifies the marker cannot be mistaken for the start of one.
        AuthenticationTicket ticket = CreateTicket(startContentNodeCount: 40, startMediaNodeCount: 20, roleCount: 10);
        var uncompressed = TicketSerializer.Default.Serialize(ticket);

        AuthenticationTicket? result = CreateSerializer().Deserialize(uncompressed);

        Assert.That(result, Is.Not.Null);
        Assert.That(
            result!.Principal.Claims.Count(),
            Is.EqualTo(ticket.Principal.Claims.Count()));
    }

    [Test]
    public void Deserialize_Reads_Compressed_Payload_When_Compression_Is_Disabled()
    {
        // Writing is optional, reading is not - otherwise disabling compression would exercise a different read path
        // from the one that runs in production.
        AuthenticationTicket ticket = CreateTicket(startContentNodeCount: 40, startMediaNodeCount: 20, roleCount: 10);
        var compressed = CreateSerializer().Serialize(ticket);

        AuthenticationTicket? result = CreateSerializer(compress: false).Deserialize(compressed);

        Assert.That(result, Is.Not.Null);
        Assert.That(
            result!.Principal.Claims.Count(),
            Is.EqualTo(ticket.Principal.Claims.Count()));
    }

    [Test]
    public void Serialize_When_Compression_Disabled_Expect_Inner_Payload()
    {
        AuthenticationTicket ticket = CreateTicket(startContentNodeCount: 40, startMediaNodeCount: 20, roleCount: 10);

        var result = CreateSerializer(compress: false).Serialize(ticket);

        Assert.That(result, Is.EqualTo(TicketSerializer.Default.Serialize(ticket)));
    }

    [Test]
    public void Serialize_Is_Never_Larger_Than_The_Uncompressed_Payload()
    {
        // A payload too small to benefit is written uncompressed, so the worst case is parity.
        AuthenticationTicket ticket = CreateTicket(startContentNodeCount: 0, startMediaNodeCount: 0, roleCount: 1);

        var uncompressed = TicketSerializer.Default.Serialize(ticket);
        var result = CreateSerializer().Serialize(ticket);

        Assert.That(result, Has.Length.LessThanOrEqualTo(uncompressed.Length));
    }

    [Test]
    public void Deserialize_When_Compressed_Payload_Is_Truncated_Expect_Null()
    {
        // Truncation is the awkward case: enough of the stream survives to decompress, and it is only reading the
        // ticket back out of the result that fails.
        AuthenticationTicket ticket = CreateTicket(startContentNodeCount: 40, startMediaNodeCount: 20, roleCount: 10);
        var compressed = CreateSerializer().Serialize(ticket);
        var truncated = compressed[..(compressed.Length / 2)];

        Assert.That(CreateSerializer().Deserialize(truncated), Is.Null);
    }

    [Test]
    public void Deserialize_When_Compressed_Payload_Is_Not_Readable_Expect_Null()
    {
        byte[] garbage = [.. new byte[] { 0x55, 0x5A }, .. Enumerable.Repeat((byte)0xFF, 64)];

        Assert.That(CreateSerializer().Deserialize(garbage), Is.Null);
    }

    /// <summary>
    ///     Reports the cookie size for a range of user profiles, with and without compression.
    /// </summary>
    /// <remarks>
    ///     This measures the cookie rather than the serialized payload, because it is the cookie header that competes
    ///     with the request line for the request size limit. The assertion is deliberately weak - the numbers written
    ///     to the test output are the point.
    /// </remarks>
    [Test]
    public void Measure_Cookie_Size_Reduction()
    {
        (string Name, AuthenticationTicket Ticket)[] profiles =
        [
            ("Minimal (1 group, no start nodes)", CreateTicket(0, 0, 1)),
            ("Typical (3 groups, 2 start nodes)", CreateTicket(2, 0, 3)),
            ("Heavy (10 groups, 40 + 20 start nodes)", CreateTicket(40, 20, 10)),
            ("Heavy + 20 external login claims", CreateTicket(40, 20, 10, externalClaimCount: 20)),
        ];

        IDataProtector protector = new EphemeralDataProtectionProvider().CreateProtector("test");
        var uncompressedFormat = new SecureDataFormat<AuthenticationTicket>(CreateSerializer(compress: false), protector);
        var compressedFormat = new SecureDataFormat<AuthenticationTicket>(CreateSerializer(), protector);

        TestContext.Out.WriteLine("| Profile | Uncompressed bytes / chunks | Compressed bytes / chunks | Reduction |");
        TestContext.Out.WriteLine("|---|---|---|---|");

        foreach ((var name, AuthenticationTicket ticket) in profiles)
        {
            var uncompressed = uncompressedFormat.Protect(ticket).Length;
            var compressed = compressedFormat.Protect(ticket).Length;
            var reduction = 1 - ((double)compressed / uncompressed);

            TestContext.Out.WriteLine(
                $"| {name} | {uncompressed:N0} / {ChunkCount(uncompressed)} | {compressed:N0} / {ChunkCount(compressed)} | {reduction:P0} |");
        }

        // Only the profiles with enough repetition to compress are asserted on, and only for direction - a threshold
        // here would fail the build the day a claim is added.
        Assert.That(
            compressedFormat.Protect(profiles[2].Ticket).Length,
            Is.LessThan(uncompressedFormat.Protect(profiles[2].Ticket).Length));
    }

    private static int ChunkCount(int cookieLength)
        => (int)Math.Ceiling((double)cookieLength / CookieChunkSize);

    private static AuthenticationTicket CreateTicket(
        int startContentNodeCount,
        int startMediaNodeCount,
        int roleCount,
        int externalClaimCount = 0)
    {
        var identity = new ClaimsIdentity(Constants.Security.BackOfficeAuthenticationType);

        for (var i = 0; i < externalClaimCount; i++)
        {
            identity.AddClaim(new Claim($"http://schemas.example.org/identity/claims/external{i}", Guid.NewGuid().ToString()));
        }

        identity.AddRequiredClaims(
            "1234",
            Guid.NewGuid(),
            "author@example.org",
            "A N Author",
            Enumerable.Range(1000, startContentNodeCount),
            Enumerable.Range(2000, startMediaNodeCount),
            "en-US",
            Guid.NewGuid().ToString(),
            ["content", "media", "settings", "packages", "users", "members", "forms", "translation"],
            Enumerable.Range(0, roleCount).Select(_ => Guid.NewGuid().ToString()));

        return new AuthenticationTicket(
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { ExpiresUtc = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            Constants.Security.BackOfficeAuthenticationType);
    }
}
