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
/// Controller responsible for handling requests to update stylesheet resources in the management API.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessStylesheets)]
public class UpdateStylesheetController : StylesheetControllerBase
{
    private readonly IStylesheetService _stylesheetService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateStylesheetController"/> class.
    /// </summary>
    /// <param name="stylesheetService">Service used to manage and update stylesheet entities.</param>
    /// <param name="umbracoMapper">The mapper used for converting between different Umbraco models.</param>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security features and context.</param>
    public UpdateStylesheetController(
        IStylesheetService stylesheetService,
        IUmbracoMapper umbracoMapper,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _stylesheetService = stylesheetService;
        _umbracoMapper = umbracoMapper;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Updates an existing stylesheet specified by its virtual path using the details provided in the request model.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="path">The virtual path identifying the stylesheet to update. This path is decoded and mapped to the system path internally.</param>
    /// <param name="requestModel">The model containing the updated stylesheet properties.</param>
    /// <returns>An <see cref="IActionResult"/> indicating whether the update was successful or providing error details if not found or invalid.</returns>
    [HttpPut("{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates a stylesheet.")]
    [EndpointDescription("Updates a stylesheet identified by the provided Id with the details from the request model.")]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        string path,
        UpdateStylesheetRequestModel requestModel)
    {
        path = DecodePath(path).VirtualPathToSystemPath();
        StylesheetUpdateModel updateModel = _umbracoMapper.Map<StylesheetUpdateModel>(requestModel)!;

        Attempt<IStylesheet?, StylesheetOperationStatus> updateAttempt = await _stylesheetService.UpdateAsync(path, updateModel, CurrentUserKey(_backOfficeSecurityAccessor));

        return updateAttempt.Success
            ? Ok()
            : StylesheetOperationStatusResult(updateAttempt.Status);
    }
}
