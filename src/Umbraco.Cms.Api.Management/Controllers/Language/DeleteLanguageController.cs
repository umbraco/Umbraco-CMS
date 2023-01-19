using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Language;

public class DeleteLanguageController : LanguageControllerBase
{
    private readonly ILocalizationService _localizationService;

    public DeleteLanguageController(ILocalizationService localizationService) => _localizationService = localizationService;

    [HttpDelete("{isoCode}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(string isoCode)
    {
        ILanguage? language = _localizationService.GetLanguageByIsoCode(isoCode);
        if (language == null)
        {
            return await Task.FromResult(NotFound());
        }

        Attempt<ILanguage, LanguageOperationStatus> result = _localizationService.Delete(language);

        if (result.Success)
        {
            return await Task.FromResult(Ok());
        }

        return LanguageOperationStatusResult(result.Status);
    }
}
