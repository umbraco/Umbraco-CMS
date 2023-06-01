using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Template;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Template;

[ApiVersion("1.0")]
public class ScaffoldTemplateController : TemplateControllerBase
{
    private readonly IDefaultViewContentProvider _defaultViewContentProvider;
    private readonly ITemplateService _templateService;

    public ScaffoldTemplateController(
        IDefaultViewContentProvider defaultViewContentProvider,
        ITemplateService templateService)
    {
        _defaultViewContentProvider = defaultViewContentProvider;
        _templateService = templateService;
    }

    [HttpGet("scaffold")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(TemplateScaffoldResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TemplateScaffoldResponseModel>> Scaffold([FromQuery(Name = "masterTemplateId")] Guid? masterTemplateId)
    {
        var scaffoldViewModel = new TemplateScaffoldResponseModel
        {
            Content = await _templateService.GetScaffoldAsync(masterTemplateId),
        };

        return await Task.FromResult(Ok(scaffoldViewModel));
    }
}
