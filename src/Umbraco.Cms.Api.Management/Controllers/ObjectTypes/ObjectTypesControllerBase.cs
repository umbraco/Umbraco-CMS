﻿using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.ObjectTypes;

[ApiController]
[VersionedApiBackOfficeRoute("object-types")]
[ApiExplorerSettings(GroupName = "Object Types")]
public class ObjectTypesControllerBase : ManagementApiControllerBase
{
}
