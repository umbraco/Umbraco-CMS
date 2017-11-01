using System.Linq;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Migrations.Syntax.Execute;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionEight
{
    [Migration("8.0.0", 400, Constants.System.UmbracoMigrationName)]
    public class VariantsMigration : MigrationBase
    {
        public VariantsMigration(IMigrationContext context)
            : base(context)
        { }

        public override void Up()
        {
            // delete keys and indexes
            Execute.DropKeysAndIndexes(Constants.DatabaseSchema.Tables.PropertyData);

            // fixme is this enough or shall we drop *all* keys and indexes?

            MigratePropertyData();

            // re-create keys and indexes
            Create.KeysAndIndexes<PropertyDataDto>();
        }

        private void MigrateContentDocumentAndVersion()
        {
            // todo
            // add a varyBy field that's an enum (invariant, culture, segment)
        }

        private void MigratePropertyData()
        {
            // add column propertyData.languageId
            if (!ColumnExists(Constants.DatabaseSchema.Tables.PropertyData, "languageId"))
                Alter.Table(Constants.DatabaseSchema.Tables.PropertyData).AddColumn("languageId").AsInt32().Nullable();

            // add column propertyData.segment
            if (!ColumnExists(Constants.DatabaseSchema.Tables.PropertyData, "segment"))
                Alter.Table(Constants.DatabaseSchema.Tables.PropertyData).AddColumn("segment").AsString(256).Nullable();

            // rename column propertyData.contentNodeId to nodeId
            if (ColumnExists(Constants.DatabaseSchema.Tables.PropertyData, "contentNodeId"))
                Rename.Column("contentNodeId").OnTable(Constants.DatabaseSchema.Tables.PropertyData).To("nodeId");

            // rename column propertyData.dataNtext to textValue
            // rename column propertyData.dataNvarchar to varcharValue
            // rename column propertyData.dataDecimal to decimalValue
            // rename column propertyData.dataInt to intValue
            // rename column propertyData.dataDate to dateValue
            if (ColumnExists(Constants.DatabaseSchema.Tables.PropertyData, "dataNtext"))
                Rename.Column("dataNtext").OnTable(Constants.DatabaseSchema.Tables.PropertyData).To("textValue");
            if (ColumnExists(Constants.DatabaseSchema.Tables.PropertyData, "dataNvarchar"))
                Rename.Column("dataNtext").OnTable(Constants.DatabaseSchema.Tables.PropertyData).To("varcharValue");
            if (ColumnExists(Constants.DatabaseSchema.Tables.PropertyData, "dataDecimal"))
                Rename.Column("dataDecimal").OnTable(Constants.DatabaseSchema.Tables.PropertyData).To("decimalValue");
            if (ColumnExists(Constants.DatabaseSchema.Tables.PropertyData, "dataInt"))
                Rename.Column("dataInt").OnTable(Constants.DatabaseSchema.Tables.PropertyData).To("intValue");
            if (ColumnExists(Constants.DatabaseSchema.Tables.PropertyData, "dataDate"))
                Rename.Column("dataDate").OnTable(Constants.DatabaseSchema.Tables.PropertyData).To("dateValue");
        }

        private bool ColumnExists(string tableName, string columnName)
        {
            // that's ok even on MySql
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).Distinct().ToArray();
            return columns.Any(x => x.TableName.InvariantEquals(tableName) && x.ColumnName.InvariantEquals(columnName));
        }
    }
}
