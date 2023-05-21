using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Web.Common.UmbracoContext;

/// <summary>
///     Creates and manages <see cref="IUmbracoContext" /> instances.
/// </summary>
public class UmbracoContextFactory : IUmbracoContextFactory
{
    private readonly ICookieManager _cookieManager;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPublishedSnapshotService _publishedSnapshotService;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly UmbracoRequestPaths _umbracoRequestPaths;
    private readonly UriUtility _uriUtility;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoContextFactory" /> class.
    /// </summary>
    public UmbracoContextFactory(
        IUmbracoContextAccessor umbracoContextAccessor,
        IPublishedSnapshotService publishedSnapshotService,
        UmbracoRequestPaths umbracoRequestPaths,
        IHostingEnvironment hostingEnvironment,
        UriUtility uriUtility,
        ICookieManager cookieManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _umbracoContextAccessor =
            umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
        _publishedSnapshotService = publishedSnapshotService ??
                                    throw new ArgumentNullException(nameof(publishedSnapshotService));
        _umbracoRequestPaths = umbracoRequestPaths ?? throw new ArgumentNullException(nameof(umbracoRequestPaths));
        _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        _uriUtility = uriUtility ?? throw new ArgumentNullException(nameof(uriUtility));
        _cookieManager = cookieManager ?? throw new ArgumentNullException(nameof(cookieManager));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc />
    public UmbracoContextReference EnsureUmbracoContext()
    {
        if (_umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext))
        {
            return new UmbracoContextReference(umbracoContext, false, _umbracoContextAccessor);
        }

        IUmbracoContext createdUmbracoContext = CreateUmbracoContext();

        _umbracoContextAccessor.Set(createdUmbracoContext);
        return new UmbracoContextReference(createdUmbracoContext, true, _umbracoContextAccessor);
    }

    private IUmbracoContext CreateUmbracoContext() => new UmbracoContext(
        _publishedSnapshotService,
        _umbracoRequestPaths,
        _hostingEnvironment,
        _uriUtility,
        _cookieManager,
        _httpContextAccessor);
}
