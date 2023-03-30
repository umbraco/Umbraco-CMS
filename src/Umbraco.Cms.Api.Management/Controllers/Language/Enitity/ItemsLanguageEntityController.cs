using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Language.Entity;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Language.Enitity;

public class ItemsLanguageEntityController : LanguageEntityControllerBase
{
    private readonly ILanguageService _languageService;
    private readonly IUmbracoMapper _mapper;

    public ItemsLanguageEntityController(ILanguageService languageService, IUmbracoMapper mapper)
    {
        _languageService = languageService;
        _mapper = mapper;
    }

    [HttpGet("items")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<LanguageEntityResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Items()
    {
        IEnumerable<ILanguage> languages = await _languageService.GetAllAsync();
        List<LanguageEntityResponseModel> entityResponseModels = _mapper.MapEnumerable<ILanguage, LanguageEntityResponseModel>(languages);
        return Ok(entityResponseModels);
    }
}
