using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Cms.Web.Common;

public class UmbracoHelperAccessor : IUmbracoHelperAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UmbracoHelperAccessor(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    public bool TryGetUmbracoHelper([MaybeNullWhen(false)] out UmbracoHelper umbracoHelper)
    {
        umbracoHelper = _httpContextAccessor.HttpContext?.RequestServices.GetService<UmbracoHelper>();
        return umbracoHelper is not null;
    }
}
