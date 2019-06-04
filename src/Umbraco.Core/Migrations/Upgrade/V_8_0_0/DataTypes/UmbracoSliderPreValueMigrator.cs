using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0.DataTypes
{
    class UmbracoSliderPreValueMigrator : PreValueMigratorBase
    {
        public override bool CanMigrate(string editorAlias)
            => editorAlias == "Umbraco.Slider";

        public override object GetConfiguration(int dataTypeId, string editorAlias, Dictionary<string, PreValueDto> preValues)
        {
            return new SliderConfiguration
            {
                EnableRange = GetBoolValue(preValues, "enableRange"),
                InitialValue = GetDecimalValue(preValues, "initVal1"),
                InitialValue2 = GetDecimalValue(preValues, "initVal2"),
                MaximumValue = GetDecimalValue(preValues, "maxVal"),
                MinimumValue = GetDecimalValue(preValues, "minVal"),
                StepIncrements = GetDecimalValue(preValues, "step")
            };
        }
    }
}
