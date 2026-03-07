using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Language.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Language.Item;

    /// <summary>
    /// Provides API endpoints for managing individual language items within the Umbraco CMS.
    /// </summary>
[ApiVersion("1.0")]
public class ItemLanguageItemController : LanguageItemControllerBase
{
    private readonly ILanguageService _languageService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemLanguageItemController"/> class.
    /// </summary>
    /// <param name="languageService">Service used for language management operations.</param>
    /// <param name="mapper">The mapper used to map between Umbraco models and API models.</param>
    public ItemLanguageItemController(ILanguageService languageService, IUmbracoMapper mapper)
    {
        _languageService = languageService;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves a collection of language items corresponding to the specified ISO codes.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="isoCodes">A set of ISO codes representing the languages to retrieve.</param>
    /// <returns>An <see cref="IActionResult"/> containing a collection of <see cref="LanguageItemResponseModel"/> objects for the requested languages.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<LanguageItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of language items.")]
    [EndpointDescription("Gets a collection of language items identified by the provided Ids.")]
    public async Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "isoCode")] HashSet<string> isoCodes)
    {
        if (isoCodes.Count is 0)
        {
            return Ok(Enumerable.Empty<LanguageItemResponseModel>());
        }

        IEnumerable<ILanguage> languages = await _languageService.GetMultipleAsync(isoCodes);
        List<LanguageItemResponseModel> entityResponseModels = _mapper.MapEnumerable<ILanguage, LanguageItemResponseModel>(languages);
        return Ok(entityResponseModels);
    }
}
