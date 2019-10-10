using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0.DataTypes
{
    public abstract class PreValueMigratorBase : IPreValueMigrator
    {
        public abstract bool CanMigrate(string editorAlias);

        public virtual string GetNewAlias(string editorAlias)
            => editorAlias;

        public abstract object GetConfiguration(int dataTypeId, string editorAlias, IEnumerable<PreValueDto> preValues);

        protected bool GetBoolValue(IEnumerable<PreValueDto> preValues, string alias, bool defaultValue = false)
        {
            var preValue = preValues.FirstOrDefault(p => p.Alias == alias);
            return preValue != null ? (preValue.Value == "1") : defaultValue;
        }

        protected decimal GetDecimalValue(IEnumerable<PreValueDto> preValues, string alias, decimal defaultValue = 0)
        {
            var preValue = preValues.FirstOrDefault(p => p.Alias == alias);
            return preValue != null && decimal.TryParse(preValue.Value, out var value) ? value : defaultValue;
        }
    }
}
