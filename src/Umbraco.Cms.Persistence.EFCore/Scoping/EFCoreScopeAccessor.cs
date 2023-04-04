using Microsoft.EntityFrameworkCore;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

internal class EFCoreScopeAccessor<TDbContext> : IEFCoreScopeAccessor<TDbContext> where TDbContext : DbContext
{
    private readonly IAmbientEfCoreScopeStack<TDbContext> _ambientEfCoreScopeStack;

    public EFCoreScopeAccessor(IAmbientEfCoreScopeStack<TDbContext> ambientEfCoreScopeStack) => _ambientEfCoreScopeStack = ambientEfCoreScopeStack;

    public EfCoreScope<TDbContext>? AmbientScope => (EfCoreScope<TDbContext>?)_ambientEfCoreScopeStack.AmbientScope;

    IEfCoreScope<TDbContext>? IEFCoreScopeAccessor<TDbContext>.AmbientScope => _ambientEfCoreScopeStack.AmbientScope;
}
