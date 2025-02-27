using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

public class RenameTechnologyLeakingPropertyEditorAliases : MigrationBase
{
    public RenameTechnologyLeakingPropertyEditorAliases(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        MigratePropertyEditorAlias("Umbraco.TinyMCE", Constants.PropertyEditors.Aliases.RichText);
    }

    private void MigratePropertyEditorAlias(string propertyEditorAlias, string newAlias)
    {
        List<DataTypeDto> propertyEditors = Database.Fetch<DataTypeDto>().FindAll(x => x.EditorAlias == propertyEditorAlias);

        foreach (DataTypeDto propertyEditor in propertyEditors)
        {
            propertyEditor.EditorAlias = newAlias;
            Database.Update(propertyEditor);
        }
    }
}
