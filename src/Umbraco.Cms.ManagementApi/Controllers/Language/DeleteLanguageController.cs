using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Builders;

namespace Umbraco.Cms.ManagementApi.Controllers.Language;

public class DeleteLanguageController : LanguageControllerBase
{
    private readonly ILocalizationService _localizationService;

    public DeleteLanguageController(ILocalizationService localizationService) => _localizationService = localizationService;

    /// <summary>
    ///     Deletes a language with a given ID
    /// </summary>
    [HttpDelete("{id:int}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    // TODO: This needs to be an authorized endpoint.
    public async Task<IActionResult> Delete(int id)
    {
        ILanguage? language = _localizationService.GetLanguageById(id);
        if (language == null)
        {
            return await Task.FromResult(NotFound());
        }

        // the service would not let us do it, but test here nevertheless
        if (language.IsDefault)
        {
            ProblemDetails invalidModelProblem =
                new ProblemDetailsBuilder()
                    .WithTitle("Cannot delete default language")
                    .WithDetail($"Language '{language.IsoCode}' is currently set to 'default' and can not be deleted.")
                    .Build();

            return BadRequest(invalidModelProblem);
        }

        // service is happy deleting a language that's fallback for another language,
        // will just remove it - so no need to check here
        _localizationService.Delete(language);

        return await Task.FromResult(Ok());
    }
}
