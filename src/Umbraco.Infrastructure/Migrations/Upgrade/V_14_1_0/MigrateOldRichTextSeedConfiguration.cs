using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_1_0;

public class MigrateOldRichTextSeedConfiguration : MigrationBase
{
    private const string OldSeedValue =
        "{\"value\":\",code,undo,redo,cut,copy,mcepasteword,stylepicker,bold,italic,bullist,numlist,outdent,indent,mcelink,unlink,mceinsertanchor,mceimage,umbracomacro,mceinserttable,umbracoembed,mcecharmap,|1|1,2,3,|0|500,400|1049,|true|\"}";

    private const string NewDefaultValue =
        "{\"toolbar\":[\"styles\",\"bold\",\"italic\",\"alignleft\",\"aligncenter\",\"alignright\",\"bullist\",\"numlist\",\"outdent\",\"indent\",\"sourcecode\",\"link\",\"umbmediapicker\",\"umbembeddialog\"],\"mode\":\"Classic\",\"maxImageSize\":500}";
    public MigrateOldRichTextSeedConfiguration(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        Sql<ISqlContext> sql = Sql()
            .Select<DataTypeDto>()
            .From<DataTypeDto>()
            .Where<DataTypeDto>(x =>
                x.EditorAlias.Equals(Constants.PropertyEditors.Aliases.RichText)
                && x.Configuration == OldSeedValue);

        List<DataTypeDto> dataTypeDtos = Database.Fetch<DataTypeDto>(sql);

        foreach (DataTypeDto dataTypeDto in dataTypeDtos)
        {
            // Update the configuration
            dataTypeDto.Configuration = NewDefaultValue;
            Database.Update(dataTypeDto);
        }
    }
}
