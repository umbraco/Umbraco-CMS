using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView;

[ApiVersion("1.0")]
public class DeletePartialViewController : PartialViewControllerBase
{
    private readonly IPartialViewService _partialViewService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public DeletePartialViewController(
        IPartialViewService partialViewService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _partialViewService = partialViewService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpDelete]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(string path)
    {
        PartialViewOperationStatus operationStatus = await _partialViewService.DeleteAsync(path, CurrentUserKey(_backOfficeSecurityAccessor));

        return operationStatus is PartialViewOperationStatus.Success
            ? Ok()
            : PartialViewOperationStatusResult(operationStatus);
    }
}
