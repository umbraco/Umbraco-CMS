using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0.DataTypes
{
    class ListViewPreValueMigrator : DefaultPreValueMigrator
    {
        public override bool CanMigrate(string editorAlias)
            => editorAlias == "Umbraco.ListView";

        protected override IEnumerable<PreValueDto> GetPreValues(IEnumerable<PreValueDto> preValues)
        {
            return preValues.Where(preValue => preValue.Alias != "displayAtTabNumber");
        }

        protected override object GetPreValueValue(PreValueDto preValue)
        {
            if (preValue.Alias == "pageSize")
                return int.TryParse(preValue.Value, out var i) ? (int?)i : null;

            return preValue.Value.DetectIsJson() ? JsonConvert.DeserializeObject(preValue.Value) : preValue.Value;
        }
    }
}
