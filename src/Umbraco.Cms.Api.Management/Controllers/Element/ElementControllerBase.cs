using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Content;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

[VersionedApiBackOfficeRoute(Constants.UdiEntityType.Element)]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Element))]
// TODO ELEMENTS: backoffice authorization policies
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocuments)]
public class ElementControllerBase : ContentControllerBase
{
}
