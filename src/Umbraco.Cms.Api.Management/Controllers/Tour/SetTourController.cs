using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Tour;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Tour;

[ApiVersion("1.0")]
public class SetTourController : TourControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly ITourService _tourService;
    private readonly IUmbracoMapper _umbracoMapper;

    public SetTourController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        ITourService tourService,
        IUmbracoMapper umbracoMapper)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _tourService = tourService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SetTour(SetTourStatusRequestModel model)
    {
        Guid currentUserKey = CurrentUserKey(_backOfficeSecurityAccessor);

        UserTourStatus tourStatus = _umbracoMapper.Map<UserTourStatus>(model)!;

        TourOperationStatus attempt = await _tourService.SetAsync(tourStatus, currentUserKey);
        return TourOperationStatusResult(attempt);
    }
}
