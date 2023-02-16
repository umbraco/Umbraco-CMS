using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.AuditLog;

[ApiController]
[VersionedApiBackOfficeRoute("auditlog")]
[ApiExplorerSettings(GroupName = "Auditlog")]
[ApiVersion("1.0")]
public class AuditLogControllerBase : ManagementApiControllerBase
{
}
