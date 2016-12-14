namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Provides access to DatabaseScope.
    /// </summary>
    public interface IDatabaseScopeAccessor
    {
        DatabaseScope Scope { get; set; }
    }
}