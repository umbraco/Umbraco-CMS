﻿using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.User.Item;

[ApiController]
[VersionedApiBackOfficeRoute("users")]
[ApiExplorerSettings(GroupName = "Users")]
[ApiVersion("1.0")]
public class UserItemControllerBase : ManagementApiControllerBase
{
}
