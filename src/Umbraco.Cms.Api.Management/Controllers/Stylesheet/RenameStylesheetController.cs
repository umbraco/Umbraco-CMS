using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.Stylesheet;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet;

    /// <summary>
    /// Provides API endpoints for renaming stylesheet resources in the Umbraco CMS.
    /// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessStylesheets)]
public class RenameStylesheetController : StylesheetControllerBase
{
    private readonly IStylesheetService _stylesheetService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="RenameStylesheetController"/> class, which handles requests for renaming stylesheets in the Umbraco backoffice.
    /// </summary>
    /// <param name="stylesheetService">Service used to manage and manipulate stylesheet entities.</param>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security context for authorization and authentication.</param>
    /// <param name="umbracoMapper">The mapper used to convert between domain models and API models in Umbraco.</param>
    public RenameStylesheetController(IStylesheetService stylesheetService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor, IUmbracoMapper umbracoMapper)
    {
        _stylesheetService = stylesheetService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Renames an existing stylesheet file to a new specified name.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="path">The virtual path of the stylesheet to be renamed.</param>
    /// <param name="requestModel">The request model containing the new name for the stylesheet.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> that represents the result of the operation:
    /// <list type="bullet">
    /// <item><description><c>201 Created</c> if the stylesheet was successfully renamed.</description></item>
    /// <item><description><c>400 Bad Request</c> if the request is invalid.</description></item>
    /// <item><description><c>404 Not Found</c> if the stylesheet does not exist.</description></item>
    /// </list>
    /// </returns>
    [HttpPut("{path}/rename")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Renames a stylesheet.")]
    [EndpointDescription("Renames a stylesheet file to the specified new name.")]
    public async Task<IActionResult> Rename(
        CancellationToken cancellationToken,
        string path,
        RenameStylesheetRequestModel requestModel)
    {
        StylesheetRenameModel renameModel = _umbracoMapper.Map<StylesheetRenameModel>(requestModel)!;

        path = DecodePath(path).VirtualPathToSystemPath();
        Attempt<IStylesheet?, StylesheetOperationStatus> renameAttempt = await _stylesheetService.RenameAsync(path, renameModel, CurrentUserKey(_backOfficeSecurityAccessor));

        return renameAttempt.Success
            ? CreatedAtPath<ByPathStylesheetController>(controller => nameof(controller.ByPath), renameAttempt.Result!.Path.SystemPathToVirtualPath())
            : StylesheetOperationStatusResult(renameAttempt.Status);
    }
}
