﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Tree;

public class ItemsStylesheetTreeController : StylesheetTreeControllerBase
{
    public ItemsStylesheetTreeController(FileSystems fileSystems)
        : base(fileSystems)
    {
    }

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<FileSystemTreeItemPresentationModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FileSystemTreeItemPresentationModel>>> Items([FromQuery(Name = "path")] string[] paths)
        => await GetItems(paths);
}
