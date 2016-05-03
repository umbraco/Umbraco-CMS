namespace Umbraco.Core
{
    /// <summary>
    /// Gets an IScopedContext
    /// </summary>
    internal interface IScopeContextFactory
    {
        IScopeContext GetContext();
    }
}