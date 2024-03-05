﻿using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Server;

[VersionedApiBackOfficeRoute("server")]
[ApiExplorerSettings(GroupName = "Server")]
public abstract class ServerControllerBase : ManagementApiControllerBase
{

}
