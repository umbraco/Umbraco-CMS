using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.Script;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Script;

/// <summary>
/// API controller responsible for handling operations related to updating scripts in the system.
/// </summary>
[ApiVersion("1.0")]
public class UpdateScriptController : ScriptControllerBase
{
    private readonly IScriptService _scriptService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

/// <summary>
/// Initializes a new instance of the <see cref="UpdateScriptController"/> class, which handles API requests for updating scripts in Umbraco.
/// </summary>
/// <param name="scriptService">Service used for script management operations.</param>
/// <param name="umbracoMapper">The mapper used to map between Umbraco models and API models.</param>
/// <param name="backOfficeSecurityAccessor">Provides access to back office security context.</param>
    public UpdateScriptController(
        IScriptService scriptService,
        IUmbracoMapper umbracoMapper,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _scriptService = scriptService;
        _umbracoMapper = umbracoMapper;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Updates an existing script identified by the specified path using the details provided in the request model.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <param name="path">The virtual path identifying the script to update.</param>
    /// <param name="requestModel">The model containing the updated script content and metadata.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the outcome of the update operation, including success or relevant error details.</returns
    [HttpPut("{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates a script.")]
    [EndpointDescription("Updates a script identified by the provided Id with the details from the request model.")]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        string path,
        UpdateScriptRequestModel requestModel)
    {
        path = DecodePath(path).VirtualPathToSystemPath();
        ScriptUpdateModel updateModel = _umbracoMapper.Map<ScriptUpdateModel>(requestModel)!;

        Attempt<IScript?, ScriptOperationStatus> updateAttempt = await _scriptService.UpdateAsync(path, updateModel, CurrentUserKey(_backOfficeSecurityAccessor));

        return updateAttempt.Success
            ? Ok()
            : ScriptOperationStatusResult(updateAttempt.Status);
    }
}
