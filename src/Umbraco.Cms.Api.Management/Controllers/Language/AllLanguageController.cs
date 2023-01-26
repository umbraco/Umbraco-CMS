using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Language;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Language;

public class AllLanguageController : LanguageControllerBase
{
    private readonly ILanguageService _languageService;
    private readonly ILanguageFactory _languageFactory;

    public AllLanguageController(ILanguageService languageService, ILanguageFactory languageFactory)
    {
        _languageService = languageService;
        _languageFactory = languageFactory;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<LanguageViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<LanguageViewModel>>> All(int skip = 0, int take = 100)
    {
        ILanguage[] allLanguages = (await _languageService.GetAllAsync()).ToArray();
        var viewModel = new PagedViewModel<LanguageViewModel>
        {
            Total = allLanguages.Length,
            Items = allLanguages.Skip(skip).Take(take).Select(_languageFactory.CreateLanguageViewModel).ToArray()
        };

        return await Task.FromResult(Ok(viewModel));
    }
}
