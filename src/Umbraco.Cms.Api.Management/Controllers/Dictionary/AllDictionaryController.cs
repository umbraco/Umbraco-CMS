using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary;

    /// <summary>
    /// API controller responsible for retrieving and managing all dictionary items within the Umbraco CMS.
    /// </summary>
[ApiVersion("1.0")]
public class AllDictionaryController : DictionaryControllerBase
{
    private readonly IDictionaryItemService _dictionaryItemService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllDictionaryController"/> class.
    /// </summary>
    /// <param name="dictionaryItemService">An instance of <see cref="IDictionaryItemService"/> used to manage dictionary items.</param>
    /// <param name="umbracoMapper">An instance of <see cref="IUmbracoMapper"/> used for mapping between models.</param>
    public AllDictionaryController(IDictionaryItemService dictionaryItemService, IUmbracoMapper umbracoMapper)
    {
        _dictionaryItemService = dictionaryItemService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Retrieves a paginated list of dictionary items, optionally filtered by name.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="filter">An optional string to filter dictionary items by name.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <returns>A paginated view model containing dictionary overview response models.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DictionaryOverviewResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a paginated collection of dictionary items.")]
    [EndpointDescription("Gets a paginated collection of dictionary items with optional filtering by name.")]
    public async Task<ActionResult<PagedViewModel<DictionaryOverviewResponseModel>>> All(
        CancellationToken cancellationToken,
        string? filter = null,
        int skip = 0,
        int take = 100)
    {
        // unfortunately we can't paginate here...we'll have to get all and paginate in memory
        IDictionaryItem[] items = (await _dictionaryItemService.GetDescendantsAsync(Constants.System.RootKey, filter)).ToArray();
        var model = new PagedViewModel<DictionaryOverviewResponseModel>
        {
            Total = items.Length,
            Items = _umbracoMapper.MapEnumerable<IDictionaryItem, DictionaryOverviewResponseModel>(items.Skip(skip).Take(take))
        };
        return model;
    }
}
