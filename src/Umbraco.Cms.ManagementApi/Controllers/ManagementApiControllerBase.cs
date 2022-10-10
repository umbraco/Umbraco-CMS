using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.ManagementApi.Filters;
using Umbraco.Cms.Web.Common.Attributes;

namespace Umbraco.Cms.ManagementApi.Controllers;

[UmbracoManagementApiController]
[ManagementApiJsonConfiguration]
public class ManagementApiControllerBase : Controller
{

}
