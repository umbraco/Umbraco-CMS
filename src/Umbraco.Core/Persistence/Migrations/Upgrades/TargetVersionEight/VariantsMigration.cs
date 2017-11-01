using System.Linq;
using Umbraco.Core.Persistence.Migrations.Initial;
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
                Alter.Table(PreTables.PropertyData).AddColumn("languageId").AsInt32().Nullable();

            // add column propertyData.segment
            if (!ColumnExists(PreTables.PropertyData, "segment"))
                Alter.Table(PreTables.PropertyData).AddColumn("segment").AsString(256).Nullable();

            // rename column propertyData.contentNodeId to nodeId
            if (ColumnExists(PreTables.PropertyData, "contentNodeId"))
                Rename.Column("contentNodeId").OnTable(PreTables.PropertyData).To("nodeId");

            // rename column propertyData.dataNtext to textValue
            // rename column propertyData.dataNvarchar to varcharValue
            // rename column propertyData.dataDecimal to decimalValue
            // rename column propertyData.dataInt to intValue
            // rename column propertyData.dataDate to dateValue
            if (ColumnExists(PreTables.PropertyData, "dataNtext"))
                Rename.Column("dataNtext").OnTable(PreTables.PropertyData).To("textValue");
            if (ColumnExists(PreTables.PropertyData, "dataNvarchar"))
                Rename.Column("dataNtext").OnTable(PreTables.PropertyData).To("varcharValue");
            if (ColumnExists(PreTables.PropertyData, "dataDecimal"))
                Rename.Column("dataDecimal").OnTable(PreTables.PropertyData).To("decimalValue");
            if (ColumnExists(PreTables.PropertyData, "dataInt"))
                Rename.Column("dataInt").OnTable(PreTables.PropertyData).To("intValue");
            if (ColumnExists(PreTables.PropertyData, "dataDate"))
                Rename.Column("dataDate").OnTable(PreTables.PropertyData).To("dateValue");

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
