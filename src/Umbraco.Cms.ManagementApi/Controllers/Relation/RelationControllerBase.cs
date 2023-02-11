using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Relation;

[ApiController]
[BackOfficeRoute("api/v{version:apiVersion}/relation")]
[OpenApiTag("Relation")]
[ApiVersion("1.0")]
// TODO: Implement Authentication
public abstract class RelationControllerBase : ManagementApiControllerBase
{

}
