using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Language;

namespace Umbraco.Cms.ManagementApi.Controllers.Language;

public class ByIdLanguageController : LanguageControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ByIdLanguageController(ILocalizationService localizationService, IUmbracoMapper umbracoMapper)
    {
        _localizationService = localizationService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("{id:int}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<LanguageViewModel?>> ById(int id)
    {
        ILanguage? lang = _localizationService.GetLanguageById(id);
        if (lang is null)
        {
            return NotFound();
        }

        return await Task.FromResult(_umbracoMapper.Map<LanguageViewModel>(lang));
    }
}
