// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
///     Converts the Multiple Text String's <c>min</c>/<c>max</c> configuration into a single <c>validationLimit</c> range.
/// </summary>
internal sealed class MigrateMultipleTextStringMinMaxToRange : MigrateMinMaxToRangeMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MigrateMultipleTextStringMinMaxToRange"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public MigrateMultipleTextStringMinMaxToRange(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    protected override string EditorAlias => Constants.PropertyEditors.Aliases.MultipleTextstring;

    /// <inheritdoc />
    protected override bool TryMigrateConfiguration(JsonObject configuration)
        => MigrateTopLevelRange(configuration, "min", "max", "validationLimit", minZeroIsUnbounded: true, maxZeroIsUnbounded: true);
}
