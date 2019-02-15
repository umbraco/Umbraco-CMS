using System.Collections.Generic;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class RenameLabelAndRichTextPropertyEditorAliases : MigrationBase
    {
        public RenameLabelAndRichTextPropertyEditorAliases(IMigrationContext context)
            : base(context)
        {
        }

        public override void Migrate()
        {
            MigratePropertyEditorAlias("Umbraco.TinyMCEv3", Constants.PropertyEditors.Aliases.TinyMce);
            MigratePropertyEditorAlias("Umbraco.NoEdit", Constants.PropertyEditors.Aliases.Label);
        }

        private void MigratePropertyEditorAlias(string oldAlias, string newAlias)
        {
            var dataTypes = GetDataTypes(oldAlias);

            foreach (var dataType in dataTypes)
            {
                dataType.EditorAlias = newAlias;
                Database.Update(dataType);
            }
        }

        private List<DataTypeDto> GetDataTypes(string editorAlias)
        {
            var dataTypes = Database.Fetch<DataTypeDto>(Sql()
                .Select<DataTypeDto>()
                .From<DataTypeDto>()
                .Where<DataTypeDto>(x => x.EditorAlias == editorAlias));
            return dataTypes;
        }

    }
}
