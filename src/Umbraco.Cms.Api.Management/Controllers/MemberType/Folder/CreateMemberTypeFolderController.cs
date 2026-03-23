using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Folder;

/// <summary>
/// API controller responsible for handling requests to create folders for member types.
/// </summary>
[ApiVersion("1.0")]
public class CreateMemberTypeFolderController : MemberTypeFolderControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateMemberTypeFolderController"/> class, which handles API requests for creating member type folders.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security features.</param>
    /// <param name="memberTypeContainerService">Service for managing member type containers (folders).</param>
    public CreateMemberTypeFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMemberTypeContainerService memberTypeContainerService)
        : base(backOfficeSecurityAccessor, memberTypeContainerService)
    {
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Creates a member type folder.")]
    [EndpointDescription("Creates a new member type folder with the provided name and parent location.")]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken,
        CreateFolderRequestModel createFolderRequestModel)
        => await CreateFolderAsync<ByKeyMemberTypeFolderController>(
            createFolderRequestModel,
            controller => nameof(controller.ByKey)).ConfigureAwait(false);
}
