using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Folder;

/// <summary>
/// Provides API endpoints for managing member type folders identified by key.
/// </summary>
[ApiVersion("1.0")]
public class ByKeyMemberTypeFolderController : MemberTypeFolderControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyMemberTypeFolderController"/> class, which manages member type folders by their unique key.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security features for authorization and authentication.</param>
    /// <param name="memberTypeContainerService">Service used to manage member type containers.</param>
    public ByKeyMemberTypeFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMemberTypeContainerService memberTypeContainerService)
        : base(backOfficeSecurityAccessor, memberTypeContainerService)
    {
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(FolderResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a member type folder.")]
    [EndpointDescription("Gets a member type folder identified by the provided Id.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id) => await GetFolderAsync(id);
}
