using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

public interface IEfCoreScopeProvider
{
    IEfCoreScope? CreateScope();

    IScopeContext? AmbientScopeContext { get; }
}
