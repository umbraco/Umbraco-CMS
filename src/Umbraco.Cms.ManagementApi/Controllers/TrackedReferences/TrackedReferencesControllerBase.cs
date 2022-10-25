﻿using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.TrackedReferences;

[ApiController]
[VersionedApiBackOfficeRoute("tracked-reference")]
[OpenApiTag("Tracked Reference")]
[ApiVersion("1.0")]
public abstract class TrackedReferencesControllerBase : ManagementApiControllerBase
{
}
