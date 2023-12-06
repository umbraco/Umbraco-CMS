﻿using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Services.Paging;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary.Tree;

[ApiVersion("1.0")]
public class ChildrenDictionaryTreeController : DictionaryTreeControllerBase
{
    public ChildrenDictionaryTreeController(IEntityService entityService, IDictionaryItemService dictionaryItemService)
        : base(entityService, dictionaryItemService)
    {
    }

    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<EntityTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<EntityTreeItemResponseModel>>> Children(Guid parentId, int skip = 0, int take = 100)
    {
        if (PaginationService.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize, out ProblemDetails? error) == false)
        {
            return BadRequest(error);
        }

        PagedModel<IDictionaryItem> dictionaryItems = await PaginatedDictionaryItems(
            pageNumber,
            pageSize,
            parentId);

        EntityTreeItemResponseModel[] viewModels = await MapTreeItemViewModels(parentId, dictionaryItems.Items.ToArray());

        PagedViewModel<EntityTreeItemResponseModel> result = PagedViewModel(viewModels, dictionaryItems.Total);
        return await Task.FromResult(Ok(result));
    }
}
