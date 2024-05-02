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

[Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]
[ApiVersion("1.0")]
public class QueryOEmbedController : OEmbedControllerBase
{
    private readonly IOEmbedService _oEmbedService;

    public QueryOEmbedController(IOEmbedService oEmbedService)
    {
        _oEmbedService = oEmbedService;
    }

    [HttpGet("query")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(OEmbedResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Query(CancellationToken cancellationToken, Uri url, int? maxWidth = null, int? maxHeight = null)
    {
        Attempt<string, OEmbedOperationStatus> result = await _oEmbedService.GetMarkupAsync(url, maxWidth, maxHeight, cancellationToken);

        return result.Success
            ? Ok(new OEmbedResponseModel() { Markup = result.Result })
            : OEmbedOperationStatusResult(result.Status);
    }
}
