﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Languages;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.New.Cms.Core.Models;

namespace Umbraco.Cms.ManagementApi.Controllers.Language;

[ApiVersion("1.0")]
public class GetAllLanguageController : LanguageControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly IUmbracoMapper _umbracoMapper;

    public GetAllLanguageController(ILocalizationService localizationService, IUmbracoMapper umbracoMapper)
    {
        _localizationService = localizationService;
        _umbracoMapper = umbracoMapper;
    }
    /// <summary>1
    ///     Returns all currently configured languages.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<LanguageViewModel>), StatusCodes.Status200OK)]
    public async Task<PagedViewModel<LanguageViewModel>?> GetAll(int skip, int take)
    {
        PagedModel<ILanguage> allLanguages = _localizationService.GetAllLanguagesPaged(skip, take);

        return _umbracoMapper.Map<PagedModel<ILanguage>, PagedViewModel<LanguageViewModel>>(allLanguages);

    }
}
