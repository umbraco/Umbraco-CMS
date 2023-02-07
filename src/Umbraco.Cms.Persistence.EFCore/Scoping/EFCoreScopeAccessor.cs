using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

internal class EFCoreScopeAccessor : IEFCoreScopeAccessor
{
    private readonly IAmbientEfCoreScopeStack _ambientEfCoreScopeStack;

    public EFCoreScopeAccessor(IAmbientEfCoreScopeStack ambientEfCoreScopeStack)
    {
        _ambientEfCoreScopeStack = ambientEfCoreScopeStack;
    }

    public EfCoreScope? AmbientScope => (EfCoreScope?)_ambientEfCoreScopeStack.AmbientScope;

    IEfCoreScope? IEFCoreScopeAccessor.AmbientScope => _ambientEfCoreScopeStack.AmbientScope;
}
