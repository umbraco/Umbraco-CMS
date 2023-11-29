using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Cms.Api.Delivery.Accessors;

public abstract class RequestContextServiceAccessorBase<T>
    where T : class
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    protected RequestContextServiceAccessorBase(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public bool TryGetValue([NotNullWhen(true)] out T? requestStartNodeService)
    {
        requestStartNodeService = _httpContextAccessor.HttpContext?.RequestServices.GetService<T>();
        return requestStartNodeService is not null;
    }
}
