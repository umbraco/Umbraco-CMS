using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ScriptResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> ByPath(string path)
    {
        IScript? script = await _scriptService.GetAsync(path);

        return script is null
            ? NotFound()
            : Ok(_mapper.Map<ScriptResponseModel>(script));
    }
}
