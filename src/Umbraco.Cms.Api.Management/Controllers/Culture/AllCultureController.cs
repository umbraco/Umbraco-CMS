using System.Globalization;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Api.Management.ViewModels.Culture;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Culture;

/// <summary>
/// API controller responsible for retrieving and managing culture information in the system.
/// </summary>
[ApiVersion("1.0")]
public class AllCultureController : CultureControllerBase
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly ICultureService _cultureService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllCultureController"/> class with the specified Umbraco mapper and culture service.
    /// </summary>
    /// <param name="umbracoMapper">An instance of <see cref="IUmbracoMapper"/> used for mapping Umbraco objects.</param>
    /// <param name="cultureService">An instance of <see cref="ICultureService"/> used for managing culture information.</param>
    public AllCultureController(IUmbracoMapper umbracoMapper, ICultureService cultureService)
    {
        _umbracoMapper = umbracoMapper;
        _cultureService = cultureService;
    }

    /// <summary>
    /// Retrieves a paginated list of all available cultures, including their English and localized names.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of cultures to skip before starting to collect the result set.</param>
    /// <param name="take">The maximum number of cultures to return.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="PagedViewModel{CultureReponseModel}"/> with the paginated cultures.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<CultureReponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a paginated collection of cultures available for creating languages.")]
    [EndpointDescription("Gets a paginated collection containing the English and localized names of all available cultures.")]
    public Task<PagedViewModel<CultureReponseModel>> GetAll(CancellationToken cancellationToken, int skip = 0, int take = 100)
    {
        CultureInfo[] all = _cultureService.GetValidCultureInfos();

        var viewModel = new PagedViewModel<CultureReponseModel>
        {
            Items = _umbracoMapper.MapEnumerable<CultureInfo, CultureReponseModel>(all.Skip(skip).Take(take)),
            Total = all.Length
        };

        return Task.FromResult(viewModel);
    }
}
