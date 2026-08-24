namespace Umbraco.Cms.Core.SchemaLockdown;

/// <summary>
/// Builds the schema lockdown matrix once and caches it.
/// </summary>
internal class SchemaLockdownMatrixAccessor : ISchemaLockdownMatrixAccessor
{
    private readonly Lazy<SchemaLockdownMatrix> _matrix;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaLockdownMatrixAccessor"/> class.
    /// </summary>
    /// <param name="configurators">The registered configurators.</param>
    public SchemaLockdownMatrixAccessor(SchemaLockdownConfiguratorCollection configurators)
        => _matrix = new Lazy<SchemaLockdownMatrix>(() => Build(configurators));

    /// <inheritdoc />
    public SchemaLockdownMatrix Matrix => _matrix.Value;

    private static SchemaLockdownMatrix Build(SchemaLockdownConfiguratorCollection configurators)
    {
        var matrix = new SchemaLockdownMatrix();

        foreach (ISchemaLockdownConfigurator configurator in configurators)
        {
            configurator.Configure(matrix);
        }

        matrix.Freeze();
        return matrix;
    }
}
