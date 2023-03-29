using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Filters;
using Umbraco.Cms.Api.Content.Filters;
using Umbraco.Cms.Api.Content.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.ContentApi;

namespace Umbraco.Cms.Api.Content.Controllers;

[ApiController]
[VersionedContentApiRoute("content")]
[ApiExplorerSettings(GroupName = "Content")]
[ApiVersion("1.0")]
[ContentApiAccess]
[JsonOptionsName(Constants.JsonOptionsNames.ContentApi)]
[LocalizeFromAcceptLanguageHeader]
public abstract class ContentApiControllerBase : Controller
{
    protected IApiPublishedContentCache ApiPublishedContentCache { get; }

    protected IApiContentBuilder ApiContentBuilder { get; }

    protected ContentApiControllerBase(IApiPublishedContentCache apiPublishedContentCache, IApiContentBuilder apiContentBuilder)
    {
        ApiPublishedContentCache = apiPublishedContentCache;
        ApiContentBuilder = apiContentBuilder;
    }
}
