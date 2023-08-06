using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.AuditLog;

[ApiController]
[VersionedApiBackOfficeRoute("audit-log")]
[ApiExplorerSettings(GroupName = "Audit Log")]
public class AuditLogControllerBase : ManagementApiControllerBase
{
}
