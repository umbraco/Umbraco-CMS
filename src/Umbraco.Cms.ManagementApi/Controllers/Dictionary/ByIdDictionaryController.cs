using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Dictionary;

namespace Umbraco.Cms.ManagementApi.Controllers.Dictionary;

public class ByIdDictionaryController : DictionaryControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ByIdDictionaryController(ILocalizationService localizationService, IUmbracoMapper umbracoMapper)
    {
        _localizationService = localizationService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets a dictionary item by guid
    /// </summary>
    /// <param name="id">
    ///     The id.
    /// </param>
    /// <returns>
    ///     The <see cref="DictionaryDisplay" />. Returns a not found response when dictionary item does not exist
    /// </returns>
    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DictionaryViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DictionaryViewModel>> ById(Guid id)
    {
        IDictionaryItem? dictionary = _localizationService.GetDictionaryItemById(id);
        if (dictionary == null)
        {
            return NotFound();
        }

        return await Task.FromResult(_umbracoMapper.Map<IDictionaryItem, DictionaryViewModel>(dictionary)!);
    }
}
