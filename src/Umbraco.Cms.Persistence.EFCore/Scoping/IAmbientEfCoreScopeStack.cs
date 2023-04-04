using Microsoft.EntityFrameworkCore;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

internal interface IAmbientEfCoreScopeStack<TDbContext> : IEFCoreScopeAccessor<TDbContext> where TDbContext : DbContext
{
    public IEfCoreScope<TDbContext>? AmbientScope { get; }

    IEfCoreScope<TDbContext> Pop();

    void Push(IEfCoreScope<TDbContext> scope);
}
