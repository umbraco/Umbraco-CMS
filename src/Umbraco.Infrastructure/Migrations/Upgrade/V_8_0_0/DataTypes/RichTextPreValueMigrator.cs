using Newtonsoft.Json;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0.DataTypes;

internal class RichTextPreValueMigrator : DefaultPreValueMigrator
{
    public override bool CanMigrate(string editorAlias)
        => editorAlias == "Umbraco.TinyMCEv3";

    public override string GetNewAlias(string editorAlias)
        => Constants.PropertyEditors.Aliases.TinyMce;

    protected override object? GetPreValueValue(PreValueDto preValue)
    {
        if (preValue.Alias == "hideLabel")
        {
            return preValue.Value == "1";
        }

        return preValue.Value?.DetectIsJson() ?? false ? JsonConvert.DeserializeObject(preValue.Value) : preValue.Value;
    }
}
