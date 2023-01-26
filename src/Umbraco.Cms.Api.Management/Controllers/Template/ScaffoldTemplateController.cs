using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Template;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.Template;

public class ScaffoldTemplateController : TemplateControllerBase
{
    private readonly IDefaultViewContentProvider _defaultViewContentProvider;

    public ScaffoldTemplateController(IDefaultViewContentProvider defaultViewContentProvider)
        => _defaultViewContentProvider = defaultViewContentProvider;

    [HttpGet("scaffold")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(TemplateScaffoldViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TemplateScaffoldViewModel>> Scaffold()
    {
        var scaffoldViewModel = new TemplateScaffoldViewModel
        {
            Content = _defaultViewContentProvider.GetDefaultFileContent()
        };

        return await Task.FromResult(Ok(scaffoldViewModel));
    }
}
