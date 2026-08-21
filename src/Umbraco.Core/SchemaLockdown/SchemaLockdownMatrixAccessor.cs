using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

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
    /// <param name="settings">The schema lockdown settings.</param>
    /// <param name="configurators">The registered configurators.</param>
    public SchemaLockdownMatrixAccessor(
        IOptions<SchemaLockdownSettings> settings,
        SchemaLockdownConfiguratorCollection configurators)
        => _matrix = new Lazy<SchemaLockdownMatrix>(() => Build(settings.Value, configurators));

    /// <inheritdoc />
    public SchemaLockdownMatrix Matrix => _matrix.Value;

    private static SchemaLockdownMatrix Build(
        SchemaLockdownSettings settings,
        SchemaLockdownConfiguratorCollection configurators)
    {
        var matrix = new SchemaLockdownMatrix();

        if (settings.Enabled)
        {
            foreach (var configured in settings.LockedEntityTypes)
            {
                // A value that is not resolved cannot key the decision table, and start-up validation has already
                // reported it, so blocking on it here would only create a cell nothing ever looks up.
                if (SchemaEntityTypes.TryResolve(configured, out var entityType))
                {
                    matrix.BlockMutations(entityType);
                }
            }
        }

        foreach (ISchemaLockdownConfigurator configurator in configurators)
        {
            configurator.Configure(matrix);
        }

        matrix.Freeze();
        return matrix;
    }
}
