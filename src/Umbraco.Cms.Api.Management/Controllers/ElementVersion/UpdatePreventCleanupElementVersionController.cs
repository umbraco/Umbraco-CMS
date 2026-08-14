using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.ElementVersion;

/// <summary>
/// API controller responsible for updating the prevent-cleanup status of an element version.
/// </summary>
[ApiVersion("1.0")]
public class UpdatePreventCleanupElementVersionController : ElementVersionControllerBase
{
    private readonly IElementVersionService _elementVersionService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePreventCleanupElementVersionController"/> class.
    /// </summary>
    /// <param name="elementVersionService">Service for managing element versions.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    public UpdatePreventCleanupElementVersionController(
        IElementVersionService elementVersionService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _elementVersionService = elementVersionService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Sets the prevent-cleanup status for an element version.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the element version.</param>
    /// <param name="preventCleanup">Whether the version should be excluded from content history cleanup.</param>
    /// <returns>An <see cref="IActionResult"/> representing the outcome of the operation.</returns>
    [MapToApiVersion("1.0")]
    [HttpPut("{id:guid}/prevent-cleanup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Sets the prevent clean up status for an element version.")]
    [EndpointDescription("Sets the prevent clean up boolean status for an element version to the provided value. This controls whether the version will be a candidate for removal in content history clean up.")]
    public async Task<IActionResult> Set(CancellationToken cancellationToken, Guid id, bool preventCleanup)
    {
        Attempt<ContentVersionOperationStatus> attempt =
            await _elementVersionService.SetPreventCleanupAsync(id, preventCleanup, CurrentUserKey(_backOfficeSecurityAccessor));

        return attempt.Success
            ? Ok()
            : MapFailure(attempt.Result);
    }
}
