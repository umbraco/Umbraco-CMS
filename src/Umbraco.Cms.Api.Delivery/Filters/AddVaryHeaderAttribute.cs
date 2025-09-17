using Microsoft.AspNetCore.Mvc.Filters;

namespace Umbraco.Cms.Api.Delivery.Filters;

public sealed class AddVaryHeaderAttribute : ActionFilterAttribute
{
    private const string Vary = "Accept-Language, Preview, Start-Item";

    public override void OnResultExecuting(ResultExecutingContext context)
        => context.HttpContext.Response.Headers.Vary = context.HttpContext.Response.Headers.Vary.Count > 0
            ? $"{context.HttpContext.Response.Headers.Vary}, {Vary}"
            : Vary;
}
