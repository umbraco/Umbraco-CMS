﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;

namespace Umbraco.Cms.ManagementApi.Controllers.Stylesheet.Tree;

public class RootStylesheetTreeController : StylesheetTreeControllerBase
{
    public RootStylesheetTreeController(FileSystems fileSystems)
        : base(fileSystems)
    {
    }

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<FileSystemTreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<FileSystemTreeItemViewModel>>> Root(int skip = 0, int take = 100)
        => await GetRoot(skip, take);
}
