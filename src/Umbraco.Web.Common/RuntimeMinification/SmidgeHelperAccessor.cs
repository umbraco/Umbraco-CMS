using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Smidge;

namespace Umbraco.Cms.Web.Common.RuntimeMinification;

// work around for SmidgeHelper being request/scope lifetime
public sealed class SmidgeHelperAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SmidgeHelperAccessor(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public SmidgeHelper SmidgeHelper
    {
        get
        {
            HttpContext? httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException(
                    $"Cannot get a {nameof(SmidgeHelper)} instance since there is no current http request");
            }

            return httpContext.RequestServices.GetService<SmidgeHelper>()!;
        }
    }
}
