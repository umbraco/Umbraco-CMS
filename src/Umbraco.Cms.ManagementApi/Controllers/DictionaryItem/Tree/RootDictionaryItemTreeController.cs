﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Services.Paging;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;

namespace Umbraco.Cms.ManagementApi.Controllers.DictionaryItem.Tree;

public class RootDictionaryItemTreeController : DictionaryItemTreeControllerBase
{
    public RootDictionaryItemTreeController(IEntityService entityService, ILocalizationService localizationService)
        : base(entityService, localizationService)
    {
    }

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<EntityTreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<EntityTreeItemViewModel>>> Root(int skip = 0, int take = 100)
    {
        if (PaginationService.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize, out ProblemDetails? error) == false)
        {
            return BadRequest(error);
        }

        IDictionaryItem[] dictionaryItems = PaginatedDictionaryItems(
            pageNumber,
            pageSize,
            LocalizationService.GetRootDictionaryItems(),
            out var totalItems);

        EntityTreeItemViewModel[] viewModels = MapTreeItemViewModels(null, dictionaryItems);

        PagedViewModel<EntityTreeItemViewModel> result = PagedViewModel(viewModels, totalItems);
        return await Task.FromResult(Ok(result));
    }
}
