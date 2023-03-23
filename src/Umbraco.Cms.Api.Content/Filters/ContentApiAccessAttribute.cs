using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core.ContentApi;

namespace Umbraco.Cms.Api.Content.Filters;

public class ContentApiAccessAttribute : TypeFilterAttribute
{
    public ContentApiAccessAttribute()
        : base(typeof(ContentApiAccessFilter))
    {
    }

    private class ContentApiAccessFilter : IActionFilter
    {
        private readonly IApiAccessService _apiAccessService;
        private readonly IRequestPreviewService _requestPreviewService;

        public ContentApiAccessFilter(IApiAccessService apiAccessService, IRequestPreviewService requestPreviewService)
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
