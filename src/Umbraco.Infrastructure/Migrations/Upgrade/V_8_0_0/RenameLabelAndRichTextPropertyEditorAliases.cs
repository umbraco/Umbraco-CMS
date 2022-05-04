using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

public class RenameLabelAndRichTextPropertyEditorAliases : MigrationBase
{
    public RenameLabelAndRichTextPropertyEditorAliases(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        MigratePropertyEditorAlias("Umbraco.TinyMCEv3", Constants.PropertyEditors.Aliases.TinyMce);
        MigratePropertyEditorAlias("Umbraco.NoEdit", Constants.PropertyEditors.Aliases.Label);
    }

    private void MigratePropertyEditorAlias(string oldAlias, string newAlias)
    {
        List<DataTypeDto> dataTypes = GetDataTypes(oldAlias);

        foreach (DataTypeDto dataType in dataTypes)
        {
            dataType.EditorAlias = newAlias;
            Database.Update(dataType);
        }
    }

    private List<DataTypeDto> GetDataTypes(string editorAlias)
    {
        List<DataTypeDto>? dataTypes = Database.Fetch<DataTypeDto>(Sql()
            .Select<DataTypeDto>()
            .From<DataTypeDto>()
            .Where<DataTypeDto>(x => x.EditorAlias == editorAlias));
        return dataTypes;
    }
}
