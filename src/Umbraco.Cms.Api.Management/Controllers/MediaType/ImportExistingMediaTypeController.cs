using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.ImportExport;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

/// <summary>
/// API controller responsible for importing definitions of existing media types in the system.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
public class ImportExistingMediaTypeController : MediaTypeControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IMediaTypeImportService _mediaTypeImportService;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.MediaType.ImportExistingMediaTypeController"/> class,
    /// providing dependencies for back office security and media type import operations.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security context and authentication information.</param>
    /// <param name="mediaTypeImportService">Service responsible for handling media type import functionality.</param>
    public ImportExistingMediaTypeController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMediaTypeImportService mediaTypeImportService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _mediaTypeImportService = mediaTypeImportService;
    }

    [HttpPut("{id:guid}/import")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Imports a media type.")]
    [EndpointDescription("Imports a media type from the provided file upload.")]
    public async Task<IActionResult> Import(
        CancellationToken cancellationToken,
        Guid id,
        ImportMediaTypeRequestModel model)
    {
        Attempt<IMediaType?, MediaTypeImportOperationStatus> importAttempt = await _mediaTypeImportService.Import(model.File.Id, CurrentUserKey(_backOfficeSecurityAccessor));

        return importAttempt.Success is false
            ? MediaTypeImportOperationStatusResult(importAttempt.Status)
            : Ok();
    }
}
