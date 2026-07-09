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
/// Provides API endpoints for renaming script files within the management system.
/// </summary>
[ApiVersion("1.0")]
public class RenameScriptController : ScriptControllerBase
{
    private readonly IScriptService _scriptService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Script.RenameScriptController"/> class.
    /// </summary>
    /// <param name="scriptService">Service used for managing script files within the Umbraco CMS.</param>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security operations and context.</param>
    /// <param name="umbracoMapper">Maps between different Umbraco models and view models.</param>
    public RenameScriptController(IScriptService scriptService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor, IUmbracoMapper umbracoMapper)
    {
        _scriptService = scriptService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Renames an existing script file to a new specified name.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="path">The virtual path of the script file to be renamed.</param>
    /// <param name="requestModel">The request model containing the new name for the script.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> that represents the result of the operation:
    /// <list type="bullet">
    /// <item><description><c>201 Created</c> if the script was successfully renamed.</description></item>
    /// <item><description><c>400 Bad Request</c> if the request is invalid.</description></item>
    /// <item><description><c>404 Not Found</c> if the script does not exist.</description></item>
    /// </list>
    /// </returns>
    [HttpPut("{path}/rename")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Renames a script.")]
    [EndpointDescription("Renames a script file to the specified new name.")]
    public async Task<IActionResult> Rename(
        CancellationToken cancellationToken,
        string path,
        RenameScriptRequestModel requestModel)
    {
        ScriptRenameModel renameModel = _umbracoMapper.Map<ScriptRenameModel>(requestModel)!;

        path = DecodePath(path).VirtualPathToSystemPath();
        Attempt<IScript?, ScriptOperationStatus> renameAttempt = await _scriptService.RenameAsync(path, renameModel, CurrentUserKey(_backOfficeSecurityAccessor));

        return renameAttempt.Success
            ? CreatedAtPath<ByPathScriptController>(controller => nameof(controller.ByPath), renameAttempt.Result!.Path.SystemPathToVirtualPath())
            : ScriptOperationStatusResult(renameAttempt.Status);
    }
}
