using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Template;

public class DeleteTemplateController : TemplateControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public DeleteTemplateController(ITemplateService templateService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _templateService = templateService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpDelete("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid key)
    {
        ITemplate? template = _templateService.GetTemplate(key);
        if (template == null)
        {
            return NotFound();
        }

        _templateService.DeleteTemplate(template.Alias, CurrentUserId(_backOfficeSecurityAccessor));
        return await Task.FromResult(Ok());
    }
}
