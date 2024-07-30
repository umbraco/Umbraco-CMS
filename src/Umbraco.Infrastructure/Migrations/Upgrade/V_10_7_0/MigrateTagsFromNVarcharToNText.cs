using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_10_7_0;

[Obsolete("This is no longer used and will be removed in V14.")]
public class MigrateTagsFromNVarcharToNText : MigrationBase
{
    public MigrateTagsFromNVarcharToNText(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        // Firstly change the storage type for the Umbraco.Tags property editor
        Sql<ISqlContext> updateDbTypeForTagsQuery = Database.SqlContext.Sql()
            .Update<DataTypeDto>(x => x.Set(dt => dt.DbType, ValueStorageType.Ntext.ToString()))
            .Where<DataTypeDto>(dt => dt.EditorAlias == Constants.PropertyEditors.Aliases.Tags);

        Database.Execute(updateDbTypeForTagsQuery);

        // Then migrate the data from "varcharValue" column to "textValue"
        if (Database.DatabaseType == DatabaseType.SQLite)
        {
            MigrateSqlLiteData();
        }
        else
        {
            MigrateSqlServerData();
        }
    }

    private void MigrateSqlLiteData()
    {
        Sql<ISqlContext> tagsDataTypeIdQuery = Database.SqlContext.Sql()
            .Select<DataTypeDto>(dt => dt.NodeId)
            .From<DataTypeDto>()
            .Where<DataTypeDto>(dt => dt.EditorAlias == Constants.PropertyEditors.Aliases.Tags);

        Sql<ISqlContext> tagsPropertyTypeIdQuery = Database.SqlContext.Sql()
            .Select<PropertyTypeDto>(pt => pt.Id)
            .From<PropertyTypeDto>()
            .WhereIn<PropertyTypeDto>(pt => pt.DataTypeId, tagsDataTypeIdQuery);

        Sql<ISqlContext> updatePropertyDataColumnsQuery = Database.SqlContext.Sql()
            .Update<PropertyDataDto>()
            .Append("SET textValue = varcharValue, varcharValue = null")
            .WhereIn<PropertyDataDto>(pd => pd.PropertyTypeId, tagsPropertyTypeIdQuery)
            .Where<PropertyDataDto>(pd => pd.TextValue == null)
            .Where<PropertyDataDto>(pd => pd.VarcharValue != null);

        Database.Execute(updatePropertyDataColumnsQuery);
    }

    private void MigrateSqlServerData()
    {
        Sql<ISqlContext> updateTagsValues = Database.SqlContext.Sql()
            .Update<PropertyDataDto>()
            .Append("SET textValue = COALESCE([textValue], [varCharValue]), varcharValue = null")
            .From<DataTypeDto>()
            .InnerJoin<PropertyTypeDto>().On<DataTypeDto, PropertyTypeDto>((dt, pt) => dt.NodeId == pt.DataTypeId)
            .InnerJoin<PropertyDataDto>().On<PropertyTypeDto, PropertyDataDto>((pt, pd) => pt.Id == pd.PropertyTypeId)
            .Where<DataTypeDto>(dt => dt.EditorAlias == Constants.PropertyEditors.Aliases.Tags);

        Database.Execute(updateTagsValues);
    }
}
