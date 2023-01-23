using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid key)
    {
        Attempt<ITemplate?, TemplateOperationStatus> result = await _templateService.DeleteAsync(key, CurrentUserId(_backOfficeSecurityAccessor));
        return result.Success
            ? Ok()
            : TemplateOperationStatusResult(result.Status);
    }
}
