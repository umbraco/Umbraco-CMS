﻿using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Script.Item;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Item;

[ApiVersion("1.0")]
public class ItemScriptItemController : ScriptItemControllerBase
{
    private readonly IFileItemPresentationFactory _fileItemPresentationFactory;

    public ItemScriptItemController(IFileItemPresentationFactory fileItemPresentationFactory)
        => _fileItemPresentationFactory = fileItemPresentationFactory;

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<ScriptItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item([FromQuery(Name = "path")] HashSet<string> paths)
    {
        paths = paths.Select(path => path.VirtualPathToSystemPath()).ToHashSet();
        IEnumerable<ScriptItemResponseModel> responseModels = _fileItemPresentationFactory.CreateScriptItemResponseModels(paths);
        return await Task.FromResult(Ok(responseModels));
    }
}
