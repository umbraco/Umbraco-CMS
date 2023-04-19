using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.PartialView;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView;

public class CreatePartialViewController : PartialViewControllerBase
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IPartialViewService _partialViewService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreatePartialViewController(
        IUmbracoMapper umbracoMapper,
        IPartialViewService partialViewService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _umbracoMapper = umbracoMapper;
        _partialViewService = partialViewService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Create(CreatePartialViewRequestModel createRequestModel)
    {
        PartialViewCreateModel createModel = _umbracoMapper.Map<PartialViewCreateModel>(createRequestModel)!;

        Attempt<IPartialView?, PartialViewOperationStatus> createAttempt = await _partialViewService.CreateAsync(createModel, CurrentUserKey(_backOfficeSecurityAccessor));

        return createAttempt.Success
            ? Ok()
            : PartialViewOperationStatusResult(createAttempt.Status);
    }
}
