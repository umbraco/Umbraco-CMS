using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Routing;

public class DefaultAbsoluteUrlBuilder : IAbsoluteUrlBuilder
{
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly IUrlAssembler _urlAssembler;

    public DefaultAbsoluteUrlBuilder(IUmbracoContextAccessor umbracoContextAccessor, IUrlAssembler urlAssembler)
    {
        _umbracoContextAccessor = umbracoContextAccessor;
        _urlAssembler = urlAssembler;
    }

    public Uri ToAbsoluteUrl(string url)
    {
        IUmbracoContext umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
        Uri current = umbracoContext.CleanedUmbracoUrl;

        return _urlAssembler.AssembleUrl(url, current, UrlMode.Absolute);
    }
}
