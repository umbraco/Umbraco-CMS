using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.AuditLog;

[ApiController]
[VersionedApiBackOfficeRoute("audit-log")]
[ApiExplorerSettings(GroupName = "Audit Log")]
[ApiVersion("1.0")]
public class AuditLogControllerBase : ManagementApiControllerBase
{
}
