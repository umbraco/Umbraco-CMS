using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Migrations;

/// <summary>
///     Implements <see cref="IMigrationContext" />.
/// </summary>
internal class MigrationContext : IMigrationContext
{
    private readonly Action? _onCompleteAction;
    private readonly List<Type> _postMigrations = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="MigrationContext" /> class.
    /// </summary>
    public MigrationContext(MigrationPlan plan, IUmbracoDatabase? database, ILogger<MigrationContext> logger, Action? onCompleteAction = null)
    {
        _onCompleteAction = onCompleteAction;
        Plan = plan;
        Database = database ?? throw new ArgumentNullException(nameof(database));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public ILogger<IMigrationContext> Logger { get; }

    public MigrationPlan Plan { get; }

    /// <inheritdoc />
    public IUmbracoDatabase Database { get; internal set; }

    /// <inheritdoc />
    public ISqlContext SqlContext => Database.SqlContext;

    /// <inheritdoc />
    public int Index { get; set; }

    /// <inheritdoc />
    public bool BuildingExpression { get; set; }

    public bool IsCompleted { get; private set; } = false;

    public void Complete()
    {
        if (IsCompleted)
        {
            return;
        }

        _onCompleteAction?.Invoke();

        IsCompleted = true;
    }
}
