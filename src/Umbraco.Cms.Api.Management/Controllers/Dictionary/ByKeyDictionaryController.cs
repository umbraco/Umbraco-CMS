using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary;

[ApiVersion("1.0")]
public class ByKeyDictionaryController : DictionaryControllerBase
{
    private readonly IDictionaryItemService _dictionaryItemService;
    private readonly IDictionaryPresentationFactory _dictionaryPresentationFactory;

    public ByKeyDictionaryController(IDictionaryItemService dictionaryItemService, IDictionaryPresentationFactory dictionaryPresentationFactory)
    {
        _dictionaryItemService = dictionaryItemService;
        _dictionaryPresentationFactory = dictionaryPresentationFactory;
    }

    [HttpGet($"{{{nameof(id)}:guid}}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DictionaryItemResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        IDictionaryItem? dictionary = await _dictionaryItemService.GetAsync(id);
        if (dictionary == null)
        {
            return DictionaryItemNotFound();
        }

        return Ok(await _dictionaryPresentationFactory.CreateDictionaryItemViewModelAsync(dictionary));
    }
}
