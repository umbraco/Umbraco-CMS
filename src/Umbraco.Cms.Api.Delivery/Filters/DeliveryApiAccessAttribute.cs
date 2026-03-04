using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Filters;

/// <summary>
/// An action filter attribute that verifies public or preview access to the Delivery API, returning
/// a <c>401 Unauthorized</c> result if access is denied.
/// </summary>
public sealed class DeliveryApiAccessAttribute : TypeFilterAttribute
{
    public DeliveryApiAccessAttribute()
        : base(typeof(DeliveryApiAccessFilter))
    {
    }

    private sealed class DeliveryApiAccessFilter : IActionFilter
    {
        private readonly IApiAccessService _apiAccessService;
        private readonly IRequestPreviewService _requestPreviewService;

        public DeliveryApiAccessFilter(IApiAccessService apiAccessService, IRequestPreviewService requestPreviewService)
        {
            _apiAccessService = apiAccessService;
            _requestPreviewService = requestPreviewService;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var hasAccess = _requestPreviewService.IsPreview()
                ? _apiAccessService.HasPreviewAccess()
                : _apiAccessService.HasPublicAccess();

            if (hasAccess)
            {
                return;
            }

            context.Result = new UnauthorizedResult();
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
