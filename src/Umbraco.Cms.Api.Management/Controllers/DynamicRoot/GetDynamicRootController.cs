using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DynamicRoot;
using Umbraco.Cms.Core.DynamicRoot;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DynamicRoot;

[Authorize(Policy = "New" + AuthorizationPolicies.SectionAccessContent)]
[ApiVersion("1.0")]
public class GetRootsController : DynamicRootControllerBase
{
    private readonly IDynamicRootService _dynamicRootService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IVariationContextAccessor _variationContextAccessor;

    public GetRootsController(IDynamicRootService dynamicRootService, IUmbracoMapper umbracoMapper, IVariationContextAccessor variationContextAccessor)
    {
        _dynamicRootService = dynamicRootService;
        _umbracoMapper = umbracoMapper;
        _variationContextAccessor = variationContextAccessor;
    }

    [HttpPost("determine-roots-from-query")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DynamicRootResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoots([FromBody]DynamicRootRequestModel model)
    {
        //TODO For management API, can we figure out a better way to set the variation context
        _variationContextAccessor.VariationContext = new VariationContext(model.Context.Culture, model.Context.Segment);

        DynamicRootNodeQuery dynamicRootNodeQuery = _umbracoMapper.Map<DynamicRootNodeQuery>(model)!;

        IEnumerable<Guid> roots = await _dynamicRootService.GetDynamicRootsAsync(dynamicRootNodeQuery);

        return Ok(new DynamicRootResponseModel()
        {
            Roots = roots
        });
    }
}
