using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Infrastructure.Scoping;

public interface IAmbientScopeContextStack
{
    IScopeContext? AmbientContext { get; }
    IScopeContext Pop();
    void Push(IScopeContext scope);
}
