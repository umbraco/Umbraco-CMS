using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.ViewModels.Language;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.New.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.Language;

public class AllLanguageController : LanguageControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllLanguageController(ILocalizationService localizationService, IUmbracoMapper umbracoMapper)
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
    [ProducesResponseType(typeof(PagedViewModel<LanguageViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<LanguageViewModel>>> GetAll(int skip, int take)
    {
        PagedModel<ILanguage> allLanguages = _localizationService.GetAllLanguagesPaged(skip, take);

        return await Task.FromResult(_umbracoMapper.Map<PagedModel<ILanguage>, PagedViewModel<LanguageViewModel>>(allLanguages)!);
    }
}
