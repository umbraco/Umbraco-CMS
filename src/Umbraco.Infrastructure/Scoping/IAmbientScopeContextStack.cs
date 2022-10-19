using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Infrastructure.Scoping;

internal interface IAmbientScopeContextStack
{
    IScopeContext? AmbientContext { get; }
    IScopeContext Pop();
    void Push(IScopeContext scope);
}
