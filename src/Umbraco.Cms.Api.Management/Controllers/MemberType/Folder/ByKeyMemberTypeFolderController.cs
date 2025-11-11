using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Folder;

[ApiVersion("1.0")]
public class ByKeyMemberTypeFolderController : MemberTypeFolderControllerBase
{
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
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id) => await GetFolderAsync(id);
}
