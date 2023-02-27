using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Content.Filters;

public class LocalizeFromAcceptLanguageHeaderAttribute : TypeFilterAttribute
{
    public LocalizeFromAcceptLanguageHeaderAttribute()
        : base(typeof(LocalizeFromAcceptLanguageHeaderAttributeFilter))
    {
    }

    private class LocalizeFromAcceptLanguageHeaderAttributeFilter : IActionFilter
    {
        private readonly IRequestCultureService _requestCultureService;

        public LocalizeFromAcceptLanguageHeaderAttributeFilter(IRequestCultureService requestCultureService)
            => _requestCultureService = requestCultureService;

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var requestedCulture = _requestCultureService.GetRequestedCulture();
            if (requestedCulture.IsNullOrWhiteSpace())
            {
                return;
            }

            _requestCultureService.SetRequestCulture(requestedCulture);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
