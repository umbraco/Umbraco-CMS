using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Index;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_13_0_0;

public class AddGuidsToUserGroups : MigrationBase
{
    public AddGuidsToUserGroups(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();

        // SQL server can simply add the column, but for SQLite this won't work,
        // so we'll have to create a new table and copy over data.
        if (DatabaseType != DatabaseType.SQLite)
        {
            AddColumnIfNotExists<UserGroupDto>(columns, "uniqueId");
        }

        // Rename the table to something that's unlikely to conflict with custom tables
        var tableName = Constants.DatabaseSchema.Tables.UserGroup;
        var oldTableName = tableName + "UmbracoThirteenZeroGuid";
        Rename
            .Table(tableName)
            .To(oldTableName)
            .Do();

        // GetDefinedIndexes returns indexes as a tuple (TableName, IndexName, ColumnName, IsUnique)
        var indexNames = SqlSyntax.GetDefinedIndexes(Database)
            .Where(x => x.Item1 == oldTableName)
            .Select(x => x.Item2);

        // We have to delete the existing indexes to be able to recreate the table since that also creates the indexes.
        foreach (var indexName in indexNames)
        {
            Delete
                .Index(indexName)
                .OnTable(oldTableName)
                .Do();
        }

        // Now we can create the table again with all the right columns and then migrate the data
        Create.Table<UserGroupDto>().Do();
    }
}
