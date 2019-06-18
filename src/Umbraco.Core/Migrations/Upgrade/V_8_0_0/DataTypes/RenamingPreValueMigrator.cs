using System;
using System.Linq;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0.DataTypes
{
    class RenamingPreValueMigrator : DefaultPreValueMigrator
    {
        private readonly string[] _editors =
        {
            "Umbraco.NoEdit"
        };

        public override bool CanMigrate(string editorAlias)
            => _editors.Contains(editorAlias);

        public override string GetNewAlias(string editorAlias)
        {
            switch (editorAlias)
            {
                case "Umbraco.NoEdit":
                    return Constants.PropertyEditors.Aliases.Label;
                default:
                    throw new Exception("panic");
            }
        }
    }
}
