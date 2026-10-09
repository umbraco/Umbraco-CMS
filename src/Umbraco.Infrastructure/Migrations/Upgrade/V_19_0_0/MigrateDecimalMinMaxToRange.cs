// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
///     Converts the Decimal editor's <c>min</c>/<c>max</c> configuration into a single <c>validationRange</c> range.
/// </summary>
internal sealed class MigrateDecimalMinMaxToRange : MigrateMinMaxToRangeMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MigrateDecimalMinMaxToRange"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public MigrateDecimalMinMaxToRange(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    protected override string EditorAlias => Constants.PropertyEditors.Aliases.Decimal;

    /// <inheritdoc />
    protected override bool TryMigrateConfiguration(JsonObject configuration)
        => MigrateTopLevelRange(configuration, "min", "max", "validationRange", minZeroIsUnbounded: true, maxZeroIsUnbounded: true);
}
