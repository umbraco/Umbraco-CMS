using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

public interface IEFCoreScopeProvider<TDbContext>
{
    IEfCoreScope<TDbContext> CreateScope(RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified, bool? scopeFileSystems = null);

    IEfCoreScope<TDbContext> CreateDetachedScope(RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified, bool? scopeFileSystems = null);

    void AttachScope(IEfCoreScope<TDbContext> other);

    IEfCoreScope<TDbContext> DetachScope();

    IScopeContext? AmbientScopeContext { get; }
}
