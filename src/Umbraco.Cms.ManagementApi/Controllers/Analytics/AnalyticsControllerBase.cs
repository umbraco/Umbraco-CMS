﻿using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Analytics;

[ApiController]
[VersionedApiBackOfficeRoute("telemetry")]
[OpenApiTag("Telemetry")]
[ApiVersion("1.0")]
public abstract class AnalyticsControllerBase : ManagementApiControllerBase
{
}
