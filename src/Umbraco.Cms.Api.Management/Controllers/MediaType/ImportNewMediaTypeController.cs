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
/// Provides API endpoints for importing new media types into the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
public class ImportNewMediaTypeController : MediaTypeControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IMediaTypeImportService _mediaTypeImportService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportNewMediaTypeController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">An accessor for back office security, implementing <see cref="IBackOfficeSecurityAccessor"/>.</param>
    /// <param name="mediaTypeImportService">A service for importing media types, implementing <see cref="IMediaTypeImportService"/>.</param>
    public ImportNewMediaTypeController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMediaTypeImportService mediaTypeImportService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _mediaTypeImportService = mediaTypeImportService;
    }

    /// <summary>
    /// Imports a new media type from the provided file upload.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the import operation.</param>
    /// <param name="model">The <see cref="ImportMediaTypeRequestModel"/> containing the file information for the media type to import.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> that represents the result of the import operation.
    /// Returns <c>201 Created</c> with the imported media type's key if successful,
    /// <c>400 Bad Request</c> if the request is invalid, or <c>404 Not Found</c> if the file does not exist.
    /// </returns>
    [HttpPost("import")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Imports a media type.")]
    [EndpointDescription("Imports a media type from the provided file upload.")]
    public async Task<IActionResult> Import(
        CancellationToken cancellationToken,
        ImportMediaTypeRequestModel model)
    {
        Attempt<IMediaType?, MediaTypeImportOperationStatus> importAttempt = await _mediaTypeImportService.Import(model.File.Id, CurrentUserKey(_backOfficeSecurityAccessor));

        return importAttempt.Success is false
            ? MediaTypeImportOperationStatusResult(importAttempt.Status)
            : CreatedAtId<ByKeyMediaTypeController>(controller => nameof(controller.ByKey), importAttempt.Result!.Key);
    }
}
