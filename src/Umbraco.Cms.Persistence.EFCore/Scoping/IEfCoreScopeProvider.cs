using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

public interface IEfCoreScopeProvider
{
    IEfCoreScope CreateScope();

    IEfCoreScope CreateDetachedScope();

    void AttachScope(IEfCoreScope other);

    IEfCoreScope DetachScope();

    IScopeContext? AmbientScopeContext { get; }
}
