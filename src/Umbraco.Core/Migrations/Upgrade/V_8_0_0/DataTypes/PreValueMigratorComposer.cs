﻿using Umbraco.Core.Composing;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0.DataTypes
{
[RuntimeLevel(MinLevel = RuntimeLevel.Upgrade, MaxLevel = RuntimeLevel.Upgrade)] // only on upgrades
public class PreValueMigratorComposer : ICoreComposer
{
    public void Compose(Composition composition)
    {
        // do NOT add DefaultPreValueMigrator to this list!
        // it will be automatically used if nothing matches

        composition.WithCollectionBuilder<PreValueMigratorCollectionBuilder>()
            .Append<RenamingPreValueMigrator>()
            .Append<RichTextPreValueMigrator>()
            .Append<UmbracoSliderPreValueMigrator>()
            .Append<MediaPickerPreValueMigrator>()
            .Append<ContentPickerPreValueMigrator>()
            .Append<NestedContentPreValueMigrator>()
            .Append<DecimalPreValueMigrator>()
            .Append<ListViewPreValueMigrator>()
            .Append<DropDownFlexiblePreValueMigrator>()
            .Append<ValueListPreValueMigrator>()
            .Append<MarkdownEditorPreValueMigrator>();
    }
}
}
