using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0.DataTypes;

internal class ContentPickerPreValueMigrator : DefaultPreValueMigrator
{
    public override bool CanMigrate(string editorAlias)
        => editorAlias == Constants.PropertyEditors.Legacy.Aliases.ContentPicker2;

    public override string? GetNewAlias(string editorAlias)
        => null;

    protected override object? GetPreValueValue(PreValueDto preValue)
    {
        if (preValue.Alias == "showOpenButton" ||
            preValue.Alias == "ignoreUserStartNodes")
        {
            return preValue.Value == "1";
        }

        return base.GetPreValueValue(preValue);
    }
}
