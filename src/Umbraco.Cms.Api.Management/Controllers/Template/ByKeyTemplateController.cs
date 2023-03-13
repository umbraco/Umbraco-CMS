using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Template;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Template;

public class ByKeyTemplateController : TemplateControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ByKeyTemplateController(ITemplateService templateService, IUmbracoMapper umbracoMapper)
    {
        _templateService = templateService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(TemplateResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TemplateResponseModel>> ByKey(Guid key)
    {
        ITemplate? template = await _templateService.GetAsync(key);
        return template == null
            ? NotFound()
            : Ok(_umbracoMapper.Map<TemplateResponseModel>(template));
    }
}
