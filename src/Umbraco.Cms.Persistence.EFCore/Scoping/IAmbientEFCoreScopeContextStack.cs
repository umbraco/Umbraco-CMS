using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

internal interface IAmbientEFCoreScopeContextStack
{
    IScopeContext? AmbientContext { get; }

    IScopeContext Pop();

    void Push(IScopeContext scope);
}
