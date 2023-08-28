using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Filters;

internal sealed class DeliveryApiAccessAttribute : TypeFilterAttribute
{
    public DeliveryApiAccessAttribute()
        : base(typeof(DeliveryApiAccessFilter))
    {
    }

    private class DeliveryApiAccessFilter : IActionFilter
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
