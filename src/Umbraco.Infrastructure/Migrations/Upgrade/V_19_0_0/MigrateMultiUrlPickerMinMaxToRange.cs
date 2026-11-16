// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
///     Converts the Multi URL Picker's <c>minNumber</c>/<c>maxNumber</c> configuration into a single
///     <c>validationLimit</c> range.
/// </summary>
internal sealed class MigrateMultiUrlPickerMinMaxToRange : MigrateMinMaxToRangeMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MigrateMultiUrlPickerMinMaxToRange"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public MigrateMultiUrlPickerMinMaxToRange(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    protected override string EditorAlias => Constants.PropertyEditors.Aliases.MultiUrlPicker;

    /// <inheritdoc />
    protected override bool TryMigrateConfiguration(JsonObject configuration)
        => MigrateTopLevelRange(configuration, "minNumber", "maxNumber", "validationLimit", minZeroIsUnbounded: true, maxZeroIsUnbounded: true);
}
