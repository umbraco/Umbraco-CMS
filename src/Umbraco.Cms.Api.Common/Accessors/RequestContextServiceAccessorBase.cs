using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Cms.Api.Common.Accessors;

/// <summary>
///     Base class for accessing request-scoped services from the current HTTP context.
/// </summary>
/// <typeparam name="T">The type of service to access.</typeparam>
public abstract class RequestContextServiceAccessorBase<T>
    where T : class
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RequestContextServiceAccessorBase{T}"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    protected RequestContextServiceAccessorBase(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    /// <summary>
    ///     Attempts to retrieve the service from the current HTTP context's request services.
    /// </summary>
    /// <param name="requestStartNodeService">When this method returns, contains the service instance if found; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the service was found; otherwise, <c>false</c>.</returns>
    public bool TryGetValue([NotNullWhen(true)] out T? requestStartNodeService)
    {
        requestStartNodeService = _httpContextAccessor.HttpContext?.RequestServices.GetService<T>();
        return requestStartNodeService is not null;
    }
}
