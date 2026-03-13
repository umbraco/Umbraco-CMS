using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Api.Common.Builders;
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
    private readonly ILogger<GetHelpController> _logger;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly HelpPageSettings _helpPageSettings;

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
        _logger = logger;
        _jsonSerializer = jsonSerializer;
        _helpPageSettings = helpPageSettings.CurrentValue;
    }

    /// <summary>
    /// Retrieves help information and documentation resources for a specified section (and optionally tree) of the Umbraco back office.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <param name="section">The alias of the section for which to retrieve help information.</param>
    /// <param name="tree">The optional alias of the tree within the section to filter help information.</param>
    /// <param name="skip">The number of items to skip before returning results (for paging).</param>
    /// <param name="take">The maximum number of items to return (for paging).</param>
    /// <param name="baseUrl">The base URL used to fetch help documentation resources. Defaults to the official Umbraco documentation site.</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="IActionResult"/> with a paged list of help information or an error response.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PagedViewModel<HelpPageResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets help information.")]
    [EndpointDescription("Gets help information and documentation resources for the Umbraco back office.")]
    public async Task<IActionResult> Get(
        CancellationToken cancellationToken,
        string section,
        string? tree,
        int skip = 0,
        int take = 100,
        string? baseUrl = "https://our.umbraco.com")
    {
        if (IsAllowedUrl(baseUrl) is false)
        {
            _logger.LogError($"The following URL is not listed in the allowlist for HelpPage in HelpPageSettings: {baseUrl}");

            ProblemDetails invalidModelProblem =
                new ProblemDetailsBuilder()
                    .WithTitle("Invalid database configuration")
                    .WithDetail("The provided database configuration is invalid")
                    .Build();

            return BadRequest(invalidModelProblem);
        }

        var url = string.Format(baseUrl + "/Umbraco/Documentation/Lessons/GetContextHelpDocs?sectionAlias={0}&treeAlias={1}", section, tree);

        try
        {
            var httpClient = new HttpClient();

            // fetch dashboard json and parse to JObject
            var json = await httpClient.GetStringAsync(url);
            List<HelpPageResponseModel>? result = _jsonSerializer.Deserialize<List<HelpPageResponseModel>>(json);
            if (result != null)
            {
                return Ok(new PagedViewModel<HelpPageResponseModel>
                {
                    Total = result.Count,
                    Items = result.Skip(skip).Take(take),
                });
            }
        }
        catch (HttpRequestException rex)
        {
            _logger.LogInformation($"Check your network connection, exception: {rex.Message}");
        }

        return Ok(PagedViewModel<HelpPageResponseModel>.Empty());
    }

    private bool IsAllowedUrl(string? url) => url is null || _helpPageSettings.HelpPageUrlAllowList.Contains(url);
}
