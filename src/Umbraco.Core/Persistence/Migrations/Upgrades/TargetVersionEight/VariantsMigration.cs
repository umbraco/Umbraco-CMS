using System;
using System.Linq;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Migrations.Initial;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter.Column;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter.Table;
using Umbraco.Core.Persistence.Migrations.Syntax.Execute;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionEight
{
    [Migration("8.0.0", 1000, Constants.System.UmbracoMigrationName)]
    public class VariantsMigration : MigrationBase
    {
        public VariantsMigration(IMigrationContext context)
            : base(context)
        { }

        public override void Up()
        {
            // delete *all* keys and indexes - because of FKs
            //Execute.DropKeysAndIndexes(PreTables.PropertyData);
            Execute.DropKeysAndIndexes();

            MigratePropertyData();

            // re-create *all* keys and indexes
            //Create.KeysAndIndexes<PropertyDataDto>();
            foreach (var x in DatabaseSchemaCreation.OrderedTables)
                Create.KeysAndIndexes(x.Value);
        }

        private void MigratePropertyData()
        {
            // if the table has already been renamed, we're done
            if (TableExists(Constants.DatabaseSchema.Tables.PropertyData))
                return;

            // add column propertyData.languageId
            if (!ColumnExists(PreTables.PropertyData, "languageId"))
                AddColumn<PropertyDataDto>(PreTables.PropertyData, "languageId");

            // add column propertyData.segment
            if (!ColumnExists(PreTables.PropertyData, "segment"))
                AddColumn<PropertyDataDto>(PreTables.PropertyData, "segment");

            // do NOT use Rename.Column as it's borked on SQLCE - use ReplaceColumn instead

            // rename column propertyData.contentNodeId to nodeId
            if (ColumnExists(PreTables.PropertyData, "contentNodeId"))
                ReplaceColumn<PropertyDataDto>(PreTables.PropertyData, "contentNodeId", "nodeId");

            // rename column propertyData.dataNtext to textValue
            // rename column propertyData.dataNvarchar to varcharValue
            // rename column propertyData.dataDecimal to decimalValue
            // rename column propertyData.dataInt to intValue
            // rename column propertyData.dataDate to dateValue
            if (ColumnExists(PreTables.PropertyData, "dataNtext"))
                ReplaceColumn<PropertyDataDto>(PreTables.PropertyData, "dataNtext", "textValue");
            if (ColumnExists(PreTables.PropertyData, "dataNvarchar"))
                ReplaceColumn<PropertyDataDto>(PreTables.PropertyData, "dataNvarchar", "varcharValue");
            if (ColumnExists(PreTables.PropertyData, "dataDecimal"))
                ReplaceColumn<PropertyDataDto>(PreTables.PropertyData, "dataDecimal", "decimalValue");
            if (ColumnExists(PreTables.PropertyData, "dataInt"))
                ReplaceColumn<PropertyDataDto>(PreTables.PropertyData, "dataInt", "intValue");
            if (ColumnExists(PreTables.PropertyData, "dataDate"))
                ReplaceColumn<PropertyDataDto>(PreTables.PropertyData, "dataDate", "dateValue");

            // rename table
            Rename.Table(PreTables.PropertyData).To(Constants.DatabaseSchema.Tables.PropertyData);
        }

        private static class PreTables
        {
            public const string Lock = "umbracoLock";
            public const string Log = "umbracoLog";

            public const string Node = "umbracoNode";
            public const string NodeData = "cmsContentNu";
            public const string NodeXml = "cmsContentXml";
            public const string NodePreviewXml = "cmsPreviewXml";

            public const string ContentType = "cmsContentType";
            public const string ContentChildType = "cmsContentTypeAllowedContentType";
            public const string DocumentType = "cmsDocumentType";
            public const string ElementTypeTree = "cmsContentType2ContentType";
            public const string DataType = "cmsDataType";
            public const string DataTypePreValue = "cmsDataTypePreValues";
            public const string Template = "cmsTemplate";

            public const string Content = "cmsContent";
            public const string ContentVersion = "cmsContentVersion";
            public const string Document = "cmsDocument";

            public const string PropertyType = "cmsPropertyType";
            public const string PropertyTypeGroup = "cmsPropertyTypeGroup";
            public const string PropertyData = "cmsPropertyData";

            public const string RelationType = "umbracoRelationType";
            public const string Relation = "umbracoRelation";

            public const string Domain = "umbracoDomains";
            public const string Language = "umbracoLanguage";
            public const string DictionaryEntry = "cmsDictionary";
            public const string DictionaryValue = "cmsLanguageText";

            public const string User = "umbracoUser";
            public const string UserGroup = "umbracoUserGroup";
            public const string UserStartNode = "umbracoUserStartNode";
            public const string User2UserGroup = "umbracoUser2UserGroup";
            public const string User2NodeNotify = "umbracoUser2NodeNotify";
            public const string UserGroup2App = "umbracoUserGroup2App";
            public const string UserGroup2NodePermission = "umbracoUserGroup2NodePermission";
            public const string ExternalLogin = "umbracoExternalLogin";

            public const string Macro = "cmsMacro";
            public const string MacroProperty = "cmsMacroProperty";

            public const string Member = "cmsMember";
            public const string MemberType = "cmsMemberType";
            public const string Member2MemberGroup = "cmsMember2MemberGroup";

            public const string Access = "umbracoAccess";
            public const string AccessRule = "umbracoAccessRule";
            public const string RedirectUrl = "umbracoRedirectUrl";

            public const string CacheInstruction = "umbracoCacheInstruction";
            public const string Migration = "umbracoMigration";
            public const string Server = "umbracoServer";

            public const string Tag = "cmsTags";
            public const string TagRelationship = "cmsTagRelationship";

            public const string Task = "cmsTask";
            public const string TaskType = "cmsTaskType";
        }

        private void AddColumn<T>(string tableName, string columnName)
        {
            AddColumn<T>(tableName, columnName, out var notNull);
            if (notNull != null) Execute.Sql(notNull);
        }

        private void AddColumn<T>(string tableName, string columnName, out string notNull)
        {
            if (ColumnExists(tableName, columnName))
                throw new InvalidOperationException($"Column {tableName}.{columnName} already exists.");

            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
            var column = table.Columns.First(x => x.Name == columnName);
            var create = SqlSyntax.Format(column);
            // some db cannot add a NOT NULL column, so change it into NULL
            if (create.Contains("NOT NULL"))
            {
                notNull = string.Format(SqlSyntax.AlterColumn, SqlSyntax.GetQuotedTableName(tableName), create);
                create = create.Replace("NOT NULL", "NULL");
            }
            else
            {
                notNull = null;
            }
            Execute.Sql($"ALTER TABLE {SqlSyntax.GetQuotedTableName(tableName)} ADD COLUMN " + create);
        }

        private void ReplaceColumn<T>(string tableName, string currentName, string newName)
        {
            AddColumn<T>(tableName, newName, out var notNull);
            Execute.Sql($"UPDATE {SqlSyntax.GetQuotedTableName(tableName)} SET {SqlSyntax.GetQuotedColumnName(newName)}={SqlSyntax.GetQuotedColumnName(currentName)}");
            if (notNull != null) Execute.Sql(notNull);
            Delete.Column(currentName).FromTable(tableName);
        }

        private bool TableExists(string tableName)
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database);
            return tables.Any(x => x.InvariantEquals(tableName));
        }

        private bool ColumnExists(string tableName, string columnName)
        {
            // that's ok even on MySql
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).Distinct().ToArray();
            return columns.Any(x => x.TableName.InvariantEquals(tableName) && x.ColumnName.InvariantEquals(columnName));
        }
    }
}
