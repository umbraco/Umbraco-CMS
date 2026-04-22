namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
/// Provides a way to check whether the database is read-only.
/// </summary>
public interface IDatabaseReadOnlyAccessor
{
    /// <summary>
    /// Gets a value indicating whether the database is read only.
    /// </summary>
    bool IsReadOnly();
}
