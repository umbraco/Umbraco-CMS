using System.Collections.Generic;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0
{
    public class RenameLabelAndRichTextPropertyEditorAliases : MigrationBase
    {
        public RenameLabelAndRichTextPropertyEditorAliases(IMigrationContext context)
            : base(context)
        {
        }

        public override void Migrate()
        {
            MigratePropertyEditorAlias("Umbraco.TinyMCEv3", Cms.Core.Constants.PropertyEditors.Aliases.TinyMce);
            MigratePropertyEditorAlias("Umbraco.NoEdit", Cms.Core.Constants.PropertyEditors.Aliases.Label);
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
