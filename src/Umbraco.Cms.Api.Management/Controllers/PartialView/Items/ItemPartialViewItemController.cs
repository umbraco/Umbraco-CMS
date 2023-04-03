﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.MemberType.Items;
using Umbraco.Cms.Api.Management.ViewModels.PartialView.Item;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Items;

public class ItemPartialViewItemController : PartialViewItemControllerBase
{
    private readonly IFileItemPresentationModelFactory _fileItemPresentationModelFactory;

    public ItemPartialViewItemController(IFileItemPresentationModelFactory fileItemPresentationModelFactory) => _fileItemPresentationModelFactory = fileItemPresentationModelFactory;

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<PartialViewItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item([FromQuery(Name = "key")] string[] paths)
    {
        IEnumerable<PartialViewItemResponseModel> responseModels = _fileItemPresentationModelFactory.CreatePartialViewResponseModels(paths);
        return Ok(responseModels);
    }
}
