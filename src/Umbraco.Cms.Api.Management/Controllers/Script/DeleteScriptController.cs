using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Script;

/// <summary>
/// Controller responsible for handling requests to delete scripts in the management API.
/// </summary>
[ApiVersion("1.0")]
public class DeleteScriptController : ScriptControllerBase
{
    private readonly IScriptService _scriptService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Script.DeleteScriptController"/> class.
    /// </summary>
    /// <param name="scriptService">Service used for managing script files within the Umbraco CMS.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context, used to manage authentication and authorization for back office users.</param>
    public DeleteScriptController(
        IScriptService scriptService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _scriptService = scriptService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Deletes the script at the specified file path.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <param name="path">The virtual file path of the script to delete.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the outcome of the operation.</returns>
    [HttpDelete("{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Deletes a script.")]
    [EndpointDescription("Deletes a script identified by the provided file path.")]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, string path)
    {
        path = DecodePath(path).VirtualPathToSystemPath();
        ScriptOperationStatus operationStatus = await _scriptService.DeleteAsync(path, CurrentUserKey(_backOfficeSecurityAccessor));

        return operationStatus is ScriptOperationStatus.Success
            ? Ok()
            : ScriptOperationStatusResult(operationStatus);
    }
}
