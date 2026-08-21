namespace Umbraco.Cms.Core.SchemaLockdown;

/// <summary>
/// Provides the frozen schema lockdown matrix.
/// </summary>
public interface ISchemaLockdownMatrixAccessor
{
    /// <summary>
    /// Gets the matrix, building it on first access.
    /// </summary>
    SchemaLockdownMatrix Matrix { get; }
}
