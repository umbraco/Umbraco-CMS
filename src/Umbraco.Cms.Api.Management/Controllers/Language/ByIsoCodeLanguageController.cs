using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Language;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Language;

[ApiVersion("1.0")]
public class ByIsoCodeLanguageController : LanguageControllerBase
{
    private readonly ILanguageService _languageService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ByIsoCodeLanguageController(ILanguageService languageService, IUmbracoMapper umbracoMapper)
    {
        _languageService = languageService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet($"{{{nameof(isoCode)}}}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(LanguageResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> ByIsoCode(CancellationToken cancellationToken, string isoCode)
    {
        ILanguage? language = await _languageService.GetAsync(isoCode);
        if (language == null)
        {
            return LanguageNotFound();
        }

        return Ok(_umbracoMapper.Map<LanguageResponseModel>(language)!);
    }
}
