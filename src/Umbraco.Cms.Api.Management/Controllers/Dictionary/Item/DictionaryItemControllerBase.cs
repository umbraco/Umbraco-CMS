using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary.Item;

[ApiController]
[VersionedApiBackOfficeRoute("dictionary")]
[ApiExplorerSettings(GroupName = "Dictionary")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessDictionary)]
public class DictionaryItemControllerBase : ManagementApiControllerBase
{
}
