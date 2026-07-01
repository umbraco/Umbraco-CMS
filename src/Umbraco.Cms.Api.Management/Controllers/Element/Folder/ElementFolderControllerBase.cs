using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Element.Folder;

/// <summary>
/// Serves as the base controller for element folder management operations within the Umbraco CMS Management API.
/// Provides shared functionality for derived element folder controllers.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Element}/folder")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Element))]
[Authorize(Policy = AuthorizationPolicies.TreeAccessElements)]
public abstract class ElementFolderControllerBase : FolderManagementControllerBase<IElement>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElementFolderControllerBase"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="elementContainerService">Service for managing element containers.</param>
    protected ElementFolderControllerBase(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IElementContainerService elementContainerService)
        : base(backOfficeSecurityAccessor, elementContainerService)
    {
    }
}
