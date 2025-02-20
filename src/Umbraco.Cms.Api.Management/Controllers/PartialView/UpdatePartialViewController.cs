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
public class UpdatePartialViewController : PartialViewControllerBase
{
    private readonly IPartialViewService _partialViewService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUmbracoMapper _mapper;

    public UpdatePartialViewController(
        IPartialViewService partialViewService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUmbracoMapper mapper)
    {
        _partialViewService = partialViewService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _mapper = mapper;
    }

    [HttpPut("{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        string path,
        UpdatePartialViewRequestModel updateViewModel)
    {
        path = DecodePath(path).VirtualPathToSystemPath();
        PartialViewUpdateModel updateModel = _mapper.Map<PartialViewUpdateModel>(updateViewModel)!;

        Attempt<IPartialView?, PartialViewOperationStatus> updateAttempt = await _partialViewService.UpdateAsync(path, updateModel, CurrentUserKey(_backOfficeSecurityAccessor));

        return updateAttempt.Success
            ? Ok()
            : PartialViewOperationStatusResult(updateAttempt.Status);
    }
}
