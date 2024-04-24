using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Language.Item;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.Language}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Language))]
[Authorize(Policy = AuthorizationPolicies.TreeAccessLanguages)]
public class LanguageItemControllerBase : ManagementApiControllerBase
{
}
