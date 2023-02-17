using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core.Models.PublishedContent;
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
        private readonly IVariationContextAccessor _variationContextAccessor;

        public LocalizeFromAcceptLanguageHeaderAttributeFilter(IVariationContextAccessor variationContextAccessor)
            => _variationContextAccessor = variationContextAccessor;

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var acceptLanguage = context.HttpContext.Request.Headers.AcceptLanguage.ToString();
            if (acceptLanguage.IsNullOrWhiteSpace() || _variationContextAccessor.VariationContext?.Culture == acceptLanguage)
            {
                return;
            }

            _variationContextAccessor.VariationContext = new VariationContext(acceptLanguage);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
