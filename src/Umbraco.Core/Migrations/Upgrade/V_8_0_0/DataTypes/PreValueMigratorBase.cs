using System.Collections.Generic;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0.DataTypes
{
    public abstract class PreValueMigratorBase : IPreValueMigrator
    {
        public abstract bool CanMigrate(string editorAlias);

        public abstract object GetConfiguration(int dataTypeId, string editorAlias, Dictionary<string, PreValueDto> preValues);

        protected bool GetBoolValue(Dictionary<string, PreValueDto> preValues, string alias, bool defaultValue = false)
            => preValues.TryGetValue(alias, out var preValue) ? preValue.Value == "1" : defaultValue;

        protected decimal GetDecimalValue(Dictionary<string, PreValueDto> preValues, string alias, decimal defaultValue = 0)
            => preValues.TryGetValue(alias, out var preValue) && decimal.TryParse(preValue.Value, out var value) ? value : defaultValue;
    }
}
