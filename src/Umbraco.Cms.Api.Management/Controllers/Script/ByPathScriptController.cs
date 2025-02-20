using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.Script;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Script;

[ApiVersion("1.0")]
public class ByPathScriptController : ScriptControllerBase
{
    private readonly IScriptService _scriptService;
    private readonly IUmbracoMapper _mapper;

    public ByPathScriptController(
        IScriptService scriptService,
        IUmbracoMapper mapper)
    {
        _scriptService = scriptService;
        _mapper = mapper;
    }

    [HttpGet("{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ScriptResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByPath(CancellationToken cancellationToken, string path)
    {
        path = DecodePath(path).VirtualPathToSystemPath();
        IScript? script = await _scriptService.GetAsync(path);

        return script is not null
            ? Ok(_mapper.Map<ScriptResponseModel>(script))
            : ScriptNotFound();
    }
}
