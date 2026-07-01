using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.Script;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Script;

/// <summary>
/// Provides API endpoints for managing script resources identified by their path.
/// </summary>
[ApiVersion("1.0")]
public class ByPathScriptController : ScriptControllerBase
{
    private readonly IScriptService _scriptService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Script.ByPathScriptController"/> class.
    /// </summary>
    /// <param name="scriptService">An instance of <see cref="IScriptService"/> used to manage script operations.</param>
    /// <param name="mapper">An instance of <see cref="IUmbracoMapper"/> used for mapping between models.</param>
    public ByPathScriptController(
        IScriptService scriptService,
        IUmbracoMapper mapper)
    {
        _scriptService = scriptService;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves a script file identified by the specified path.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="path">The virtual or system file path used to locate the script.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a <see cref="ScriptResponseModel"/> with the script data if found (HTTP 200),
    /// or a <see cref="ProblemDetails"/> result if the script is not found (HTTP 404).
    /// </returns>
    [HttpGet("{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ScriptResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a script by path.")]
    [EndpointDescription("Gets a script identified by the provided file path.")]
    public async Task<IActionResult> ByPath(CancellationToken cancellationToken, string path)
    {
        path = DecodePath(path).VirtualPathToSystemPath();
        IScript? script = await _scriptService.GetAsync(path);

        return script is not null
            ? Ok(_mapper.Map<ScriptResponseModel>(script))
            : ScriptNotFound();
    }
}
