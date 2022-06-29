using System.Globalization;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Core.Routing;

// TODO: Kill this, but we need to port all of it's functionality
public class PublishedRequestOld // : IPublishedRequest
{
    private readonly IPublishedRouter _publishedRouter;
    private readonly WebRoutingSettings _webRoutingSettings;
    private CultureInfo? _culture;
    private DomainAndUri? _domain;
    private bool _is404;
    private IPublishedContent? _publishedContent;

    private bool _readonly; // after prepared

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublishedRequest" /> class.
    /// </summary>
    public PublishedRequestOld(IPublishedRouter publishedRouter, IUmbracoContext umbracoContext, IOptions<WebRoutingSettings> webRoutingSettings, Uri? uri = null)
    {
        UmbracoContext = umbracoContext ?? throw new ArgumentNullException(nameof(umbracoContext));
        _publishedRouter = publishedRouter ?? throw new ArgumentNullException(nameof(publishedRouter));
        _webRoutingSettings = webRoutingSettings.Value;
        Uri = uri ?? umbracoContext.CleanedUmbracoUrl;
    }

    /// <summary>
    ///     Gets the UmbracoContext.
    /// </summary>
    public IUmbracoContext UmbracoContext { get; }

    /// <summary>
    ///     Gets or sets the cleaned up Uri used for routing.
    /// </summary>
    /// <remarks>The cleaned up Uri has no virtual directory, no trailing slash, no .aspx extension, etc.</remarks>
    public Uri Uri { get; }

    public bool CacheabilityNoCache { get; set; }

    ///// <summary>
    ///// Prepares the request.
    ///// </summary>
    // public void Prepare()
    // {
    //     _publishedRouter.PrepareRequest(this);
    // }

    /// <summary>
    ///     Gets or sets a value indicating whether the Umbraco Backoffice should ignore a collision for this request.
    /// </summary>
    public bool IgnorePublishedContentCollisions { get; set; }

    /// <summary>
    ///     Gets or sets the template model to use to display the requested content.
    /// </summary>
    public ITemplate? Template { get; }

    /// <summary>
    ///     Gets the alias of the template to use to display the requested content.
    /// </summary>
    public string? TemplateAlias => Template?.Alias;

    /// <summary>
    ///     Gets or sets the content request's domain.
    /// </summary>
    /// <remarks>
    ///     Is a DomainAndUri object ie a standard Domain plus the fully qualified uri. For example,
    ///     the <c>Domain</c> may contain "example.com" whereas the <c>Uri</c> will be fully qualified eg
    ///     "http://example.com/".
    /// </remarks>
    public DomainAndUri? Domain
    {
        get => _domain;
        set
        {
            EnsureWriteable();
            _domain = value;
        }
    }

    /// <summary>
    ///     Gets a value indicating whether the content request has a domain.
    /// </summary>
    public bool HasDomain => Domain != null;

    /// <summary>
    ///     Gets or sets the content request's culture.
    /// </summary>
    public CultureInfo Culture
    {
        get => _culture ?? Thread.CurrentThread.CurrentCulture;
        set
        {
            EnsureWriteable();
            _culture = value;
        }
    }

    // utility for ensuring it is ok to set some properties
    public void EnsureWriteable()
    {
        if (_readonly)
        {
            throw new InvalidOperationException("Cannot modify a PublishedRequest once it is read-only.");
        }
    }

    // #region Events

    ///// <summary>
    ///// Triggers before the published content request is prepared.
    ///// </summary>
    ///// <remarks>When the event triggers, no preparation has been done. It is still possible to
    ///// modify the request's Uri property, for example to restore its original, public-facing value
    ///// that might have been modified by an in-between equipment such as a load-balancer.</remarks>
    // public static event EventHandler<EventArgs> Preparing;

    ///// <summary>
    ///// Triggers once the published content request has been prepared, but before it is processed.
    ///// </summary>
    ///// <remarks>When the event triggers, preparation is done ie domain, culture, document, template,
    ///// rendering engine, etc. have been setup. It is then possible to change anything, before
    ///// the request is actually processed and rendered by Umbraco.</remarks>
    // public static event EventHandler<EventArgs> Prepared;

    ///// <summary>
    ///// Triggers the Preparing event.
    ///// </summary>
    // public void OnPreparing()
    // {
    //     Preparing?.Invoke(this, EventArgs.Empty);
    // }

    ///// <summary>
    ///// Triggers the Prepared event.
    ///// </summary>
    // public void OnPrepared()
    // {
    //     Prepared?.Invoke(this, EventArgs.Empty);

    // if (HasPublishedContent == false)
    //         Is404 = true; // safety

    // _readonly = true;
    // }

    // #endregion
    #region PublishedContent

    ///// <summary>
    ///// Gets or sets the requested content.
    ///// </summary>
    ///// <remarks>Setting the requested content clears <c>Template</c>.</remarks>
    // public IPublishedContent PublishedContent
    // {
    //    get { return _publishedContent; }
    //    set
    //    {
    //        EnsureWriteable();
    //        _publishedContent = value;
    //        IsInternalRedirectPublishedContent = false;
    //        TemplateModel = null;
    //    }
    // }

    /// <summary>
    ///     Sets the requested content, following an internal redirect.
    /// </summary>
    /// <param name="content">The requested content.</param>
    /// <remarks>
    ///     Depending on <c>UmbracoSettings.InternalRedirectPreservesTemplate</c>, will
    ///     preserve or reset the template, if any.
    /// </remarks>
    public void SetInternalRedirectPublishedContent(IPublishedContent content)
    {
        // if (content == null)
        //     throw new ArgumentNullException(nameof(content));
        // EnsureWriteable();

        //// unless a template has been set already by the finder,
        //// template should be null at that point.

        //// IsInternalRedirect if IsInitial, or already IsInternalRedirect
        // var isInternalRedirect = IsInitialPublishedContent || IsInternalRedirectPublishedContent;

        //// redirecting to self
        // if (content.Id == PublishedContent.Id) // neither can be null
        // {
        //     // no need to set PublishedContent, we're done
        //     IsInternalRedirectPublishedContent = isInternalRedirect;
        //     return;
        // }

        //// else

        //// save
        // var template = Template;

        //// set published content - this resets the template, and sets IsInternalRedirect to false
        // PublishedContent = content;
        // IsInternalRedirectPublishedContent = isInternalRedirect;

        //// must restore the template if it's an internal redirect & the config option is set
        // if (isInternalRedirect && _webRoutingSettings.InternalRedirectPreservesTemplate)
        // {
        //    // restore
        //    TemplateModel = template;
        // }
    }

    /// <summary>
    ///     Gets the initial requested content.
    /// </summary>
    /// <remarks>
    ///     The initial requested content is the content that was found by the finders,
    ///     before anything such as 404, redirect... took place.
    /// </remarks>
    public IPublishedContent? InitialPublishedContent { get; private set; }

    /// <summary>
    ///     Gets value indicating whether the current published content is the initial one.
    /// </summary>
    public bool IsInitialPublishedContent =>
        InitialPublishedContent != null && InitialPublishedContent == _publishedContent;

    /// <summary>
    ///     Indicates that the current PublishedContent is the initial one.
    /// </summary>
    public void SetIsInitialPublishedContent()
    {
        EnsureWriteable();

        // note: it can very well be null if the initial content was not found
        InitialPublishedContent = _publishedContent;
        IsInternalRedirectPublishedContent = false;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the current published content has been obtained
    ///     from the initial published content following internal redirections exclusively.
    /// </summary>
    /// <remarks>
    ///     Used by PublishedContentRequestEngine.FindTemplate() to figure out whether to
    ///     apply the internal redirect or not, when content is not the initial content.
    /// </remarks>
    public bool IsInternalRedirectPublishedContent { get; private set; }

    #endregion

    // note: do we want to have an ordered list of alternate cultures,
    // to allow for fallbacks when doing dictionary lookup and such?
    #region Status

    /// <summary>
    ///     Gets or sets a value indicating whether the requested content could not be found.
    /// </summary>
    /// <remarks>
    ///     This is set in the <c>PublishedContentRequestBuilder</c> and can also be used in
    ///     custom content finders or <c>Prepared</c> event handlers, where we want to allow developers
    ///     to indicate a request is 404 but not to cancel it.
    /// </remarks>
    public bool Is404
    {
        get => _is404;
        set
        {
            EnsureWriteable();
            _is404 = value;
        }
    }

    /// <summary>
    ///     Gets a value indicating whether the content request triggers a redirect (permanent or not).
    /// </summary>
    public bool IsRedirect => string.IsNullOrWhiteSpace(RedirectUrl) == false;

    /// <summary>
    ///     Gets or sets a value indicating whether the redirect is permanent.
    /// </summary>
    public bool IsRedirectPermanent { get; private set; }

    /// <summary>
    ///     Gets or sets the URL to redirect to, when the content request triggers a redirect.
    /// </summary>
    public string? RedirectUrl { get; private set; }

    /// <summary>
    ///     Indicates that the content request should trigger a redirect (302).
    /// </summary>
    /// <param name="url">The URL to redirect to.</param>
    /// <remarks>
    ///     Does not actually perform a redirect, only registers that the response should
    ///     redirect. Redirect will or will not take place in due time.
    /// </remarks>
    public void SetRedirect(string url)
    {
        EnsureWriteable();
        RedirectUrl = url;
        IsRedirectPermanent = false;
    }

    /// <summary>
    ///     Indicates that the content request should trigger a permanent redirect (301).
    /// </summary>
    /// <param name="url">The URL to redirect to.</param>
    /// <remarks>
    ///     Does not actually perform a redirect, only registers that the response should
    ///     redirect. Redirect will or will not take place in due time.
    /// </remarks>
    public void SetRedirectPermanent(string url)
    {
        EnsureWriteable();
        RedirectUrl = url;
        IsRedirectPermanent = true;
    }

    /// <summary>
    ///     Indicates that the content request should trigger a redirect, with a specified status code.
    /// </summary>
    /// <param name="url">The URL to redirect to.</param>
    /// <param name="status">The status code (300-308).</param>
    /// <remarks>
    ///     Does not actually perform a redirect, only registers that the response should
    ///     redirect. Redirect will or will not take place in due time.
    /// </remarks>
    public void SetRedirect(string url, int status)
    {
        EnsureWriteable();

        if (status < 300 || status > 308)
        {
            throw new ArgumentOutOfRangeException(nameof(status), "Valid redirection status codes 300-308.");
        }

        RedirectUrl = url;
        IsRedirectPermanent = status == 301 || status == 308;

        // default redirect statuses
        if (status != 301 && status != 302)
        {
            ResponseStatusCode = status;
        }
    }

    /// <summary>
    ///     Gets or sets the content request http response status code.
    /// </summary>
    /// <remarks>
    ///     Does not actually set the http response status code, only registers that the response
    ///     should use the specified code. The code will or will not be used, in due time.
    /// </remarks>
    public int ResponseStatusCode { get; private set; }

    /// <summary>
    ///     Gets or sets the content request http response status description.
    /// </summary>
    /// <remarks>
    ///     Does not actually set the http response status description, only registers that the response
    ///     should use the specified description. The description will or will not be used, in due time.
    /// </remarks>
    public string? ResponseStatusDescription { get; private set; }

    /// <summary>
    ///     Sets the http response status code, along with an optional associated description.
    /// </summary>
    /// <param name="code">The http status code.</param>
    /// <param name="description">The description.</param>
    /// <remarks>
    ///     Does not actually set the http response status code and description, only registers that
    ///     the response should use the specified code and description. The code and description will or will
    ///     not be used, in due time.
    /// </remarks>
    public void SetResponseStatus(int code, string? description = null)
    {
        EnsureWriteable();

        // .Status is deprecated
        // .SubStatusCode is IIS 7+ internal, ignore
        ResponseStatusCode = code;
        ResponseStatusDescription = description;
    }

    #endregion

    #region Response Cache

    // ///  <summary>
    // /// Gets or sets the <c>System.Web.HttpCacheability</c>
    // /// </summary>
    // Note: we used to set a default value here but that would then be the default
    // for ALL requests, we shouldn't overwrite it though if people are using [OutputCache] for example
    // see: https://our.umbraco.com/forum/using-umbraco-and-getting-started/79715-output-cache-in-umbraco-752
    // public HttpCacheability Cacheability { get; set; }

    /// <summary>
    ///     Gets or sets a list of Extensions to append to the Response.Cache object.
    /// </summary>
    public List<string> CacheExtensions { get; set; } = new();

    /// <summary>
    ///     Gets or sets a dictionary of Headers to append to the Response object.
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    #endregion
}
