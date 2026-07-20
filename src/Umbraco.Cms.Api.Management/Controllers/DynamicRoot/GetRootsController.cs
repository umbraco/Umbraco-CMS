using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DynamicRoot;
using Umbraco.Cms.Core.DynamicRoot;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.Context;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DynamicRoot;

/// <summary>
/// Controller for retrieving root dynamic content items.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]
[ApiVersion("1.0")]
public class GetRootsController : DynamicRootControllerBase
{
    private readonly IDynamicRootService _dynamicRootService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeVariationContextAccessor _backOfficeVariationContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRootsController"/> class.
    /// </summary>
    /// <param name="dynamicRootService">Service used to retrieve dynamic root information.</param>
    /// <param name="umbracoMapper">Mapper for converting between Umbraco domain and API models.</param>
    /// <param name="backOfficeVariationContextAccessor">Accessor for the current back office variation context.</param>
    public GetRootsController(IDynamicRootService dynamicRootService, IUmbracoMapper umbracoMapper, IBackOfficeVariationContextAccessor backOfficeVariationContextAccessor)
    {
        _dynamicRootService = dynamicRootService;
        _umbracoMapper = umbracoMapper;
        _backOfficeVariationContextAccessor = backOfficeVariationContextAccessor;
    }

    /// <summary>
    /// Retrieves a collection of dynamic root item identifiers based on the specified query configuration and context.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="model">The request model containing the query configuration and context information, such as culture and segment.</param>
    /// <returns>An <see cref="IActionResult"/> containing a <see cref="DynamicRootResponseModel"/> with the collection of dynamic root item identifiers.</returns>
    [HttpPost("query")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DynamicRootResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets dynamic roots.")]
    [EndpointDescription("Gets a collection of dynamic root items based on the provided query configuration.")]
    public async Task<IActionResult> GetRoots(CancellationToken cancellationToken, DynamicRootRequestModel model)
    {
        _backOfficeVariationContextAccessor.VariationContext = new BackOfficeVariationContext(model.Context.Culture, model.Context.Segment);

        DynamicRootNodeQuery dynamicRootNodeQuery = _umbracoMapper.Map<DynamicRootNodeQuery>(model)!;

        IEnumerable<Guid> roots = await _dynamicRootService.GetDynamicRootsAsync(dynamicRootNodeQuery);

        return Ok(new DynamicRootResponseModel()
        {
            Roots = roots
        });
    }
}
