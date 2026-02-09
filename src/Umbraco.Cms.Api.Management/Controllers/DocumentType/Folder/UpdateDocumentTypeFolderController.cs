using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType.Folder;

[ApiVersion("1.0")]
public class UpdateDocumentTypeFolderController : DocumentTypeFolderControllerBase
{
    public UpdateDocumentTypeFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentTypeContainerService contentTypeContainerService)
        : base(backOfficeSecurityAccessor, contentTypeContainerService)
    {
    }

    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates a document type folder.")]
    [EndpointDescription("Updates a document type folder identified by the provided Id with the details from the request model.")]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        Guid id,
        UpdateFolderResponseModel updateFolderResponseModel)
        => await UpdateFolderAsync(id, updateFolderResponseModel);
}
