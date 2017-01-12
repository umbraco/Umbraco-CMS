namespace Umbraco.Core.Scoping
{
    public interface IScopeProvider
    {
        IScope CreateScope();
        IScope CreateDetachedScope();
        void AttachScope(IScope scope);
        IScope DetachScope();
    }
}
