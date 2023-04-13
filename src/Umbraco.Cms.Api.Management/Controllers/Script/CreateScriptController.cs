using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Script;

namespace Umbraco.Cms.Api.Management.Controllers.Script;

public class CreateScriptController : ScriptControllerBase
{

    [HttpPost]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Create(CreateScriptRequestModel createRequestModel)
    {
        return new OkResult();
    }
}
