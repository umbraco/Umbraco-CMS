using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Language;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Language;

    /// <summary>
    /// API controller responsible for handling operations related to all languages within the management interface.
    /// </summary>
[ApiVersion("1.0")]
public class AllLanguageController : LanguageControllerBase
{
    private readonly ILanguageService _languageService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllLanguageController"/> class, which manages operations related to all languages in the Umbraco CMS.
    /// </summary>
    /// <param name="languageService">The service used to manage and retrieve language information.</param>
    /// <param name="umbracoMapper">The mapper used to convert between Umbraco domain models and API models.</param>
    public AllLanguageController(ILanguageService languageService, IUmbracoMapper umbracoMapper)
    {
        _languageService = languageService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Retrieves a paginated list of all languages configured in the system.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="skip">The number of languages to skip before starting to collect the result set. Defaults to 0.</param>
    /// <param name="take">The maximum number of languages to return. Defaults to 100.</param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing a <see cref="PagedViewModel{LanguageResponseModel}"/> with the total count and the collection of language response models for the specified page.
    /// </returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<LanguageResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a paginated collection of languages.")]
    [EndpointDescription("Gets a paginated collection of all configured languages.")]
    public async Task<ActionResult<PagedViewModel<LanguageResponseModel>>> All(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
    {
        IEnumerable<ILanguage> result = await _languageService.GetAllAsync();
        ILanguage[] allLanguages = result.ToArray();
        var viewModel = new PagedViewModel<LanguageResponseModel>
        {
            Total = allLanguages.Length,
            Items = _umbracoMapper.MapEnumerable<ILanguage, LanguageResponseModel>(allLanguages.Skip(skip).Take(take))
        };

        return Ok(viewModel);
    }
}
