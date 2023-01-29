﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Language;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Language;

public class ByIsoCodeLanguageController : LanguageControllerBase
{
    private readonly ILanguageService _languageService;
    private readonly ILanguageFactory _languageFactory;

    public ByIsoCodeLanguageController(ILanguageService languageService, ILanguageFactory languageFactory)
    {
        _languageService = languageService;
        _languageFactory = languageFactory;
    }

    [HttpGet($"{{{nameof(isoCode)}}}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(LanguageViewModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<LanguageViewModel>> ByIsoCode(string isoCode)
    {
        ILanguage? language = await _languageService.GetAsync(isoCode);
        if (language == null)
        {
            return NotFound();
        }

        return Ok(_languageFactory.CreateLanguageViewModel(language));
    }
}
