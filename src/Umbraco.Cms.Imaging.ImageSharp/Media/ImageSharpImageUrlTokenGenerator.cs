using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Middleware;
using Umbraco.Cms.Core.Media;

namespace Umbraco.Cms.Imaging.ImageSharp.Media;

/// <summary>
/// ImageSharp-backed <see cref="IImageUrlTokenGenerator"/> that re-signs image URLs using the
/// configured HMAC secret key.
/// </summary>
/// <remarks>
/// <para>
/// Computed tokens are cached in-process keyed by the canonical, sanitised URL (path + only the
/// query parameters that contribute to the HMAC). Cache-buster parameters such as <c>v</c> or
/// <c>rnd</c> are stripped from the cache key so they do not bloat memory when used per-request.
/// The cache is bounded with an entry cap and sliding expiration so stale entries (e.g. for
/// deleted media or unused crop variants) cannot accumulate indefinitely.
/// </para>
/// <para>
/// The secret key is captured at construction via <see cref="IOptions{TOptions}"/>, mirroring
/// ImageSharp.Web's own <c>RequestAuthorizationUtilities</c>. Changing the key at runtime
/// requires an application restart.
/// </para>
/// </remarks>
public sealed class ImageSharpImageUrlTokenGenerator : IImageUrlTokenGenerator, IDisposable
{
    private readonly RequestAuthorizationUtilities? _requestAuthorizationUtilities;
    private readonly ImageSharpMiddlewareOptions _options;
    private readonly MemoryCache _tokenCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageSharpImageUrlTokenGenerator"/> class.
    /// </summary>
    /// <param name="requestAuthorizationUtilities">The ImageSharp request authorization utilities used to compute HMAC tokens.</param>
    /// <param name="options">The ImageSharp middleware options containing the HMAC secret key.</param>
    public ImageSharpImageUrlTokenGenerator(
        RequestAuthorizationUtilities? requestAuthorizationUtilities,
        IOptions<ImageSharpMiddlewareOptions> options)
    {
        _requestAuthorizationUtilities = requestAuthorizationUtilities;
        _options = options.Value;

        // 10k entries × ~500 bytes per CacheEntry (canonical URL ~250 chars, 64-char token,
        // dictionary overhead) caps the in-process token cache around 5 MB.
        _tokenCache = new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = 10_000,
        });
    }

    /// <inheritdoc />
    public string RefreshSignature(string url)
    {
        if (string.IsNullOrEmpty(url) || _requestAuthorizationUtilities is null || _options.HMACSecretKey.Length == 0)
        {
            return url;
        }

        SplitUrl split = SplitAndDecodeQuery(url);

        Dictionary<string, StringValues> outputParams = split.DecodedQuery.Length == 0
            ? []
            : QueryHelpers.ParseQuery(split.DecodedQuery);

        // Strip any previously persisted HMAC token; we are about to compute a fresh one.
        outputParams.Remove(RequestAuthorizationUtilities.TokenCommand);

        // Build a sanitised command collection - only parameters recognised by an
        // IImageWebProcessor - and use that as the cache key. This collapses different cache-buster
        // values (which don't affect the HMAC) onto the same entry.
        var canonicalCommands = new CommandCollection();
        foreach (KeyValuePair<string, StringValues> kvp in outputParams)
        {
            canonicalCommands.Add(kvp.Key, kvp.Value.ToString());
        }

        _requestAuthorizationUtilities.StripUnknownCommands(canonicalCommands);

        var canonicalUrl = BuildUrl(split.Path, canonicalCommands);

        // Sign over the already-sanitised URL; no need for ImageSharp.Web to sanitise again.
        var token = _tokenCache.GetOrCreate(canonicalUrl, entry =>
        {
            entry.Size = 1;
            entry.SlidingExpiration = TimeSpan.FromHours(24);
            return ComputeToken(canonicalUrl);
        }) ?? string.Empty;

        if (string.IsNullOrEmpty(token) is false)
        {
            outputParams[RequestAuthorizationUtilities.TokenCommand] = token;
        }

        return BuildOutputUrl(split.Path, outputParams, split.HadEntityEncoding);
    }

    /// <inheritdoc />
    public void Dispose() => _tokenCache.Dispose();

    /// <summary>
    /// Splits a URL into its path and a parsable query, normalising HTML-entity-encoded
    /// ampersands in the query portion.
    /// </summary>
    /// <remarks>
    /// Rich text editors store URLs with <c>&amp;amp;</c> as the query separator so the
    /// surrounding markup is valid HTML. The query must be decoded before it can be parsed
    /// as a standard query string; the returned <see cref="SplitUrl.HadEntityEncoding"/>
    /// flag tells the caller to re-encode the output the same way. The path is returned
    /// unchanged.
    /// </remarks>
    private static SplitUrl SplitAndDecodeQuery(string url)
    {
        var questionIndex = url.IndexOf('?');
        var pathPart = questionIndex < 0 ? url : url[..questionIndex];
        var rawQuery = questionIndex < 0 ? string.Empty : url[(questionIndex + 1)..];
        var hadEntityEncoding = rawQuery.Contains("&amp;", StringComparison.Ordinal);
        if (hadEntityEncoding)
        {
            rawQuery = rawQuery.Replace("&amp;", "&");
        }

        return new SplitUrl(pathPart, rawQuery, hadEntityEncoding);
    }

    private string ComputeToken(string canonicalUrl)
        => _requestAuthorizationUtilities!.ComputeHMAC(canonicalUrl, CommandHandling.None) ?? string.Empty;

    private sealed record SplitUrl(string Path, string DecodedQuery, bool HadEntityEncoding);

    private static string BuildUrl(string pathPart, CommandCollection commands)
    {
        if (commands.Keys.Any() is false)
        {
            return pathPart;
        }

        var dict = new Dictionary<string, string?>();
        foreach (var key in commands.Keys)
        {
            dict[key] = commands[key];
        }

        return QueryHelpers.AddQueryString(pathPart, dict);
    }

    private static string BuildOutputUrl(string pathPart, Dictionary<string, StringValues> queryParams, bool entityEncodeSeparators)
    {
        if (queryParams.Count == 0)
        {
            return pathPart;
        }

        var dict = new Dictionary<string, string?>(queryParams.Count);
        foreach (KeyValuePair<string, StringValues> kvp in queryParams)
        {
            dict[kvp.Key] = kvp.Value.ToString();
        }

        var withQuery = QueryHelpers.AddQueryString(pathPart, dict);
        if (entityEncodeSeparators is false)
        {
            return withQuery;
        }

        // Only encode the query portion - the path part is left untouched even if it contains '&'.
        var qStart = pathPart.Length + 1;
        return withQuery[..qStart] + withQuery[qStart..].Replace("&", "&amp;");
    }
}
