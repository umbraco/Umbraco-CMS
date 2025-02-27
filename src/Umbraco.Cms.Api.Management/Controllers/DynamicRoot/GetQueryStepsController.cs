using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.DynamicRoot.QuerySteps;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DynamicRoot;

[Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
[ApiVersion("1.0")]
public class GetQueryStepsController : DynamicRootControllerBase
{
    private readonly DynamicRootQueryStepCollection _dynamicRootQueryStepCollection;

    public GetQueryStepsController(DynamicRootQueryStepCollection dynamicRootQueryStepCollection)
    {
        _dynamicRootQueryStepCollection = dynamicRootQueryStepCollection;
    }

    [HttpGet($"steps")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuerySteps(CancellationToken cancellationToken)
    {
        IEnumerable<string> querySteps = _dynamicRootQueryStepCollection.Select(x => x.SupportedDirectionAlias);

        return await Task.FromResult(Ok(querySteps));
    }
}
