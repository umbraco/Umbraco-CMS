using System;
using System.Linq;
using Umbraco.Core.Exceptions;

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
                    throw new PanicException($"The alias {editorAlias} is not supported");
            }
        }
    }
}
