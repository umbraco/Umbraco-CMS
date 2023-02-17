using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.ContentApi;

namespace Umbraco.Cms.Api.Content.Rendering;

public class RequestContextOutputExpansionStrategyAccessor : IOutputExpansionStrategyAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestContextOutputExpansionStrategyAccessor(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public bool TryGetValue([NotNullWhen(true)] out IOutputExpansionStrategy? outputExpansionStrategy)
    {
        outputExpansionStrategy = _httpContextAccessor.HttpContext?.RequestServices.GetService<IOutputExpansionStrategy>();
        return outputExpansionStrategy is not null;
    }
}
