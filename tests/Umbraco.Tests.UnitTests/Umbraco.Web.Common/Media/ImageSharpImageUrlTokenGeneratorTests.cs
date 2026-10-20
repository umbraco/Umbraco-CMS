// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Imaging.ImageSharp.Media;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Media;

[TestFixture]
public class ImageSharpImageUrlTokenGeneratorTests
{
    private const string MediaPath = "/media/1001/img.jpg";

    private static readonly byte[] _keyBytes =
    {
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
        16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31,
    };

    [Test]
    public void Returns_Url_Unchanged_When_No_Secret_Configured()
    {
        var options = new ImageSharpMiddlewareOptions(); // empty HMACSecretKey
        var requestAuthorization = BuildRequestAuthorization(options);

        var generator = new ImageSharpImageUrlTokenGenerator(requestAuthorization, Options.Create(options));

        const string url = MediaPath + "?width=400&height=400";
        Assert.AreEqual(url, generator.RefreshSignature(url));
    }

    [Test]
    public void Returns_Url_Unchanged_When_RequestAuthorization_Is_Null()
    {
        var options = new ImageSharpMiddlewareOptions { HMACSecretKey = _keyBytes };

        var generator = new ImageSharpImageUrlTokenGenerator(null, Options.Create(options));

        const string url = MediaPath + "?width=400&height=400";
        Assert.AreEqual(url, generator.RefreshSignature(url));
    }

    [Test]
    public void Returns_Empty_Input_Unchanged()
    {
        var options = new ImageSharpMiddlewareOptions { HMACSecretKey = _keyBytes };
        var requestAuthorization = BuildRequestAuthorization(options);

        var generator = new ImageSharpImageUrlTokenGenerator(requestAuthorization, Options.Create(options));

        Assert.AreEqual(string.Empty, generator.RefreshSignature(string.Empty));
    }

    [Test]
    public void Appends_Token_When_None_Present()
    {
        var options = new ImageSharpMiddlewareOptions { HMACSecretKey = _keyBytes };
        var requestAuthorization = BuildRequestAuthorization(options);

        var generator = new ImageSharpImageUrlTokenGenerator(requestAuthorization, Options.Create(options));

        var result = generator.RefreshSignature(MediaPath + "?width=400&height=400");

        StringAssert.StartsWith(MediaPath + "?width=400&height=400&hmac=", result);
    }

    [Test]
    public void Replaces_Existing_Stale_Token()
    {
        var options = new ImageSharpMiddlewareOptions { HMACSecretKey = _keyBytes };
        var requestAuthorization = BuildRequestAuthorization(options);

        var generator = new ImageSharpImageUrlTokenGenerator(requestAuthorization, Options.Create(options));

        var freshFromScratch = generator.RefreshSignature(MediaPath + "?width=400&height=400");
        var refreshedFromStale = generator.RefreshSignature(MediaPath + "?width=400&height=400&hmac=deadbeefdeadbeef");

        Assert.AreEqual(freshFromScratch, refreshedFromStale);
        Assert.AreEqual(1, CountOccurrences(refreshedFromStale, "hmac="));
    }

    [Test]
    public void Produces_Same_Token_As_ImageSharpImageUrlGenerator()
    {
        // Round-trip: a URL produced by the editor-side generator must round-trip through the signer unchanged.
        // Pins down that both call sites canonicalise/sign the URL the same way.
        var options = new ImageSharpMiddlewareOptions { HMACSecretKey = _keyBytes };
        var requestAuthorization = BuildRequestAuthorization(options);

        var editorGenerator = new ImageSharpImageUrlGenerator(
            Array.Empty<string>(),
            Options.Create(options),
            requestAuthorization);

        var tokenGenerator = new ImageSharpImageUrlTokenGenerator(requestAuthorization, Options.Create(options));

        var signedByEditor = editorGenerator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
        {
            Width = 400,
            Height = 400,
        });

        Assert.IsNotNull(signedByEditor);
        Assert.AreEqual(signedByEditor, tokenGenerator.RefreshSignature(signedByEditor!));
    }

    [Test]
    public void Path_With_No_Recognised_Commands_Returns_Unsigned()
    {
        // Matches the editor-side behaviour in ImageSharpImageUrlGenerator: when ImageSharp.Web has
        // no recognised commands to sign, ComputeHMAC returns empty and no token is attached.
        var options = new ImageSharpMiddlewareOptions { HMACSecretKey = _keyBytes };
        var requestAuthorization = BuildRequestAuthorization(options);

        var generator = new ImageSharpImageUrlTokenGenerator(requestAuthorization, Options.Create(options));

        Assert.AreEqual(MediaPath, generator.RefreshSignature(MediaPath));
    }

    [Test]
    public void Preserves_HtmlEntity_Encoded_Ampersands()
    {
        var options = new ImageSharpMiddlewareOptions { HMACSecretKey = _keyBytes };
        var requestAuthorization = BuildRequestAuthorization(options);

        var generator = new ImageSharpImageUrlTokenGenerator(requestAuthorization, Options.Create(options));

        var result = generator.RefreshSignature(MediaPath + "?width=400&amp;height=400&amp;hmac=stale");

        StringAssert.StartsWith(MediaPath + "?width=400&amp;height=400&amp;hmac=", result);
        StringAssert.DoesNotContain("hmac=stale", result);
        Assert.AreEqual(0, CountOccurrences(result, "&hmac="), "ampersands must remain entity-encoded");
        Assert.AreEqual(1, CountOccurrences(result, "&amp;hmac="), "exactly one fresh token, &amp;-encoded");
    }

    [Test]
    public void Does_Not_Encode_Ampersands_In_The_Path()
    {
        // The path is not part of the query and any '&' in it must not be encoded on output.
        var options = new ImageSharpMiddlewareOptions { HMACSecretKey = _keyBytes };
        var requestAuthorization = BuildRequestAuthorization(options);

        var generator = new ImageSharpImageUrlTokenGenerator(requestAuthorization, Options.Create(options));

        // The input uses &amp; in the query (triggers entity-preservation), but the path contains a literal '&'.
        const string pathWithAmpersand = "/media/foo&bar/img.jpg";
        var result = generator.RefreshSignature(pathWithAmpersand + "?width=400&amp;height=400");

        StringAssert.StartsWith(pathWithAmpersand + "?width=400&amp;height=400&amp;hmac=", result);
    }

    [Test]
    public void Different_Cache_Busters_Do_Not_Bloat_Cache()
    {
        // Cache-busters ('v', 'rnd') don't contribute to the HMAC (they're stripped by Sanitize).
        // Two URLs differing only in their cache-buster should produce the same token. Same token
        // implies the cache key collapsed both inputs onto a single entry.
        var options = new ImageSharpMiddlewareOptions { HMACSecretKey = _keyBytes };
        var requestAuthorization = BuildRequestAuthorization(options);

        var generator = new ImageSharpImageUrlTokenGenerator(requestAuthorization, Options.Create(options));

        var firstResult = generator.RefreshSignature(MediaPath + "?width=400&v=20240101");
        var secondResult = generator.RefreshSignature(MediaPath + "?width=400&v=20251231");

        var firstToken = TokenFrom(firstResult);
        var secondToken = TokenFrom(secondResult);

        Assert.IsNotEmpty(firstToken);
        Assert.AreEqual(firstToken, secondToken);

        // Each output URL retains its own cache buster.
        StringAssert.Contains("v=20240101", firstResult);
        StringAssert.Contains("v=20251231", secondResult);
    }

    [Test]
    public void Subsequent_Calls_Are_Deterministic()
    {
        var options = new ImageSharpMiddlewareOptions { HMACSecretKey = _keyBytes };
        var requestAuthorization = BuildRequestAuthorization(options);

        var generator = new ImageSharpImageUrlTokenGenerator(requestAuthorization, Options.Create(options));

        var first = generator.RefreshSignature(MediaPath + "?width=400");
        var second = generator.RefreshSignature(MediaPath + "?width=400");

        Assert.AreEqual(first, second);
    }

    private static RequestAuthorizationUtilities BuildRequestAuthorization(ImageSharpMiddlewareOptions options)
        => new(
            Options.Create(options),
            new QueryCollectionRequestParser(),
            [new ResizeWebProcessor()],
            new CommandParser(Enumerable.Empty<ICommandConverter>()),
            new ServiceCollection().BuildServiceProvider());

    private static int CountOccurrences(string haystack, string needle)
    {
        var count = 0;
        var idx = 0;
        while ((idx = haystack.IndexOf(needle, idx, StringComparison.Ordinal)) >= 0)
        {
            count++;
            idx += needle.Length;
        }

        return count;
    }

    private static string TokenFrom(string signedUrl)
    {
        const string marker = "hmac=";
        var idx = signedUrl.LastIndexOf(marker, StringComparison.Ordinal);
        return idx < 0 ? string.Empty : signedUrl[(idx + marker.Length)..];
    }
}
