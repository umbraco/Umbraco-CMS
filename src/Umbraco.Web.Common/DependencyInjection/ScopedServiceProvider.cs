using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Web.Common.DependencyInjection;

/// <inheritdoc />
internal class ScopedServiceProvider : IScopedServiceProvider
{
    private readonly IHttpContextAccessor _accessor;

    public ScopedServiceProvider(IHttpContextAccessor accessor) => _accessor = accessor;

    /// <inheritdoc />
    public IServiceProvider? ServiceProvider => _accessor.HttpContext?.RequestServices;
}
