using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Language;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Language;

/// <summary>
/// Controller for managing language operations by ISO code.
/// </summary>
[ApiVersion("1.0")]
public class ByIsoCodeLanguageController : LanguageControllerBase
{
    private readonly ILanguageService _languageService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Language.ByIsoCodeLanguageController"/> class.
    /// </summary>
    /// <param name="languageService">Service used to manage and retrieve language information.</param>
    /// <param name="umbracoMapper">The mapper used to convert between Umbraco domain models and API models.</param>
    public ByIsoCodeLanguageController(ILanguageService languageService, IUmbracoMapper umbracoMapper)
    {
        _languageService = languageService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>Gets a language identified by the provided ISO code.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="isoCode">The ISO code of the language to retrieve.</param>
    /// <returns>An <see cref="IActionResult"/> containing the language data if found; otherwise, a 404 Not Found response.</returns>
    [HttpGet($"{{{nameof(isoCode)}}}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(LanguageResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a language by ISO code.")]
    [EndpointDescription("Gets a language identified by the provided ISO code.")]
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
