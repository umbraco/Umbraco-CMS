namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0.DataTypes
{
    class ContentPickerPreValueMigrator : DefaultPreValueMigrator
    {
        public override bool CanMigrate(string editorAlias)
            => editorAlias == "Umbraco.ContentPicker2";

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
