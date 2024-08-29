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
public class CreatePartialViewController : PartialViewControllerBase
{
    private readonly IPartialViewService _partialViewService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreatePartialViewController(
        IPartialViewService partialViewService,
        IUmbracoMapper umbracoMapper,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _partialViewService = partialViewService;
        _umbracoMapper = umbracoMapper;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken,
        CreatePartialViewRequestModel requestModel)
    {
        PartialViewCreateModel createModel = _umbracoMapper.Map<PartialViewCreateModel>(requestModel)!;
        Attempt<IPartialView?, PartialViewOperationStatus> createAttempt = await _partialViewService.CreateAsync(createModel, CurrentUserKey(_backOfficeSecurityAccessor));

        return createAttempt.Success
            ? CreatedAtPath<ByPathPartialViewController>(controller => nameof(controller.ByPath), createAttempt.Result!.Path.SystemPathToVirtualPath())
            : PartialViewOperationStatusResult(createAttempt.Status);
    }
}
