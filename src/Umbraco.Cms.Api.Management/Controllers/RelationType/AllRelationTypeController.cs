using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.RelationType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.RelationType;

/// <summary>
/// Provides API endpoints for managing all relation types in Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class AllRelationTypeController : RelationTypeControllerBase
{
    private readonly IRelationService _relationService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllRelationTypeController"/> class.
    /// </summary>
    /// <param name="relationService">Service used to manage and query relation types.</param>
    /// <param name="umbracoMapper">The mapper used to map Umbraco domain models to API models.</param>
    public AllRelationTypeController(
        IRelationService relationService,
        IUmbracoMapper umbracoMapper)
    {
        _relationService = relationService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Retrieves a paginated list of all relation types configured in the system.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of relation types to skip before starting to collect the result set.</param>
    /// <param name="take">The maximum number of relation types to return.</param>
    /// <returns>A paginated view model containing relation type response models.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<RelationTypeResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a paginated collection of relation types.")]
    [EndpointDescription("Gets a paginated collection of all relation types configured in the system.")]
    public async Task<ActionResult<PagedViewModel<RelationTypeResponseModel>>> Get(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
    {
        PagedModel<IRelationType> allRelationTypes = await _relationService.GetPagedRelationTypesAsync(skip, take);

        var pagedResult = new PagedViewModel<RelationTypeResponseModel>
        {
            Total = allRelationTypes.Total,
            Items = _umbracoMapper.MapEnumerable<IRelationType, RelationTypeResponseModel>(allRelationTypes.Items.Skip(skip).Take(take)),
        };

        return Ok(pagedResult);
    }
}
