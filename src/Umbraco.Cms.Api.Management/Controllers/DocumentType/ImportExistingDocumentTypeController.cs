using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.ImportExport;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

/// <summary>
/// API controller responsible for handling the import of existing document types in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class ImportExistingDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IContentTypeImportService _contentTypeImportService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportExistingDocumentTypeController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security, used to manage authentication and authorization for the controller.</param>
    /// <param name="contentTypeImportService">Service responsible for importing content types into the system.</param>
    public ImportExistingDocumentTypeController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentTypeImportService contentTypeImportService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _contentTypeImportService = contentTypeImportService;
    }

    [HttpPut("{id:guid}/import")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Imports a document type.")]
    [EndpointDescription("Imports a document type from the provided file upload.")]
    public async Task<IActionResult> Import(
        CancellationToken cancellationToken,
        Guid id,
        ImportDocumentTypeRequestModel model)
    {
        Attempt<IContentType?, ContentTypeImportOperationStatus> importAttempt = await _contentTypeImportService.Import(model.File.Id, CurrentUserKey(_backOfficeSecurityAccessor), id);

        return importAttempt.Success is false
            ? ContentTypeImportOperationStatusResult(importAttempt.Status)
            : Ok();
    }
}
