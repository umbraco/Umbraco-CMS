using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Filters;

internal sealed class ContextualizeFromAcceptHeadersAttribute : TypeFilterAttribute
{
    public ContextualizeFromAcceptHeadersAttribute()
        : base(typeof(LocalizeFromAcceptLanguageHeaderAttributeFilter))
    {
    }

    private sealed class LocalizeFromAcceptLanguageHeaderAttributeFilter : IActionFilter
    {
        private readonly IRequestCultureService _requestCultureService;
        private readonly IRequestSegmentService _requestSegmentService;
        private readonly IVariationContextAccessor _variationContextAccessor;

        public LocalizeFromAcceptLanguageHeaderAttributeFilter(
            IRequestCultureService requestCultureService,
            IRequestSegmentService requestSegmentService,
            IVariationContextAccessor variationContextAccessor)
        {
            _requestCultureService = requestCultureService;
            _requestSegmentService = requestSegmentService;
            _variationContextAccessor = variationContextAccessor;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var requestedCulture = _requestCultureService.GetRequestedCulture().NullOrWhiteSpaceAsNull();
            var requestedSegment = _requestSegmentService.GetRequestedSegment().NullOrWhiteSpaceAsNull();
            if (requestedCulture.IsNullOrWhiteSpace() && requestedSegment.IsNullOrWhiteSpace())
            {
                return;
            }

            // contextualize the request
            // NOTE: request culture or segment can be null here (but not both), so make sure to retain any existing
            //       context by means of fallback to current variation context (if available)
            _variationContextAccessor.VariationContext = new VariationContext(
                requestedCulture ?? _variationContextAccessor.VariationContext?.Culture,
                requestedSegment ?? _variationContextAccessor.VariationContext?.Segment);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
