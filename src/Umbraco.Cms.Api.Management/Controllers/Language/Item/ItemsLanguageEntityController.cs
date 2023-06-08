using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Language.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Language.Item;

[ApiVersion("1.0")]
public class ItemsLanguageEntityController : LanguageEntityControllerBase
{
    private readonly ILanguageService _languageService;
    private readonly IUmbracoMapper _mapper;

    public ItemsLanguageEntityController(ILanguageService languageService, IUmbracoMapper mapper)
    {
        _languageService = languageService;
        _mapper = mapper;
    }

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<LanguageItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Items([FromQuery(Name = "isoCode")] HashSet<string> isoCodes)
    {
        IEnumerable<ILanguage> languages = await _languageService.GetMultipleAsync(isoCodes);
        List<LanguageItemResponseModel> entityResponseModels = _mapper.MapEnumerable<ILanguage, LanguageItemResponseModel>(languages);
        return Ok(entityResponseModels);
    }
}
