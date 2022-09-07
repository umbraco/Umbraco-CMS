using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Relation;

[ApiController]
[BackOfficeRoute("api/v{version:apiVersion}/relation")]
[OpenApiTag("Relation")]
public abstract class RelationControllerBase : Controller
{

}
