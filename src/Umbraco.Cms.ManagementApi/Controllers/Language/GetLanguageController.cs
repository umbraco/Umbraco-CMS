using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Languages;

namespace Umbraco.Cms.ManagementApi.Controllers.Language;

public class GetLanguageController : LanguageControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly UmbracoMapper _umbracoMapper;

    public GetLanguageController(ILocalizationService localizationService, UmbracoMapper umbracoMapper)
    {
        _localizationService = localizationService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("/{id:int}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<LanguageViewModel?>> GetLanguage(int id)
    {
        ILanguage? lang = _localizationService.GetLanguageById(id);
        if (lang == null)
        {
            return NotFound();
        }

        return _umbracoMapper.Map<LanguageViewModel>(lang);
    }
}
