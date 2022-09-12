﻿using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Language;

[ApiController]
[BackOfficeRoute("api/v{version:apiVersion}/language")]
[OpenApiTag("Language")]
public abstract class LanguageControllerBase : Controller
{
}
