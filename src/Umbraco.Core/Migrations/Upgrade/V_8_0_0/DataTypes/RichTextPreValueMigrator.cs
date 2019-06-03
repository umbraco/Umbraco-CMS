using Newtonsoft.Json;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0.DataTypes
{
    class RichTextPreValueMigrator : DefaultPreValueMigrator
    {
        public override bool CanMigrate(string editorAlias)
            => editorAlias == "Umbraco.TinyMCEv3";

        public override string GetNewAlias(string editorAlias)
            => Constants.PropertyEditors.Aliases.TinyMce;

        protected override object GetPreValueValue(PreValueDto preValue)
        {
            if (preValue.Alias == "hideLabel")
                return preValue.Value == "1";

            return preValue.Value.DetectIsJson() ? JsonConvert.DeserializeObject(preValue.Value) : preValue.Value;
        }
    }
}