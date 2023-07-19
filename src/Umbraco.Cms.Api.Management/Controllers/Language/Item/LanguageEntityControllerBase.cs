using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Language.Item;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Language}")]
[ApiExplorerSettings(GroupName = "Language")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessLanguages)]
public class LanguageEntityControllerBase : ManagementApiControllerBase
{
}
