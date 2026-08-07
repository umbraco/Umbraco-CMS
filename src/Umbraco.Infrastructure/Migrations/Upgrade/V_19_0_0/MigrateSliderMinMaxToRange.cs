// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
///     Converts the Slider editor's <c>minVal</c>/<c>maxVal</c> configuration into a single <c>validationRange</c> range.
///     The minimum is preserved as-is (a value of zero is a real lower bound); a maximum of zero means "no maximum".
/// </summary>
internal sealed class MigrateSliderMinMaxToRange : MigrateMinMaxToRangeMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MigrateSliderMinMaxToRange"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public MigrateSliderMinMaxToRange(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    protected override string EditorAlias => Constants.PropertyEditors.Aliases.Slider;

    /// <inheritdoc />
    protected override bool TryMigrateConfiguration(JsonObject configuration)
        => MigrateTopLevelRange(configuration, "minVal", "maxVal", "validationRange", minZeroIsUnbounded: false, maxZeroIsUnbounded: true);
}
