using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0.DataTypes;

internal class MarkdownEditorPreValueMigrator : DefaultPreValueMigrator // PreValueMigratorBase
{
    public override bool CanMigrate(string editorAlias)
        => editorAlias == Constants.PropertyEditors.Aliases.MarkdownEditor;

    protected override object? GetPreValueValue(PreValueDto preValue)
    {
        if (preValue.Alias == "preview")
        {
            return preValue.Value == "1";
        }

        return base.GetPreValueValue(preValue);
    }
}
