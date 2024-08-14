using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Folder;

[ApiVersion("1.0")]
public class DeleteMemberTypeFolderController : MemberTypeFolderControllerBase
{
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
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, Guid id) => await DeleteFolderAsync(id);
}
