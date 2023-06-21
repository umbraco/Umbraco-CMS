using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Migrations;

/// <summary>
///     Implements <see cref="IMigrationContext" />.
/// </summary>
internal class MigrationContext : IMigrationContext
{
    private readonly List<Type> _postMigrations = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="MigrationContext" /> class.
    /// </summary>
    public MigrationContext(MigrationPlan plan, IUmbracoDatabase? database, ILogger<MigrationContext> logger)
    {
        Plan = plan;
        Database = database ?? throw new ArgumentNullException(nameof(database));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // this is only internally exposed
    [Obsolete("This will be removed in the V13, and replaced with a RebuildCache flag on the MigrationBase")]
    internal IReadOnlyList<Type> PostMigrations => _postMigrations;

    /// <inheritdoc />
    public ILogger<IMigrationContext> Logger { get; }

    public MigrationPlan Plan { get; }

    /// <inheritdoc />
    public IUmbracoDatabase Database { get; }

    /// <inheritdoc />
    public ISqlContext SqlContext => Database.SqlContext;

    /// <inheritdoc />
    public int Index { get; set; }

    /// <inheritdoc />
    public bool BuildingExpression { get; set; }


    /// <inheritdoc />
    [Obsolete("This will be removed in the V13, and replaced with a RebuildCache flag on the MigrationBase, and a UmbracoPlanExecutedNotification.")]
    public void AddPostMigration<TMigration>()
        where TMigration : MigrationBase =>

        // just adding - will be de-duplicated when executing
        _postMigrations.Add(typeof(TMigration));
}
