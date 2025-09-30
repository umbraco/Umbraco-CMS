using Microsoft.EntityFrameworkCore;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

internal sealed class EFCoreScopeAccessor<TDbContext> : IEFCoreScopeAccessor<TDbContext> where TDbContext : DbContext
{
    private readonly IAmbientEFCoreScopeStack<TDbContext> _ambientEfCoreScopeStack;

    public EFCoreScopeAccessor(IAmbientEFCoreScopeStack<TDbContext> ambientEfCoreScopeStack) => _ambientEfCoreScopeStack = ambientEfCoreScopeStack;

    public EFCoreScope<TDbContext>? AmbientScope => (EFCoreScope<TDbContext>?)_ambientEfCoreScopeStack.AmbientScope;

    IEfCoreScope<TDbContext>? IEFCoreScopeAccessor<TDbContext>.AmbientScope => _ambientEfCoreScopeStack.AmbientScope;
}
