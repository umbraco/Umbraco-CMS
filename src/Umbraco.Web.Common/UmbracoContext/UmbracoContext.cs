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
    // set the urls lazily, no need to allocate until they are needed...
    // NOTE: The request will not be available during app startup so we can only set this to an absolute URL of localhost, this
    // is a work around to being able to access the UmbracoContext during application startup and this will also ensure that people
    // 'could' still generate URLs during startup BUT any domain driven URL generation will not work because it is NOT possible to get
    // the current domain during application startup.
    // see: http://issues.umbraco.org/issue/U4-1890
    public Uri OriginalRequestUrl =>
_originalRequestUrl ??= RequestUrl ?? new Uri("http://localhost");

    /// <inheritdoc />
    // set the urls lazily, no need to allocate until they are needed...
    public Uri CleanedUmbracoUrl =>
_cleanedUmbracoUrl ??= _uriUtility.UriToUmbraco(OriginalRequestUrl);

    /// <inheritdoc />
    public IPublishedContentCache Content => _cacheManager.Content;

    /// <inheritdoc />
    public IPublishedMediaCache Media => _cacheManager.Media;

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
