using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

public interface IEfCoreScopeProvider
{
    IEfCoreScope CreateScope(RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified, bool? scopeFileSystems = null);

    IEfCoreScope CreateDetachedScope(RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified, bool? scopeFileSystems = null);

    void AttachScope(IEfCoreScope other);

    IEfCoreScope DetachScope();

    IScopeContext? AmbientScopeContext { get; }
}
