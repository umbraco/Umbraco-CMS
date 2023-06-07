using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Routing;

public class PublishedRequest : IPublishedRequest
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PublishedRequest" /> class.
    /// </summary>
    public PublishedRequest(
        Uri uri,
        string absolutePathDecoded,
        IPublishedContent? publishedContent,
        bool isInternalRedirect,
        ITemplate? template,
        DomainAndUri? domain,
        string? culture,
        string? redirectUrl,
        int? responseStatusCode,
        IReadOnlyList<string>? cacheExtensions,
        IReadOnlyDictionary<string, string>? headers,
        bool setNoCacheHeader,
        bool ignorePublishedContentCollisions)
    {
        Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        AbsolutePathDecoded = absolutePathDecoded ?? throw new ArgumentNullException(nameof(absolutePathDecoded));
        PublishedContent = publishedContent;
        IsInternalRedirect = isInternalRedirect;
        Template = template;
        Domain = domain;
        Culture = culture;
        RedirectUrl = redirectUrl;
        ResponseStatusCode = responseStatusCode;
        CacheExtensions = cacheExtensions;
        Headers = headers;
        SetNoCacheHeader = setNoCacheHeader;
        IgnorePublishedContentCollisions = ignorePublishedContentCollisions;
    }

    /// <inheritdoc />
    public Uri Uri { get; }

    /// <inheritdoc />
    public string AbsolutePathDecoded { get; }

    /// <inheritdoc />
    public bool IgnorePublishedContentCollisions { get; }

    /// <inheritdoc />
    public IPublishedContent? PublishedContent { get; }

    /// <inheritdoc />
    public bool IsInternalRedirect { get; }

    /// <inheritdoc />
    public ITemplate? Template { get; }

    /// <inheritdoc />
    public DomainAndUri? Domain { get; }

    /// <inheritdoc />
    public string? Culture { get; }

    /// <inheritdoc />
    public string? RedirectUrl { get; }

    /// <inheritdoc />
    public int? ResponseStatusCode { get; }

    /// <inheritdoc />
    public IReadOnlyList<string>? CacheExtensions { get; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string>? Headers { get; }

    /// <inheritdoc />
    public bool SetNoCacheHeader { get; }
}
