using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.PartialView;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView;

    /// <summary>
    /// Handles requests to rename partial views.
    /// </summary>
[ApiVersion("1.0")]
public class RenamePartialViewController : PartialViewControllerBase
{
    private readonly IPartialViewService _partialViewService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="RenamePartialViewController"/> class.
    /// </summary>
    /// <param name="partialViewService">Service used to manage and manipulate partial views.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context and authentication.</param>
    /// <param name="umbracoMapper">Mapper used to convert between Umbraco domain models and API models.</param>
    public RenamePartialViewController(IPartialViewService partialViewService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor, IUmbracoMapper umbracoMapper)
    {
        _partialViewService = partialViewService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Renames an existing partial view file to a new specified name.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="path">The virtual path of the partial view to rename.</param>
    /// <param name="requestModel">The request model containing the new name for the partial view.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the operation:
    /// <list type="bullet">
    /// <item><description><c>201 Created</c> if the rename was successful.</description></item>
    /// <item><description><c>400 Bad Request</c> if the request is invalid.</description></item>
    /// <item><description><c>404 Not Found</c> if the partial view does not exist.</description></item>
    /// </list>
    /// </returns>
    [HttpPut("{path}/rename")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Renames a partial view.")]
    [EndpointDescription("Renames a partial view file to the specified new name.")]
    public async Task<IActionResult> Rename(
        CancellationToken cancellationToken,
        string path,
        RenamePartialViewRequestModel requestModel)
    {
        PartialViewRenameModel renameModel = _umbracoMapper.Map<PartialViewRenameModel>(requestModel)!;

        path = DecodePath(path).VirtualPathToSystemPath();
        Attempt<IPartialView?, PartialViewOperationStatus> renameAttempt = await _partialViewService.RenameAsync(path, renameModel, CurrentUserKey(_backOfficeSecurityAccessor));

        return renameAttempt.Success
            ? CreatedAtPath<ByPathPartialViewController>(controller => nameof(controller.ByPath), renameAttempt.Result!.Path.SystemPathToVirtualPath())
            : PartialViewOperationStatusResult(renameAttempt.Status);
    }
}
