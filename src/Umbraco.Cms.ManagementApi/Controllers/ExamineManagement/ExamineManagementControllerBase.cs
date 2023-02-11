using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.ExamineManagement;

[ApiController]
[BackOfficeRoute("api/v{version:apiVersion}/examineManagement")]
[OpenApiTag("ExamineManagement")]
public class ExamineManagementControllerBase : ManagementApiControllerBase
{
}
