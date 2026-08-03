using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Migrations;

/// <summary>
///     Implements <see cref="IMigrationContext" />.
/// </summary>
internal sealed class MigrationContext : IMigrationContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MigrationContext" /> class.
    /// </summary>
    /// <param name="plan">The <see cref="MigrationPlan"/> that defines the migration steps to execute.</param>
    /// <param name="database">The <see cref="IUmbracoDatabase"/> instance to use for database operations during migrations, or <c>null</c> if not required.</param>
    /// <param name="logger">The <see cref="ILogger{MigrationContext}"/> instance used for logging migration activities.</param>
    public MigrationContext(MigrationPlan plan, IUmbracoDatabase? database, ILogger<MigrationContext> logger)
    {
        Plan = plan;
        Database = database ?? throw new ArgumentNullException(nameof(database));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public ILogger<IMigrationContext> Logger { get; }

    /// <summary>
    /// Gets the <see cref="MigrationPlan"/> that defines the sequence of migrations for this context.
    /// </summary>
    public MigrationPlan Plan { get; }

    /// <inheritdoc />
    public IUmbracoDatabase Database { get; internal set; }

    /// <inheritdoc />
    public ISqlContext SqlContext => Database.SqlContext;

    /// <inheritdoc />
    public int Index { get; set; }

    /// <inheritdoc />
    public bool BuildingExpression { get; set; }

    /// <summary>
    /// Gets a value indicating whether the migration has been completed.
    /// </summary>
    public bool IsCompleted { get; private set; } = false;

    /// <summary>
    /// Marks the migration context as completed.
    /// </summary>
    public void Complete() => IsCompleted = true;
}
