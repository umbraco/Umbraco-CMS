using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Routing;

/// <summary>
/// Provides the default implementation for building absolute URLs within the Umbraco CMS Management API.
/// </summary>
public class DefaultAbsoluteUrlBuilder : IAbsoluteUrlBuilder
{
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly IUrlAssembler _urlAssembler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultAbsoluteUrlBuilder"/> class, which is responsible for building absolute URLs using the provided context and URL assembler.
    /// </summary>
    /// <param name="umbracoContextAccessor">Provides access to the current Umbraco context.</param>
    /// <param name="urlAssembler">Service used to assemble URLs.</param>
    public DefaultAbsoluteUrlBuilder(IUmbracoContextAccessor umbracoContextAccessor, IUrlAssembler urlAssembler)
    {
        _umbracoContextAccessor = umbracoContextAccessor;
        _urlAssembler = urlAssembler;
    }

    /// <summary>
    /// Converts the specified relative or absolute URL string to an absolute <see cref="Uri"/>.
    /// </summary>
    /// <param name="url">The URL string to convert to an absolute URI.</param>
    /// <returns>An absolute <see cref="Uri"/> representing the specified URL.</returns>
    public Uri ToAbsoluteUrl(string url)
    {
        IUmbracoContext umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
        Uri current = umbracoContext.CleanedUmbracoUrl;

        return _urlAssembler.AssembleUrl(url, current, UrlMode.Absolute);
    }
}
