using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Template;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.Template;

[ApiVersion("1.0")]
public class ScaffoldTemplateController : TemplateControllerBase
{
    private readonly IDefaultViewContentProvider _defaultViewContentProvider;

    public ScaffoldTemplateController(IDefaultViewContentProvider defaultViewContentProvider)
        => _defaultViewContentProvider = defaultViewContentProvider;

    [HttpGet("scaffold")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(TemplateScaffoldResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TemplateScaffoldResponseModel>> Scaffold()
    {
        var scaffoldViewModel = new TemplateScaffoldResponseModel
        {
            Content = _defaultViewContentProvider.GetDefaultFileContent()
        };

        return await Task.FromResult(Ok(scaffoldViewModel));
    }
}
