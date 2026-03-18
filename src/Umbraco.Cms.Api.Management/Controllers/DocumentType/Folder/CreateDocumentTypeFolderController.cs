using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType.Folder;

/// <summary>
/// API controller responsible for handling requests to create new document type folders in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class CreateDocumentTypeFolderController : DocumentTypeFolderControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateDocumentTypeFolderController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Accessor for the back office security context.</param>
    /// <param name="contentTypeContainerService">The service used to manage content type containers.</param>
    public CreateDocumentTypeFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentTypeContainerService contentTypeContainerService)
        : base(backOfficeSecurityAccessor, contentTypeContainerService)
    {
    }

    /// <summary>
    /// Creates a new document type folder at the specified parent location.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <param name="createFolderRequestModel">The request model containing folder details, such as name and parent location.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Creates a document type folder.")]
    [EndpointDescription("Creates a new document type folder with the provided name and parent location.")]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken,
        CreateFolderRequestModel createFolderRequestModel)
        => await CreateFolderAsync<ByKeyDocumentTypeFolderController>(
            createFolderRequestModel,
            controller => nameof(controller.ByKey)).ConfigureAwait(false);
}
