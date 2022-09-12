﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.ManagementApi.Controllers.Language;

[ApiVersion("1.0")]
public class DeleteLanguageController : LanguageControllerBase
{
    private readonly ILocalizationService _localizationService;

    public DeleteLanguageController(ILocalizationService localizationService) => _localizationService = localizationService;

    /// <summary>
    ///     Deletes a language with a given ID
    /// </summary>
    [HttpDelete("delete/{id:int}")]
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
            return NotFound();
        }

        // the service would not let us do it, but test here nevertheless
        if (language.IsDefault)
        {
            var invalidModelProblem = new ProblemDetails
            {
                Title = "Cannot delete default language",
                Detail = $"Language '{language.IsoCode}' is currently set to 'default' and can not be deleted.",
                Status = StatusCodes.Status400BadRequest,
                Type = "Error",
            };
            return BadRequest(invalidModelProblem);
        }

        // service is happy deleting a language that's fallback for another language,
        // will just remove it - so no need to check here
        _localizationService.Delete(language);

        return Ok();
    }
}
