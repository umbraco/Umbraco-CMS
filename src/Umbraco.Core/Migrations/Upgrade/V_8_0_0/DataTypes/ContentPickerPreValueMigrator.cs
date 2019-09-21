using System.Linq;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0.DataTypes
{
    class ContentPickerPreValueMigrator : DefaultPreValueMigrator
    {
        private readonly string[] _editors =
        {
            Constants.PropertyEditors.Legacy.Aliases.ContentPicker2,
            Constants.PropertyEditors.Legacy.Aliases.ContentPicker
        };

        public override bool CanMigrate(string editorAlias)
            => _editors.Contains(editorAlias);

        public override string GetNewAlias(string editorAlias)
            => null;

        protected override object GetPreValueValue(PreValueDto preValue)
        {
            if (preValue.Alias == "showOpenButton" ||
                preValue.Alias == "ignoreUserStartNodes")
                return preValue.Value == "1";

            return base.GetPreValueValue(preValue);
        }
    }
}
