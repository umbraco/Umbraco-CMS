using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Folder;

public class DeletePartialViewFolderController : PartialViewFolderBaseController
{
    public DeletePartialViewFolderController(
        IUmbracoMapper mapper,
        IPartialViewFolderService partialViewFolderService)
        : base(mapper, partialViewFolderService)
    {
    }

    [HttpDelete]
    [MapToApiVersion("1.0")]
    public Task<IActionResult> Delete(string path) => DeleteAsync(path);
}
