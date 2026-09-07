using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Routing;

namespace Umbraco.Cms.Search.Core.Controllers;

/// <summary>
/// Base class for the Search Management API controllers.
/// </summary>
[ApiController]
[BackOfficeRoute("search/api/v{version:apiVersion}")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
[MapToApi(Constants.Api.Name)]
[ApiExplorerSettings(GroupName = "Umbraco Search")]
public abstract class ApiControllerBase : ManagementApiControllerBase
{
}
