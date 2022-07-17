using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0.DataTypes;

internal class UmbracoSliderPreValueMigrator : PreValueMigratorBase
{
    public override bool CanMigrate(string editorAlias)
        => editorAlias == "Umbraco.Slider";

    public override object GetConfiguration(int dataTypeId, string editorAlias,
        Dictionary<string, PreValueDto> preValues) =>
        new SliderConfiguration
        {
            EnableRange = GetBoolValue(preValues, "enableRange"),
            InitialValue = GetDecimalValue(preValues, "initVal1"),
            InitialValue2 = GetDecimalValue(preValues, "initVal2"),
            MaximumValue = GetDecimalValue(preValues, "maxVal"),
            MinimumValue = GetDecimalValue(preValues, "minVal"),
            StepIncrements = GetDecimalValue(preValues, "step"),
        };
}
