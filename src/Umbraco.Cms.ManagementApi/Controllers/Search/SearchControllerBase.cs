using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Search;

[ApiController]
[VersionedApiBackOfficeRoute("search")]
[OpenApiTag("Search")]
public class SearchControllerBase : ManagementApiControllerBase
{
}
