using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Api.Delivery.Filters;

internal sealed class ValidateStartItemAttribute : TypeFilterAttribute
{
    public ValidateStartItemAttribute()
        : base(typeof(ValidateStartItemFilter))
    {
    }

    private class ValidateStartItemFilter : IActionFilter
    {
        private readonly IRequestStartItemProvider _requestStartItemProvider;

        public ValidateStartItemFilter(IRequestStartItemProvider requestStartItemProvider)
            => _requestStartItemProvider = requestStartItemProvider;

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (_requestStartItemProvider.RequestedStartItem() is null)
            {
                return;
            }

            IPublishedContent? startItem = _requestStartItemProvider.GetStartItem();

            if (startItem is null)
            {
                context.Result = new NotFoundObjectResult("The Start-Item could not be found");
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
