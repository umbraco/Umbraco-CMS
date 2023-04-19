﻿using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.StaticFile.Item;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute("static-file")]
[ApiExplorerSettings(GroupName = "Static File")]
public class StaticFileItemControllerBase : ManagementApiControllerBase
{
}
