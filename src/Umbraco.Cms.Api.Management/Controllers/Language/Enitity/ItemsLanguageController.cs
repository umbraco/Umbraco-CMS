using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Entity;
using Umbraco.Cms.Api.Management.ViewModels.Entity;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Language.Enitity;

public class LanguageItemsEntityController : EntityControllerBase
{
    private readonly ILanguageService _languageService;
    private readonly IUmbracoMapper _mapper;

    public LanguageItemsEntityController(ILanguageService languageService, IUmbracoMapper mapper)
    {
        _languageService = languageService;
        _mapper = mapper;
    }

    [HttpGet("items")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<EntityResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Items()
    {
        IEnumerable<ILanguage> languages = await _languageService.GetAllAsync();
        List<EntityResponseModel> entityResponseModels = _mapper.MapEnumerable<ILanguage, EntityResponseModel>(languages);
        return Ok(entityResponseModels);
    }
}
