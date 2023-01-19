using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Template;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Template;

public class UpdateTemplateController : TemplateControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public UpdateTemplateController(
        ITemplateService templateService,
        IUmbracoMapper umbracoMapper,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _templateService = templateService;
        _umbracoMapper = umbracoMapper;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(Guid key, TemplateUpdateModel updateModel)
    {
        ITemplate? template = await _templateService.GetTemplateAsync(key);
        if (template == null)
        {
            return NotFound();
        }

        template = _umbracoMapper.Map(updateModel, template);
        await _templateService.SaveTemplateAsync(template, CurrentUserId(_backOfficeSecurityAccessor));

        return Ok();
    }
}
