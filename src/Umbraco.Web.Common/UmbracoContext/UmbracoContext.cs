using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
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
    private readonly Lazy<IPublishedSnapshot> _publishedSnapshot;
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
        IPublishedSnapshotService publishedSnapshotService,
        UmbracoRequestPaths umbracoRequestPaths,
        IHostingEnvironment hostingEnvironment,
        UriUtility uriUtility,
        ICookieManager cookieManager,
        IHttpContextAccessor httpContextAccessor)
    {
        if (publishedSnapshotService == null)
        {
            throw new ArgumentNullException(nameof(publishedSnapshotService));
        }

        _uriUtility = uriUtility;
        _hostingEnvironment = hostingEnvironment;
        _cookieManager = cookieManager;
        _httpContextAccessor = httpContextAccessor;
        ObjectCreated = DateTime.Now;
        UmbracoRequestId = Guid.NewGuid();
        _umbracoRequestPaths = umbracoRequestPaths;

        // beware - we cannot expect a current user here, so detecting preview mode must be a lazy thing
        _publishedSnapshot =
            new Lazy<IPublishedSnapshot>(() => publishedSnapshotService.CreatePublishedSnapshot(PreviewToken));
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
    public IPublishedSnapshot PublishedSnapshot => _publishedSnapshot.Value;

    /// <inheritdoc />
    public IPublishedContentCache? Content => PublishedSnapshot.Content;

    /// <inheritdoc />
    public IPublishedMediaCache? Media => PublishedSnapshot.Media;

    /// <inheritdoc />
    public IDomainCache? Domains => PublishedSnapshot.Domains;

    /// <inheritdoc />
    public IPublishedRequest? PublishedRequest { get; set; }

    /// <inheritdoc />
    public bool IsDebug => // NOTE: the request can be null during app startup!
        _hostingEnvironment.IsDebugMode
        && (string.IsNullOrEmpty(_httpContextAccessor.HttpContext?.GetRequestValue("umbdebugshowtrace")) == false
            || string.IsNullOrEmpty(_httpContextAccessor.HttpContext?.GetRequestValue("umbdebug")) == false
            || string.IsNullOrEmpty(_cookieManager.GetCookieValue("UMB-DEBUG")) == false);

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

    /// <inheritdoc />
    public IDisposable ForcedPreview(bool preview)
    {
        // say we render a macro or RTE in a give 'preview' mode that might not be the 'current' one,
        // then due to the way it all works at the moment, the 'current' published snapshot need to be in the proper
        // default 'preview' mode - somehow we have to force it. and that could be recursive.
        InPreviewMode = preview;
        return PublishedSnapshot.ForcedPreview(preview, orig => InPreviewMode = orig);
    }

    /// <inheritdoc />
    protected override void DisposeResources()
    {
        // DisposableObject ensures that this runs only once

        // help caches release resources
        // (but don't create caches just to dispose them)
        // context is not multi-threaded
        if (_publishedSnapshot.IsValueCreated)
        {
            _publishedSnapshot.Value.Dispose();
        }
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
}
