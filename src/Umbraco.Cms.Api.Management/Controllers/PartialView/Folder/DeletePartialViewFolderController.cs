using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Folder;

[ApiVersion("1.0")]
public class DeletePartialViewFolderController : PartialViewFolderControllerBase
{
    public DeletePartialViewFolderController(
        IUmbracoMapper mapper,
        IPartialViewFolderService partialViewFolderService)
        : base(mapper, partialViewFolderService)
    {
    }

    [HttpDelete]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<IActionResult> Delete(string path) => DeleteAsync(path);
}
