using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_1_0;

/// <summary>
/// Migrates the rich text editor configuration as part of the upgrade to version 14.1.0.
/// </summary>
public class MigrateRichTextConfiguration : MigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MigrateRichTextConfiguration"/> class with the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to be used for the migration.</param>
    public MigrateRichTextConfiguration(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        Sql<ISqlContext> sql = Sql()
            .Select<DataTypeDto>()
            .From<DataTypeDto>()
            .Where<DataTypeDto>(x => x.EditorAlias.Equals(Constants.PropertyEditors.Aliases.RichText));

        List<DataTypeDto> dataTypeDtos = Database.Fetch<DataTypeDto>(sql);

        foreach (DataTypeDto dataTypeDto in dataTypeDtos)
        {
            // Update the configuration
            dataTypeDto.Configuration = dataTypeDto.Configuration?.Replace("\"ace", "\"sourcecode");
            Database.Update(dataTypeDto);
        }
    }
}
