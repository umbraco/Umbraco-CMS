using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Api.Management.ViewModels.Help;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;

namespace Umbraco.Cms.Api.Management.Controllers.Help;

/// <summary>
/// Provides endpoints for help-related API requests in the management area.
/// </summary>
[Obsolete("This is no longer used. Scheduled for removal in Umbraco 19.")]
[ApiVersion("1.0")]
public class GetHelpController : HelpControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Help.GetHelpController"/> class.
    /// </summary>
    /// <param name="helpPageSettings">The <see cref="IOptionsMonitor{HelpPageSettings}"/> providing the help page settings.</param>
    /// <param name="logger">The <see cref="ILogger{GetHelpController}"/> instance for logging.</param>
    /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/> used for JSON serialization.</param>
    public GetHelpController(
        IOptionsMonitor<HelpPageSettings> helpPageSettings,
        ILogger<GetHelpController> logger,
        IJsonSerializer jsonSerializer)
    {
    }

    /// <summary>
    /// Retrieves help information and documentation resources for a specified section (and optionally tree) of the Umbraco back office.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <param name="section">The alias of the section for which to retrieve help information.</param>
    /// <param name="tree">The optional alias of the tree within the section to filter help information.</param>
    /// <param name="skip">The number of items to skip before returning results (for paging).</param>
    /// <param name="take">The maximum number of items to return (for paging).</param>
    /// <param name="baseUrl">The base URL used to fetch help documentation resources.</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="IActionResult"/> with an empty paged list of help information.</returns>
    /// <remarks>
    /// No help information is available, so an empty result is always returned.
    /// </remarks>
    [HttpGet]
    [MapToApiVersion("1.0")]

    // The 400 is never returned any more, but stays declared so the generated backoffice client keeps
    // exporting the "GetHelpError" type: hey-api omits that alias for operations whose only error is a 401.
    // TODO (V19): remove along with this controller.
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PagedViewModel<HelpPageResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets help information.")]
    [EndpointDescription("Gets help information and documentation resources for the Umbraco back office.")]
    public Task<IActionResult> Get(
        CancellationToken cancellationToken,
        string section,
        string? tree,
        int skip = 0,
        int take = 100,
        string? baseUrl = null)
        => Task.FromResult<IActionResult>(Ok(PagedViewModel<HelpPageResponseModel>.Empty()));
}
