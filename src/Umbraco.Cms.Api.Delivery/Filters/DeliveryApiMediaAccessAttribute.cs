using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Filters;

internal sealed class DeliveryApiMediaAccessAttribute : TypeFilterAttribute
{
    public DeliveryApiMediaAccessAttribute()
        : base(typeof(DeliveryApiMediaAccessFilter))
    {
    }

    private class DeliveryApiMediaAccessFilter : IActionFilter
    {
        private readonly IApiAccessService _apiAccessService;

        public DeliveryApiMediaAccessFilter(IApiAccessService apiAccessService)
            => _apiAccessService = apiAccessService;

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (_apiAccessService.HasMediaAccess())
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
