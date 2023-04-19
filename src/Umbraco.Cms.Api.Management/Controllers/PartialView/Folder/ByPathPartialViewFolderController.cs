using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Folder;

public class ByPathPartialViewFolderController : PartialViewFolderBaseController
{
    public ByPathPartialViewFolderController(IUmbracoMapper mapper, IPartialViewFolderService partialViewFolderService) : base(mapper, partialViewFolderService)
    {
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    public Task<IActionResult> Get(string path) => GetFolderAsync(path);
}
