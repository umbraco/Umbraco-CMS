using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Folder;

/// <summary>
/// API controller responsible for handling requests to delete member type folders in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class DeleteMemberTypeFolderController : MemberTypeFolderControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteMemberTypeFolderController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security operations.</param>
    /// <param name="memberTypeContainerService">Service for managing member type containers.</param>
    public DeleteMemberTypeFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMemberTypeContainerService memberTypeContainerService)
        : base(backOfficeSecurityAccessor, memberTypeContainerService)
    {
    }

    [HttpDelete("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Deletes a member type folder.")]
    [EndpointDescription("Deletes a member type folder identified by the provided Id.")]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, Guid id) => await DeleteFolderAsync(id);
}
