﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary;

public class ByKeyDictionaryController : DictionaryControllerBase
{
    private readonly IDictionaryItemService _dictionaryItemService;
    private readonly IDictionaryPresentationFactory _dictionaryPresentationFactory;

    public ByKeyDictionaryController(IDictionaryItemService dictionaryItemService, IDictionaryPresentationFactory dictionaryPresentationFactory)
    {
        _dictionaryItemService = dictionaryItemService;
        _dictionaryPresentationFactory = dictionaryPresentationFactory;
    }

    [HttpGet($"{{{nameof(key)}:guid}}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DictionaryItemResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DictionaryItemResponseModel>> ByKey(Guid key)
    {
        IDictionaryItem? dictionary = await _dictionaryItemService.GetAsync(key);
        if (dictionary == null)
        {
            return NotFound();
        }

        return Ok(await _dictionaryPresentationFactory.CreateDictionaryItemViewModelAsync(dictionary));
    }
}
