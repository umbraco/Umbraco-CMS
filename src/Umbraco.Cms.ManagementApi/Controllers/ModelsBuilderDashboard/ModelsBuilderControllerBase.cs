﻿using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.ModelsBuilderDashboard;

[ApiController]
[BackOfficeRoute("api/v{version:apiVersion}/modelsBuilder")]
[OpenApiTag("ModelsBuilder")]
[ApiVersion("1.0")]
public class ModelsBuilderControllerBase : ManagementApiControllerBase
{
}
