using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType.Folder;

[ApiVersion("1.0")]
public class ByKeyDocumentTypeFolderController : DocumentTypeFolderControllerBase
{
    public ByKeyDocumentTypeFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentTypeContainerService contentTypeContainerService)
        : base(backOfficeSecurityAccessor, contentTypeContainerService)
    {
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(FolderResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id) => await GetFolderAsync(id);
}
