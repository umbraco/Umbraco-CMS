using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.TrackedReferences;

[ApiController]
[BackOfficeRoute("api/v{version:apiVersion}/trackedReferences")]
[OpenApiTag("TrackedReferences")]
public abstract class TrackedReferencesControllerBase : Controller
{
}
