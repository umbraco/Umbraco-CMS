using System.Collections.Concurrent;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Routing;

/// <summary>
///     Utility class used to check if the current request is for a front-end request
/// </summary>
/// <remarks>
///     There are various checks to determine if this is a front-end request such as checking if the request is part of any
///     reserved paths or existing MVC routes.
/// </remarks>
public sealed class RoutableDocumentFilter : IRoutableDocumentFilter
{
    private readonly EndpointDataSource _endpointDataSource;
    private readonly GlobalSettings _globalSettings;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ConcurrentDictionary<string, bool> _routeChecks = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _routeLocker = new();
    private readonly WebRoutingSettings _routingSettings;
    private object _initLocker = new();
    private bool _isInit;
    private HashSet<string>? _reservedList;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RoutableDocumentFilter" /> class.
    /// </summary>
    public RoutableDocumentFilter(IOptions<GlobalSettings> globalSettings, IOptions<WebRoutingSettings> routingSettings, IHostingEnvironment hostingEnvironment, EndpointDataSource endpointDataSource)
    {
        _globalSettings = globalSettings.Value;
        _routingSettings = routingSettings.Value;
        _hostingEnvironment = hostingEnvironment;
        _endpointDataSource = endpointDataSource;
        _endpointDataSource.GetChangeToken().RegisterChangeCallback(EndpointsChanged, null);
    }

    /// <summary>
    ///     Checks if the request is a document request (i.e. one that the module should handle)
    /// </summary>
    public bool IsDocumentRequest(string absPath)
    {
        var maybeDoc = true;

        // a document request should be
        // /foo/bar/nil
        // /foo/bar/nil/
        // where /foo is not a reserved path

        // if the path contains an extension
        // then it cannot be a document request
        var extension = Path.GetExtension(absPath);
        if (maybeDoc && !extension.IsNullOrWhiteSpace())
        {
            maybeDoc = false;
        }

        // at that point we have no extension

        // if the path is reserved then it cannot be a document request
        if (maybeDoc && IsReservedPathOrUrl(absPath))
        {
            maybeDoc = false;
        }

        return maybeDoc;
    }

    private void EndpointsChanged(object value)
    {
        lock (_routeLocker)
        {
            // try clearing each entry
            foreach (var r in _routeChecks.Keys.ToList())
            {
                _routeChecks.TryRemove(r, out _);
            }

            // re-register after it has changed so we keep listening
            _endpointDataSource.GetChangeToken().RegisterChangeCallback(EndpointsChanged, null);
        }
    }

    /// <summary>
    ///     Determines whether the specified URL is reserved or is inside a reserved path.
    /// </summary>
    /// <param name="absPath">The Path of the URL to check.</param>
    /// <returns>
    ///     <c>true</c> if the specified URL is reserved; otherwise, <c>false</c>.
    /// </returns>
    private bool IsReservedPathOrUrl(string absPath)
    {
        LazyInitializer.EnsureInitialized(ref _reservedList, ref _isInit, ref _initLocker, () =>
        {
            // store references to strings to determine changes
            var reservedPathsCache = _globalSettings.ReservedPaths;
            var reservedUrlsCache = _globalSettings.ReservedUrls;

            // add URLs and paths to a new list
            var newReservedList = new HashSet<string>();
            foreach (var reservedUrlTrimmed in NormalizePaths(reservedUrlsCache, false))
            {
                newReservedList.Add(reservedUrlTrimmed);
            }

            foreach (var reservedPathTrimmed in NormalizePaths(reservedPathsCache, true))
            {
                newReservedList.Add(reservedPathTrimmed);
            }

            // use the new list from now on
            return newReservedList;
        });

        // The URL should be cleaned up before checking:
        // * If it doesn't contain an '.' in the path then we assume it is a path based URL, if that is the case we should add an trailing '/' because all of our reservedPaths use a trailing '/'
        // * We shouldn't be comparing the query at all
        if (absPath.Contains('?'))
        {
            absPath = absPath.Split('?', StringSplitOptions.RemoveEmptyEntries)[0];
        }

        if (absPath.Contains('.') == false)
        {
            absPath = absPath.EnsureEndsWith('/');
        }

        // return true if URL starts with an element of the reserved list
        var isReserved = _reservedList?.Any(x => absPath.InvariantStartsWith(x)) ?? false;

        if (isReserved)
        {
            return true;
        }

        // If configured, check if the current request matches a route, if so then it is reserved,
        // else if not configured (default) proceed as normal since we assume the request is for an Umbraco content item.
        var hasRoute = _routingSettings.TryMatchingEndpointsForAllPages &&
                       _routeChecks.GetOrAdd(absPath, x => MatchesEndpoint(absPath));
        if (hasRoute)
        {
            return true;
        }

        return false;
    }

    private IEnumerable<string> NormalizePaths(string paths, bool ensureTrailingSlash) => paths
        .Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(x => x.Trim().ToLowerInvariant())
        .Where(x => x.IsNullOrWhiteSpace() == false)
        .Select(reservedPath =>
        {
            var r = _hostingEnvironment.ToAbsolute(reservedPath).Trim().EnsureStartsWith('/');
            return ensureTrailingSlash
                ? r.EnsureEndsWith('/')
                : r;
        })
        .Where(reservedPathTrimmed => reservedPathTrimmed.IsNullOrWhiteSpace() == false);

    private bool MatchesEndpoint(string absPath)
    {
        // Borrowed and modified from https://stackoverflow.com/a/59550580

        // Return a collection of Microsoft.AspNetCore.Http.Endpoint instances.
        IEnumerable<RouteEndpoint>? routeEndpoints = _endpointDataSource.Endpoints
            .OfType<RouteEndpoint>()
            .Where(x =>
            {
                // We don't want to include dynamic endpoints in this check since we would have no idea if that
                // matches since they will probably match everything.
                var isDynamic = x.Metadata.OfType<IDynamicEndpointMetadata>().Any(y => y.IsDynamic);
                if (isDynamic)
                {
                    return false;
                }

                // filter out matched endpoints that are suppressed
                var isSuppressed = x.Metadata.OfType<ISuppressMatchingMetadata>().FirstOrDefault()?.SuppressMatching ==
                                   true;
                if (isSuppressed)
                {
                    return false;
                }

                return true;
            });

        var routeValues = new RouteValueDictionary();

        // To get the matchedEndpoint of the provide url
        RouteEndpoint? matchedEndpoint = routeEndpoints
            .Where(e => e.RoutePattern.RawText != null)
            .Where(e => new TemplateMatcher(
                    TemplateParser.Parse(e.RoutePattern.RawText!),
                    new RouteValueDictionary())
                .TryMatch(absPath, routeValues))
            .OrderBy(c => c.Order)
            .FirstOrDefault();

        return matchedEndpoint != null;
    }
}
