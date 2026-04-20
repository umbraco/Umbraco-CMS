using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.UmbracoContext;

/// <summary>
///     Class that encapsulates Umbraco information of a specific HTTP request
/// </summary>
public class UmbracoContext : DisposableObjectSlim, IUmbracoContext
{
    private static readonly Uri FallbackUrl = new("http://localhost");

    private readonly ICookieManager _cookieManager;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebProfilerService _webProfilerService;
    private readonly ICacheManager _cacheManager;
    private readonly UmbracoRequestPaths _umbracoRequestPaths;
    private readonly UriUtility _uriUtility;
    private Uri? _cleanedUmbracoUrl;
    private Uri? _originalRequestUrl;
    private bool? _previewing;
    private string? _previewToken;
    private Uri? _requestUrl;

    // initializes a new instance of the UmbracoContext class
    // internal for unit tests
    // otherwise it's used by EnsureContext above
    // warn: does *not* manage setting any IUmbracoContextAccessor
    internal UmbracoContext(
        UmbracoRequestPaths umbracoRequestPaths,
        IHostingEnvironment hostingEnvironment,
        UriUtility uriUtility,
        ICookieManager cookieManager,
        IHttpContextAccessor httpContextAccessor,
        IWebProfilerService webProfilerService,
        ICacheManager cacheManager)
    {

        _uriUtility = uriUtility;
        _hostingEnvironment = hostingEnvironment;
        _cookieManager = cookieManager;
        _httpContextAccessor = httpContextAccessor;
        _webProfilerService = webProfilerService;
        _cacheManager = cacheManager;
        ObjectCreated = DateTime.Now;
        UmbracoRequestId = Guid.NewGuid();
        _umbracoRequestPaths = umbracoRequestPaths;
    }

    /// <inheritdoc />
    public DateTime ObjectCreated { get; }

    /// <summary>
    ///     Gets the context Id
    /// </summary>
    /// <remarks>
    ///     Used internally for debugging and also used to define anything required to distinguish this request from another.
    /// </remarks>
    internal Guid UmbracoRequestId { get; }

    internal string? PreviewToken
    {
        get
        {
            if (_previewing.HasValue == false)
            {
                DetectPreviewMode();
            }

            return _previewToken;
        }
    }

    // lazily get/create a Uri for the current request
    private Uri? RequestUrl => _requestUrl ??= _httpContextAccessor.HttpContext is null
        ? null
        : new Uri(_httpContextAccessor.HttpContext.Request.GetEncodedUrl());

    /// <inheritdoc />
    /// <remarks>
    /// <para>The URL is resolved lazily in the following order of precedence:</para>
    /// <list type="number">
    ///   <item>The current HTTP request URL, when an <see cref="HttpContext"/> is available.
    ///   This value is cached for the lifetime of this context instance.</item>
    ///   <item>The configured <see cref="IHostingEnvironment.ApplicationMainUrl"/>, which is set from
    ///   <c>Umbraco:CMS:WebRouting:UmbracoApplicationUrl</c> or auto-detected from the first request.
    ///   This allows background services (e.g. <c>RecurringHostedServiceBase</c>) to generate absolute
    ///   URLs with the correct scheme and host.</item>
    ///   <item>A fallback of <c>http://localhost</c> when neither of the above is available (e.g. during
    ///   application startup before any request has been processed).</item>
    /// </list>
    /// <para>Fallback values (2 and 3) are not cached, so that a later-detected
    /// <see cref="IHostingEnvironment.ApplicationMainUrl"/> is picked up on the next access.</para>
    /// </remarks>
    public Uri OriginalRequestUrl
    {
        get
        {
            if (_originalRequestUrl is not null)
            {
                return _originalRequestUrl;
            }

            // Cache only when we have a real request URL — it is stable for this context's lifetime.
            if (RequestUrl is not null)
            {
                return _originalRequestUrl = RequestUrl;
            }

            // Don't cache fallback values — ApplicationMainUrl may become available after the
            // first HTTP request is processed (see EnsureApplicationMainUrl).
            return _hostingEnvironment.ApplicationMainUrl ?? FallbackUrl;
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// Like <see cref="OriginalRequestUrl"/>, this value is only cached when a real HTTP request URL
    /// is available. Fallback-derived values are re-evaluated on each access.
    /// </remarks>
    public Uri CleanedUmbracoUrl
    {
        get
        {
            if (_cleanedUmbracoUrl is not null)
            {
                return _cleanedUmbracoUrl;
            }

            Uri cleaned = _uriUtility.UriToUmbraco(OriginalRequestUrl);

            // _originalRequestUrl is set as a side effect of the OriginalRequestUrl access above
            // only when backed by a real HTTP request. When it's still null the value came from a
            // fallback, so we don't cache — allowing a later-detected ApplicationMainUrl to take effect.
            if (_originalRequestUrl is not null)
            {
                _cleanedUmbracoUrl = cleaned;
            }

            return cleaned;
        }
    }

    /// <inheritdoc />
    public IPublishedContentCache Content => _cacheManager.Content;

    /// <inheritdoc />
    public IPublishedMediaCache Media => _cacheManager.Media;

    /// <inheritdoc />
    public IPublishedElementCache Elements => _cacheManager.Elements;

    /// <inheritdoc />
    public IDomainCache Domains => _cacheManager.Domains;

    /// <inheritdoc />
    public IPublishedRequest? PublishedRequest { get; set; }

    /// <inheritdoc />
    public bool IsDebug
    {
        get
        {
            if (_hostingEnvironment.IsDebugMode is false)
            {
                return false;
            }

            if(string.IsNullOrEmpty(_httpContextAccessor.HttpContext?.GetRequestValue("umbdebugshowtrace")) is false)
            {
                return true;
            }

            Attempt<bool, WebProfilerOperationStatus> webProfilerStatusAttempt = _webProfilerService.GetStatus().GetAwaiter().GetResult();

            if (webProfilerStatusAttempt.Success)
            {
                return webProfilerStatusAttempt.Result;
            }

            return true;
        }
    }

    /// <inheritdoc />
    public bool InPreviewMode
    {
        get
        {
            if (_previewing.HasValue == false)
            {
                DetectPreviewMode();
            }

            return _previewing ?? false;
        }
        private set => _previewing = value;
    }

    private void DetectPreviewMode()
    {
        if (RequestUrl != null
            && _umbracoRequestPaths.IsBackOfficeRequest(RequestUrl.AbsolutePath) == false
            && _httpContextAccessor.HttpContext?.GetCurrentIdentity() != null)
        {
            var previewToken =
                _cookieManager.GetCookieValue(Core.Constants.Web.PreviewCookieName); // may be null or empty
            _previewToken = previewToken.IsNullOrWhiteSpace() ? null : previewToken;
        }

        _previewing = _previewToken.IsNullOrWhiteSpace() == false;
    }

    // TODO: Remove this
    protected override void DisposeResources()
    {
    }
}
