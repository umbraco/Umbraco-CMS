﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Member.Item;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.Member}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Member))]
[Authorize(Policy = AuthorizationPolicies.SectionAccessForMemberTree)]
public class MemberItemControllerBase : ManagementApiControllerBase
{
}
