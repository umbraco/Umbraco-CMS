namespace Umbraco.Cms.Core.SchemaLockdown;

/// <summary>
/// Adjusts the schema lockdown matrix while it is being built at start-up.
/// </summary>
/// <remarks>
/// Every registered configurator runs, in collection order, against the same matrix. Later writes to a cell win,
/// so registering last is how a rule is made authoritative.
/// </remarks>
public interface ISchemaLockdownConfigurator
{
    /// <summary>
    /// Adjusts the supplied matrix.
    /// </summary>
    /// <param name="matrix">The matrix being built.</param>
    void Configure(SchemaLockdownMatrix matrix);
}
