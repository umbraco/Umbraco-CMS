using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Tour;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Tour;

[ApiVersion("1.0")]
public class GetTourController : TourControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly ITourService _tourService;
    private readonly IUmbracoMapper _umbracoMapper;

    public GetTourController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        ITourService tourService,
        IUmbracoMapper umbracoMapper)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _tourService = tourService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(UserTourStatusesResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTours()
    {
        Guid currentUserKey = CurrentUserKey(_backOfficeSecurityAccessor);
        Attempt<IEnumerable<UserTourStatus>, TourOperationStatus> toursAttempt = await _tourService.GetAllAsync(currentUserKey);

        if (toursAttempt.Success == false)
        {
            return TourOperationStatusResult(toursAttempt.Status);
        }

        List<TourStatusViewModel> models = _umbracoMapper.MapEnumerable<UserTourStatus, TourStatusViewModel>(toursAttempt.Result);
        return Ok(new UserTourStatusesResponseModel { TourStatuses = models });
    }
}
