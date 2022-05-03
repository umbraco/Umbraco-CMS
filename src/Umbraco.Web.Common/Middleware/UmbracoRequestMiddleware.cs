using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Smidge.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.WebAssets;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Cms.Web.Common.Profiler;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Middleware;

/// <summary>
///     Manages Umbraco request objects and their lifetime
/// </summary>
/// <remarks>
///     <para>
///         This is responsible for initializing the content cache
///     </para>
///     <para>
///         This is responsible for creating and assigning an <see cref="IUmbracoContext" />
///     </para>
/// </remarks>
public class UmbracoRequestMiddleware : IMiddleware
{
    private readonly BackOfficeWebAssets _backOfficeWebAssets;
    private readonly IDefaultCultureAccessor _defaultCultureAccessor;
    private readonly IEventAggregator _eventAggregator;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ILogger<UmbracoRequestMiddleware> _logger;
    private readonly WebProfiler? _profiler;
    private readonly IRequestCache _requestCache;
    private readonly IRuntimeState _runtimeState;

    private readonly IUmbracoContextFactory _umbracoContextFactory;
    private readonly IOptions<UmbracoRequestOptions> _umbracoRequestOptions;
    private readonly UmbracoRequestPaths _umbracoRequestPaths;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private SmidgeOptions _smidgeOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoRequestMiddleware" /> class.
    /// </summary>
    // Obsolete, scheduled for removal in V11
    [Obsolete("Use constructor that takes an IOptions<UmbracoRequestOptions>")]
    public UmbracoRequestMiddleware(
        ILogger<UmbracoRequestMiddleware> logger,
        IUmbracoContextFactory umbracoContextFactory,
        IRequestCache requestCache,
        IEventAggregator eventAggregator,
        IProfiler profiler,
        IHostingEnvironment hostingEnvironment,
        UmbracoRequestPaths umbracoRequestPaths,
        BackOfficeWebAssets backOfficeWebAssets,
        IOptionsMonitor<SmidgeOptions> smidgeOptions,
        IRuntimeState runtimeState,
        IVariationContextAccessor variationContextAccessor,
        IDefaultCultureAccessor defaultCultureAccessor)
        : this(
            logger,
            umbracoContextFactory,
            requestCache,
            eventAggregator,
            profiler,
            hostingEnvironment,
            umbracoRequestPaths,
            backOfficeWebAssets,
            smidgeOptions,
            runtimeState,
            variationContextAccessor,
            defaultCultureAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IOptions<UmbracoRequestOptions>>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoRequestMiddleware" /> class.
    /// </summary>
    public UmbracoRequestMiddleware(
        ILogger<UmbracoRequestMiddleware> logger,
        IUmbracoContextFactory umbracoContextFactory,
        IRequestCache requestCache,
        IEventAggregator eventAggregator,
        IProfiler profiler,
        IHostingEnvironment hostingEnvironment,
        UmbracoRequestPaths umbracoRequestPaths,
        BackOfficeWebAssets backOfficeWebAssets,
        IOptionsMonitor<SmidgeOptions> smidgeOptions,
        IRuntimeState runtimeState,
        IVariationContextAccessor variationContextAccessor,
        IDefaultCultureAccessor defaultCultureAccessor,
        IOptions<UmbracoRequestOptions> umbracoRequestOptions)
    {
        _logger = logger;
        _umbracoContextFactory = umbracoContextFactory;
        _requestCache = requestCache;
        _eventAggregator = eventAggregator;
        _hostingEnvironment = hostingEnvironment;
        _umbracoRequestPaths = umbracoRequestPaths;
        _backOfficeWebAssets = backOfficeWebAssets;
        _runtimeState = runtimeState;
        _variationContextAccessor = variationContextAccessor;
        _defaultCultureAccessor = defaultCultureAccessor;
        _umbracoRequestOptions = umbracoRequestOptions;
        _smidgeOptions = smidgeOptions.CurrentValue;
        _profiler = profiler as WebProfiler; // Ignore if not a WebProfiler

        smidgeOptions.OnChange(x => _smidgeOptions = x);
    }

    /// <inheritdoc />
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // do not process if client-side request
        if (context.Request.IsClientSideRequest() &&
            !_umbracoRequestOptions.Value.HandleAsServerSideRequest(context.Request))
        {
            // we need this here because for bundle requests, these are 'client side' requests that we need to handle
            LazyInitializeBackOfficeServices(context.Request.Path);
            await next(context);
            return;
        }

        // Profiling start needs to be one of the first things that happens.
        // Also MiniProfiler.Current becomes null if it is handled by the event aggregator due to async/await
        _profiler?.UmbracoApplicationBeginRequest(context, _runtimeState.Level);

        _variationContextAccessor.VariationContext ??= new VariationContext(_defaultCultureAccessor.DefaultCulture);
        UmbracoContextReference umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext();

        Uri? currentApplicationUrl = GetApplicationUrlFromCurrentRequest(context.Request);
        _hostingEnvironment.EnsureApplicationMainUrl(currentApplicationUrl);

        var pathAndQuery = context.Request.GetEncodedPathAndQuery();

        try
        {
            // Verbose log start of every request
            LogHttpRequest.TryGetCurrentHttpRequestId(out Guid? httpRequestId, _requestCache);
            _logger.LogTrace("Begin request [{HttpRequestId}]: {RequestUrl}", httpRequestId, pathAndQuery);

            try
            {
                LazyInitializeBackOfficeServices(context.Request.Path);
                await _eventAggregator.PublishAsync(
                    new UmbracoRequestBeginNotification(umbracoContextReference.UmbracoContext));
            }
            catch (Exception ex)
            {
                // try catch so we don't kill everything in all requests
                _logger.LogError(ex.Message);
            }
            finally
            {
                try
                {
                    await next(context);
                }
                finally
                {
                    await _eventAggregator.PublishAsync(
                        new UmbracoRequestEndNotification(umbracoContextReference.UmbracoContext));
                }
            }
        }
        finally
        {
            // Verbose log end of every request (in v8 we didn't log the end request of ALL requests, only the front-end which was
            // strange since we always logged the beginning, so now we just log start/end of all requests)
            LogHttpRequest.TryGetCurrentHttpRequestId(out Guid? httpRequestId, _requestCache);
            _logger.LogTrace(
                "End Request [{HttpRequestId}]: {RequestUrl} ({RequestDuration}ms)",
                httpRequestId,
                pathAndQuery,
                DateTime.Now.Subtract(umbracoContextReference.UmbracoContext.ObjectCreated).TotalMilliseconds);

            try
            {
                DisposeHttpContextItems(context.Request);
            }
            finally
            {
                // Dispose the umbraco context reference which will in turn dispose the UmbracoContext itself.
                umbracoContextReference.Dispose();
            }
        }

        // Profiling end needs to be last of the first things that happens.
        // Also MiniProfiler.Current becomes null if it is handled by the event aggregator due to async/await
        _profiler?.UmbracoApplicationEndRequest(context, _runtimeState.Level);
    }

    /// <summary>
    ///     Used to lazily initialize any back office services when the first request to the back office is made
    /// </summary>
    private void LazyInitializeBackOfficeServices(PathString absPath)
    {
        if (s_firstBackOfficeRequest)
        {
            return;
        }

        if (_umbracoRequestPaths.IsBackOfficeRequest(absPath)
            || (absPath.Value?.InvariantStartsWith($"/{_smidgeOptions.UrlOptions.CompositeFilePath}") ?? false)
            || (absPath.Value?.InvariantStartsWith($"/{_smidgeOptions.UrlOptions.BundleFilePath}") ?? false))
        {
            LazyInitializer.EnsureInitialized(ref s_firstBackOfficeRequest, ref s_firstBackOfficeReqestFlag,
                ref s_firstBackOfficeRequestLocker, () =>
                {
                    _backOfficeWebAssets.CreateBundles();
                    return true;
                });
        }
    }

    private Uri? GetApplicationUrlFromCurrentRequest(HttpRequest request)
    {
        // We only consider GET and POST.
        // Especially the DEBUG sent when debugging the application is annoying because it uses http, even when the https is available.
        if (request.Method == "GET" || request.Method == "POST")
        {
            return new Uri($"{request.Scheme}://{request.Host}{request.PathBase}", UriKind.Absolute);
        }

        return null;
    }

    /// <summary>
    ///     Dispose some request scoped objects that we are maintaining the lifecycle for.
    /// </summary>
    private void DisposeHttpContextItems(HttpRequest request)
    {
        // do not process if client-side request
        if (request.IsClientSideRequest())
        {
            return;
        }

        // ensure this is disposed by DI at the end of the request
        IHttpScopeReference httpScopeReference =
            request.HttpContext.RequestServices.GetRequiredService<IHttpScopeReference>();
        httpScopeReference.Register();
    }

#pragma warning disable IDE0044 // Add readonly modifier
    private static bool s_firstBackOfficeRequest;
    private static bool s_firstBackOfficeReqestFlag;
    private static object s_firstBackOfficeRequestLocker = new();
#pragma warning restore IDE0044 // Add readonly modifier
}
