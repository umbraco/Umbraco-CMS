using System.Linq;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0.DataTypes
{
    class MediaPickerPreValueMigrator : DefaultPreValueMigrator //PreValueMigratorBase
    {
        private readonly string[] _editors =
        {
            "Umbraco.MediaPicker2",
            "Umbraco.MediaPicker"
        };

        public override bool CanMigrate(string editorAlias)
            => _editors.Contains(editorAlias);

        public override string GetNewAlias(string editorAlias)
            => "Umbraco.MediaPicker";

        // you wish - but MediaPickerConfiguration lives in Umbraco.Web
        /*
        public override object GetConfiguration(int dataTypeId, string editorAlias, Dictionary<string, PreValueDto> preValues)
        {
            return new MediaPickerConfiguration { ... };
        }
        */

        protected override object GetPreValueValue(PreValueDto preValue)
        {
            if (preValue.Alias == "multiPicker" ||
                preValue.Alias == "onlyImages" ||
                preValue.Alias == "disableFolderSelect")
                return preValue.Value == "1";

            return base.GetPreValueValue(preValue);
        }
    }
}
