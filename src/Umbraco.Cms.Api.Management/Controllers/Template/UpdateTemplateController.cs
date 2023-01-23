using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Template;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid key, TemplateUpdateModel updateModel)
    {
        ITemplate? template = await _templateService.GetAsync(key);
        if (template == null)
        {
            return NotFound();
        }

        template = _umbracoMapper.Map(updateModel, template);

        Attempt<ITemplate, TemplateOperationStatus> result = await _templateService.UpdateAsync(template, CurrentUserId(_backOfficeSecurityAccessor));

        return result.Success ?
            Ok()
            : TemplateOperationStatusResult(result.Status);
    }
}
