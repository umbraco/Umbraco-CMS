using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.DynamicRoot.QuerySteps;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DynamicRoot;

/// <summary>
/// Controller responsible for handling requests related to retrieving query steps for the Dynamic Root feature.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
[ApiVersion("1.0")]
public class GetQueryStepsController : DynamicRootControllerBase
{
    private readonly DynamicRootQueryStepCollection _dynamicRootQueryStepCollection;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetQueryStepsController"/> class.
    /// </summary>
    /// <param name="dynamicRootQueryStepCollection">A <see cref="DynamicRootQueryStepCollection"/> representing the collection of dynamic root query steps to be used by the controller.</param>
    public GetQueryStepsController(DynamicRootQueryStepCollection dynamicRootQueryStepCollection)
    {
        _dynamicRootQueryStepCollection = dynamicRootQueryStepCollection;
    }

    /// <summary>
    /// Retrieves the available query step aliases for configuring dynamic root queries.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="IActionResult"/> containing a collection of query step aliases.</returns>
    [HttpGet($"steps")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets dynamic root query steps.")]
    [EndpointDescription("Gets a collection of available query steps for configuring dynamic root queries.")]
    public Task<IActionResult> GetQuerySteps(CancellationToken cancellationToken)
    {
        IEnumerable<string> querySteps = _dynamicRootQueryStepCollection.Select(x => x.SupportedDirectionAlias);

        return Task.FromResult<IActionResult>(Ok(querySteps));
    }
}
