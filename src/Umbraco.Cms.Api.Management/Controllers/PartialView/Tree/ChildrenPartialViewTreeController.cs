﻿using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Tree;

[ApiVersion("1.0")]
public class ChildrenPartialViewTreeController : PartialViewTreeControllerBase
{
    public ChildrenPartialViewTreeController(FileSystems fileSystems)
        : base(fileSystems)
    {
    }

    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<FileSystemTreeItemPresentationModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<FileSystemTreeItemPresentationModel>>> Children(string parentPath, int skip = 0, int take = 100)
        => await GetChildren(parentPath, skip, take);
}
