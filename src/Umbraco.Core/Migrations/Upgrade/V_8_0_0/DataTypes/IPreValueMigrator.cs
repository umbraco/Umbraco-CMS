using System.Collections.Generic;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0.DataTypes
{
    public interface IPreValueMigrator
    {
        bool CanMigrate(string editorAlias);

        object GetConfiguration(int dataTypeId, string editorAlias, Dictionary<string, PreValueDto> preValues);
    }
}
