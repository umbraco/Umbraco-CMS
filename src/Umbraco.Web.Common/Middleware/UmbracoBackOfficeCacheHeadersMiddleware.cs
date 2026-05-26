using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Umbraco.Cms.Web.Common.Hosting;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Web.Common.Middleware;

/// <summary>
/// Sets the default <c>Cache-Control</c> response header on requests served from the cache-busted
/// BackOffice assets path (<c>/umbraco/backoffice/&lt;hash&gt;/...</c>).
/// </summary>
/// <remarks>
///     <para>
///         The path prefix contains a deployment-wide hash derived from the Umbraco version
///         (see <see cref="IBackOfficePathGenerator.BackOfficeCacheBustHash"/>). Because the URL itself
///         changes whenever the version changes, all responses served under that prefix are safe to mark
///         as <c>immutable</c> with a long <c>max-age</c>, regardless of whether the on-disk filename
///         contains a content hash.
///     </para>
///     <para>
///         In debug mode the underlying built assets may change while the app is running (typically
///         from a developer rebuilding the backoffice without restarting the host). The header is
///         therefore set to <c>no-cache</c>, which still allows the browser to store the response
///         but forces an <c>If-None-Match</c> revalidation on the next request — yielding fast 304s
///         when nothing has changed and full 200s when the file on disk has been rebuilt.
///         <c>no-store</c> would force a full re-download on every request, which is unnecessary.
///     </para>
///     <para>
///         This middleware is non-destructive to consumer customisation:
///         <list type="bullet">
///             <item>
///                 The header is only set when no <c>Cache-Control</c> value is already present on the
///                 response, so synchronous overrides written upstream (including
///                 <c>StaticFileOptions.OnPrepareResponse</c>) take precedence.
///             </item>
///             <item>
///                 The header is set via <c>HttpResponse.OnStarting</c>; consumer callbacks registered
///                 later in the pipeline fire first (LIFO) and can therefore override the default.
///             </item>
///             <item>
///                 Non-2xx responses (e.g. 404) are not marked as immutable to avoid long-lived caching
///                 of error responses.
///             </item>
///         </list>
///     </para>
///     <para>
///         Must run before <see cref="Umbraco.Extensions.ApplicationBuilderExtensions.UseUmbracoBackOfficeRewrites"/>
///         so the original request path (still containing the cache-bust hash) can be matched.
///     </para>
/// </remarks>
/// <seealso cref="Microsoft.AspNetCore.Http.IMiddleware" />
public class UmbracoBackOfficeCacheHeadersMiddleware : IMiddleware
{
    private readonly string _prefix;
    private readonly string _headerValue;

    public UmbracoBackOfficeCacheHeadersMiddleware(
        IBackOfficePathGenerator backOfficePathGenerator,
        IHostingEnvironment hostingEnvironment)
    {
        // Normalise to a single leading slash, no trailing slash — defensive against any
        // future change in IBackOfficePathGenerator's output shape.
        _prefix = "/" + backOfficePathGenerator.BackOfficeAssetsPath.TrimStart('/').TrimEnd('/');
        _headerValue = hostingEnvironment.IsDebugMode
            ? "no-cache"
            : "public, max-age=31536000, immutable";
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (IsCacheableAssetRequest(context.Request))
        {
            context.Response.OnStarting(static state =>
            {
                (HttpResponse response, string value) = ((HttpResponse, string))state;
                if (ShouldSetCacheControl(response))
                {
                    response.Headers[HeaderNames.CacheControl] = value;
                }

                return Task.CompletedTask;
            }, (context.Response, _headerValue));
        }

        await next(context);
    }

    // Only GET/HEAD: POST/PUT/DELETE responses aren't cacheable in the immutable sense and
    // OPTIONS is used for CORS preflight, where a long cache lifetime would prevent the
    // browser from re-issuing preflights when needed.
    private bool IsCacheableAssetRequest(HttpRequest request)
        => (HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method))
            && request.Path.StartsWithSegments(_prefix, StringComparison.OrdinalIgnoreCase);

    // Include 304 alongside 2xx: intermediate caches (CDNs, proxies) use the Cache-Control on
    // the 304 response to update freshness for the cached body.
    private static bool ShouldSetCacheControl(HttpResponse response)
        => response.StatusCode is (>= 200 and < 300) or 304
            && !response.Headers.ContainsKey(HeaderNames.CacheControl);
}
