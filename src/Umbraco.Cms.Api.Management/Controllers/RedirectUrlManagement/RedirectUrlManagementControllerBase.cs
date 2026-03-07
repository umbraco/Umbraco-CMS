using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.RedirectUrlManagement;

    /// <summary>
    /// Serves as the base controller for managing redirect URLs in the Umbraco CMS Management API.
    /// Provides common functionality for redirect URL management endpoints.
    /// </summary>
[VersionedApiBackOfficeRoute("redirect-management")]
[ApiExplorerSettings(GroupName = "Redirect Management")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]
public class RedirectUrlManagementControllerBase : ManagementApiControllerBase
{
}
