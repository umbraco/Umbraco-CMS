using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Language.Item;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.Language}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Language))]
public class LanguageItemControllerBase : ManagementApiControllerBase
{
}
