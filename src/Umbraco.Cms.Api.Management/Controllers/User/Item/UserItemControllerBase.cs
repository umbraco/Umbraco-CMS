﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.User.Item;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/user")]
[ApiExplorerSettings(GroupName = "User")]
[Authorize(Policy = "New" + AuthorizationPolicies.SectionAccessUsers)]
public class UserItemControllerBase : ManagementApiControllerBase
{
}
