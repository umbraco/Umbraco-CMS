using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Template.Item;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Template}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Template))]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessTemplates)]
public class TemplateItemControllerBase : ManagementApiControllerBase
{

}
