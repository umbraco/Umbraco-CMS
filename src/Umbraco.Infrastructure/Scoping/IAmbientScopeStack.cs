namespace Umbraco.Cms.Infrastructure.Scoping;

internal interface IAmbientScopeStack : IScopeAccessor
{
    IScope Pop();
    void Push(IScope scope);
}
