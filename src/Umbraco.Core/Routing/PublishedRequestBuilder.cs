using System.Net;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Routing;

public class PublishedRequestBuilder : IPublishedRequestBuilder
{
    private readonly IFileService _fileService;
    private bool _cacheability;
    private IReadOnlyList<string>? _cacheExtensions;
    private IReadOnlyDictionary<string, string>? _headers;
    private bool _ignorePublishedContentCollisions;
    private IPublishedContent? _publishedContent;
    private string? _redirectUrl;
    private HttpStatusCode? _responseStatus;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublishedRequestBuilder" /> class.
    /// </summary>
    public PublishedRequestBuilder(Uri uri, IFileService fileService)
    {
        Uri = uri;
        AbsolutePathDecoded = uri.GetAbsolutePathDecoded();
        _fileService = fileService;
    }

    /// <inheritdoc />
    public Uri Uri { get; }

    /// <inheritdoc />
    public string AbsolutePathDecoded { get; }

    /// <inheritdoc />
    public DomainAndUri? Domain { get; private set; }

    /// <inheritdoc />
    public string? Culture { get; private set; }

    /// <inheritdoc />
    public ITemplate? Template { get; private set; }

    /// <inheritdoc />
    public bool IsInternalRedirect { get; private set; }

    /// <inheritdoc />
    public int? ResponseStatusCode => _responseStatus.HasValue ? (int?)_responseStatus : null;

    /// <inheritdoc />
    public IPublishedContent? PublishedContent
    {
        get => _publishedContent;
        private set
        {
            _publishedContent = value;
            IsInternalRedirect = false;
            Template = null;
        }
    }

    /// <inheritdoc />
    public IPublishedRequest Build() => new PublishedRequest(
        Uri,
        AbsolutePathDecoded,
        PublishedContent,
        IsInternalRedirect,
        Template,
        Domain,
        Culture,
        _redirectUrl,
        _responseStatus.HasValue ? (int?)_responseStatus : null,
        _cacheExtensions,
        _headers,
        _cacheability,
        _ignorePublishedContentCollisions);

    /// <inheritdoc />
    public IPublishedRequestBuilder SetNoCacheHeader(bool cacheability)
    {
        _cacheability = cacheability;
        return this;
    }

    /// <inheritdoc />
    public IPublishedRequestBuilder SetCacheExtensions(IEnumerable<string> cacheExtensions)
    {
        _cacheExtensions = cacheExtensions.ToList();
        return this;
    }

    /// <inheritdoc />
    public IPublishedRequestBuilder SetCulture(string? culture)
    {
        Culture = culture;
        return this;
    }

    /// <inheritdoc />
    public IPublishedRequestBuilder SetDomain(DomainAndUri domain)
    {
        Domain = domain;
        SetCulture(domain.Culture);
        return this;
    }

    /// <inheritdoc />
    public IPublishedRequestBuilder SetHeaders(IReadOnlyDictionary<string, string> headers)
    {
        _headers = headers;
        return this;
    }

    /// <inheritdoc />
    public IPublishedRequestBuilder SetInternalRedirect(IPublishedContent content)
    {
        // unless a template has been set already by the finder,
        // template should be null at that point.

        // redirecting to self
        if (PublishedContent != null && content.Id == PublishedContent.Id)
        {
            // no need to set PublishedContent, we're done
            IsInternalRedirect = true;
            return this;
        }

        // else

        // set published content - this resets the template, and sets IsInternalRedirect to false
        PublishedContent = content;
        IsInternalRedirect = true;

        return this;
    }

    /// <inheritdoc />
    public IPublishedRequestBuilder SetPublishedContent(IPublishedContent? content)
    {
        PublishedContent = content;
        IsInternalRedirect = false;
        return this;
    }

    /// <inheritdoc />
    public IPublishedRequestBuilder SetRedirect(string url, int status = (int)HttpStatusCode.Redirect)
    {
        _redirectUrl = url;
        _responseStatus = (HttpStatusCode)status;
        return this;
    }

    /// <inheritdoc />
    public IPublishedRequestBuilder SetRedirectPermanent(string url)
    {
        _redirectUrl = url;
        _responseStatus = HttpStatusCode.Moved;
        return this;
    }

    /// <inheritdoc />
    public IPublishedRequestBuilder SetResponseStatus(int code)
    {
        _responseStatus = (HttpStatusCode)code;
        return this;
    }

    /// <inheritdoc />
    public IPublishedRequestBuilder SetTemplate(ITemplate? template)
    {
        Template = template;
        return this;
    }

    /// <inheritdoc />
    public bool TrySetTemplate(string alias)
    {
        if (string.IsNullOrWhiteSpace(alias))
        {
            Template = null;
            return true;
        }

        // NOTE - can we still get it with whitespaces in it due to old legacy bugs?
        alias = alias.Replace(" ", string.Empty);

        ITemplate? model = _fileService.GetTemplate(alias);
        if (model == null)
        {
            return false;
        }

        Template = model;
        return true;
    }

    /// <inheritdoc />
    public void IgnorePublishedContentCollisions() => _ignorePublishedContentCollisions = true;
}
