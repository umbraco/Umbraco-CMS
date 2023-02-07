using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

internal class EfCoreScopeAccessor : IEFCoreScopeAccessor
{
    private readonly IAmbientEfCoreScopeStack _ambientEfCoreScopeStack;

    public EfCoreScopeAccessor()
        : this (StaticServiceProvider.Instance.GetRequiredService<IAmbientEfCoreScopeStack>())
    {
    }

    internal EfCoreScopeAccessor(IAmbientEfCoreScopeStack ambientEfCoreScopeStack)
    {
        _ambientEfCoreScopeStack = ambientEfCoreScopeStack;
    }

    public EfCoreScope? AmbientScope => (EfCoreScope?)_ambientEfCoreScopeStack.AmbientScope;

    IEfCoreScope? IEFCoreScopeAccessor.AmbientScope => _ambientEfCoreScopeStack.AmbientScope;
}
