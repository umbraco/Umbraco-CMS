using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.OEmbed;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.OEmbed;

/// <summary>
/// Provides endpoints for querying OEmbed information from supported providers.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]
[ApiVersion("1.0")]
public class QueryOEmbedController : OEmbedControllerBase
{
    private readonly IOEmbedService _oEmbedService;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryOEmbedController"/> class.
    /// </summary>
    /// <param name="oEmbedService">The <see cref="IOEmbedService"/> instance used to handle OEmbed operations. This service is injected.</param>
    public QueryOEmbedController(IOEmbedService oEmbedService)
    {
        _oEmbedService = oEmbedService;
    }

    /// <summary>
    /// Retrieves OEmbed information for a given URL, optionally constraining the embedded content's dimensions.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="url">The URL for which to retrieve OEmbed information.</param>
    /// <param name="maxWidth">The optional maximum width of the embedded content.</param>
    /// <param name="maxHeight">The optional maximum height of the embedded content.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing an <see cref="OEmbedResponseModel"/> with the markup if successful; otherwise, an error result indicating the failure reason.
    /// </returns>
    [HttpGet("query")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(OEmbedResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Queries OEmbed information.")]
    [EndpointDescription("Queries OEmbed information for the specified URL.")]
    public async Task<IActionResult> Query(CancellationToken cancellationToken, Uri url, int? maxWidth = null, int? maxHeight = null)
    {
        Attempt<string, OEmbedOperationStatus> result = await _oEmbedService.GetMarkupAsync(url, maxWidth, maxHeight, cancellationToken);

        return result.Success
            ? Ok(new OEmbedResponseModel() { Markup = result.Result })
            : OEmbedOperationStatusResult(result.Status);
    }
}
