using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

/// <summary>
/// API controller responsible for handling requests to delete document types in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class DeleteDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteDocumentTypeController"/> class.
    /// </summary>
    /// <param name="contentTypeService">Service used to manage content types.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security operations.</param>
    public DeleteDocumentTypeController(IContentTypeService contentTypeService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _contentTypeService = contentTypeService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Deletes a document type identified by the provided Id.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="id">The unique identifier of the document type to delete.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the delete operation.</returns>
    [HttpDelete("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Deletes a document type.")]
    [EndpointDescription("Deletes a document type identified by the provided Id.")]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, Guid id)
    {
        ContentTypeOperationStatus status = await _contentTypeService.DeleteAsync(id, CurrentUserKey(_backOfficeSecurityAccessor));
        return OperationStatusResult(status);
    }
}
