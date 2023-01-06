using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Template;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Template;

public class ScaffoldTemplateController : TemplateControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly IDefaultViewContentProvider _defaultViewContentProvider;

    public ScaffoldTemplateController(
        ITemplateService templateService,
        IDefaultViewContentProvider defaultViewContentProvider)
    {
        _templateService = templateService;
        _defaultViewContentProvider = defaultViewContentProvider;
    }

    [HttpGet("scaffold")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(TemplateScaffoldViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TemplateScaffoldViewModel>> Scaffold(string? masterTemplateAlias = null)
    {
        if (masterTemplateAlias.IsNullOrWhiteSpace() == false)
        {
            if (_templateService.GetTemplate(masterTemplateAlias) == null)
            {
                return NotFound($"Could not find a master template with alias {masterTemplateAlias}");
            }
        }

        var scaffoldViewModel = new TemplateScaffoldViewModel
        {
            Content = _defaultViewContentProvider.GetDefaultFileContent(masterTemplateAlias)
        };

        return await Task.FromResult(Ok(scaffoldViewModel));
    }
}
