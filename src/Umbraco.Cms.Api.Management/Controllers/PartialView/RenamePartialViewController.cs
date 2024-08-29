using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.PartialView;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView;

[ApiVersion("1.0")]
public class RenamePartialViewController : PartialViewControllerBase
{
    private readonly IPartialViewService _partialViewService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public RenamePartialViewController(IPartialViewService partialViewService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor, IUmbracoMapper umbracoMapper)
    {
        _partialViewService = partialViewService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _umbracoMapper = umbracoMapper;
    }

    [HttpPut("{path}/rename")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Rename(
        CancellationToken cancellationToken,
        string path,
        RenamePartialViewRequestModel requestModel)
    {
        PartialViewRenameModel renameModel = _umbracoMapper.Map<PartialViewRenameModel>(requestModel)!;

        path = DecodePath(path).VirtualPathToSystemPath();
        Attempt<IPartialView?, PartialViewOperationStatus> renameAttempt = await _partialViewService.RenameAsync(path, renameModel, CurrentUserKey(_backOfficeSecurityAccessor));

        return renameAttempt.Success
            ? CreatedAtPath<ByPathPartialViewController>(controller => nameof(controller.ByPath), renameAttempt.Result!.Path.SystemPathToVirtualPath())
            : PartialViewOperationStatusResult(renameAttempt.Status);
    }
}
