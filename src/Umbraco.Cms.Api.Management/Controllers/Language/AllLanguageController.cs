using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Language;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Language;

[ApiVersion("1.0")]
public class AllLanguageController : LanguageControllerBase
{
    private readonly ILanguageService _languageService;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllLanguageController(ILanguageService languageService, IUmbracoMapper umbracoMapper)
    {
        _languageService = languageService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<LanguageResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<LanguageResponseModel>>> All(int skip = 0, int take = 100)
    {
        ILanguage[] allLanguages = (await _languageService.GetAllAsync()).ToArray();
        var viewModel = new PagedViewModel<LanguageResponseModel>
        {
            Total = allLanguages.Length,
            Items = _umbracoMapper.MapEnumerable<ILanguage, LanguageResponseModel>(allLanguages.Skip(skip).Take(take))
        };

        return await Task.FromResult(Ok(viewModel));
    }
}
