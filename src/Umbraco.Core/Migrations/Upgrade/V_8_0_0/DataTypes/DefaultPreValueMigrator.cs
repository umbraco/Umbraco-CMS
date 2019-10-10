using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0.DataTypes
{
    class DefaultPreValueMigrator : IPreValueMigrator
    {
        public virtual bool CanMigrate(string editorAlias)
            => true;

        public virtual string GetNewAlias(string editorAlias)
            => editorAlias;

        public object GetConfiguration(int dataTypeId, string editorAlias, IEnumerable<PreValueDto> preValues)
        {
            var aliases = preValues.Select(x => x.Alias).Distinct().ToArray();
            if (aliases.Length == 1 && string.IsNullOrWhiteSpace(aliases[0]))
            {
                // array-based prevalues
                return new Dictionary<string, object> { ["values"] = preValues.OrderBy(x => x.SortOrder).Select(x => x.Value).ToArray() };
            }

            // assuming we don't want to fall back to array
            if (aliases.Length != preValues.Count() || aliases.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException($"Cannot migrate datatype w/ id={dataTypeId} preValues: duplicate or null/empty alias.");

            // dictionary-base prevalues
            return GetPreValues(preValues).ToDictionary(x => x.Alias, GetPreValueValue);
        }

        protected virtual IEnumerable<PreValueDto> GetPreValues(IEnumerable<PreValueDto> preValues)
            => preValues;

        protected virtual object GetPreValueValue(PreValueDto preValue)
        {
            return preValue.Value.DetectIsJson() ? JsonConvert.DeserializeObject(preValue.Value) : preValue.Value;
        }
    }
}
