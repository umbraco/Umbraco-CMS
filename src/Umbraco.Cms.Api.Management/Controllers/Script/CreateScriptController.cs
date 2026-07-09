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
/// Controller for creating scripts via the Umbraco CMS Management API.
/// </summary>
[ApiVersion("1.0")]
public class CreateScriptController : ScriptControllerBase
{
    private readonly IScriptService _scriptService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateScriptController"/> class.
    /// </summary>
    /// <param name="scriptService">Service for managing scripts.</param>
    /// <param name="umbracoMapper">Maps Umbraco objects to API models.</param>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security features.</param>
    public CreateScriptController(
        IScriptService scriptService,
        IUmbracoMapper umbracoMapper,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _scriptService = scriptService;
        _umbracoMapper = umbracoMapper;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Handles HTTP POST requests to create a new script using the details provided in the <paramref name="requestModel"/>.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="requestModel">The model containing the configuration and content of the script to create.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> that represents the result of the create operation:
    /// <list type="bullet">
    /// <item><description><c>201 Created</c> if the script is successfully created.</description></item>
    /// <item><description><c>400 Bad Request</c> if the request is invalid.</description></item>
    /// <item><description><c>404 Not Found</c> if a required resource is not found.</description></item>
    /// </list>
    /// </returns>
    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Creates a new script.")]
    [EndpointDescription("Creates a new script with the configuration specified in the request model.")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken, CreateScriptRequestModel requestModel)
    {
        ScriptCreateModel createModel = _umbracoMapper.Map<ScriptCreateModel>(requestModel)!;
        Attempt<IScript?, ScriptOperationStatus> createAttempt = await _scriptService.CreateAsync(createModel, CurrentUserKey(_backOfficeSecurityAccessor));

        return createAttempt.Success
            ? CreatedAtPath<ByPathScriptController>(controller => nameof(controller.ByPath), createAttempt.Result!.Path.SystemPathToVirtualPath())
            : ScriptOperationStatusResult(createAttempt.Status);
    }
}
