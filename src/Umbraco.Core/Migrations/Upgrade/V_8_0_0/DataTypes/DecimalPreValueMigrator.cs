using Newtonsoft.Json;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0.DataTypes
{
    class DecimalPreValueMigrator : DefaultPreValueMigrator
    {
        public override bool CanMigrate(string editorAlias)
            => editorAlias == "Umbraco.Decimal";

        protected override object GetPreValueValue(PreValueDto preValue)
        {
            if (preValue.Alias == "min" ||
                preValue.Alias == "step" ||
                preValue.Alias == "max")
                return decimal.TryParse(preValue.Value, out var d) ? (decimal?) d : null;

            return preValue.Value.DetectIsJson() ? JsonConvert.DeserializeObject(preValue.Value) : preValue.Value;
        }
    }
}
