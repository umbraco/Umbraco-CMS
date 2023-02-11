using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.TrackedReferences;

[ApiController]
[BackOfficeRoute("api/v{version:apiVersion}/trackedReferences")]
[OpenApiTag("TrackedReferences")]
[ApiVersion("1.0")]
public abstract class TrackedReferencesControllerBase : ManagementApiControllerBase
{
}
