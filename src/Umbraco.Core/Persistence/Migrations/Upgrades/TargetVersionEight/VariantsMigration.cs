using System;
using System.Linq;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
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
            MigrateContent();
            MigrateContentVersion();
            MigrateDocumentVersion();
            MigrateDocument();

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
            // add column propertyData.segment
            // add column propertyData.published
            if (!ColumnExists(PreTables.PropertyData, "languageId"))
                AddColumn<PropertyDataDto>(PreTables.PropertyData, "languageId");
            if (!ColumnExists(PreTables.PropertyData, "segment"))
                AddColumn<PropertyDataDto>(PreTables.PropertyData, "segment");
            if (!ColumnExists(PreTables.PropertyData, "published"))
                AddColumn<PropertyDataDto>(PreTables.PropertyData, "published");

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

        private void MigrateContent()
        {
            // if the table has already been renamed, we're done
            if (TableExists(Constants.DatabaseSchema.Tables.Content))
                return;

            // rename columns
            if (ColumnExists(PreTables.Content, "contentType"))
                ReplaceColumn<PropertyDataDto>(PreTables.Content, "contentType", "contentTypeId");

            // add columns
            if (!ColumnExists(PreTables.Content, "writerUserId"))
                AddColumn<ContentDto>(PreTables.Content, "writerUserId");
            if (!ColumnExists(PreTables.Content, "updateDate"))
                AddColumn<ContentDto>(PreTables.Content, "updateDate");

            // copy data for added columns
            Execute.Sql($@"UPDATE {SqlSyntax.GetQuotedTableName(PreTables.Content)}
SET writerUserId=doc.documentUser, updateDate=doc.updateDate,
FROM {SqlSyntax.GetQuotedTableName(PreTables.Content)} con
JOIN {SqlSyntax.GetQuotedTableName(PreTables.Document)} doc ON con.nodeId=doc.nodeId AND doc.newest=1");

            // drop columns
            if (ColumnExists(PreTables.Content, "pk"))
                Delete.Column("pk").FromTable(PreTables.Content);

            // rename table
            Rename.Table(PreTables.Content).To(Constants.DatabaseSchema.Tables.Content);
        }

        private void MigrateContentVersion()
        {
            // if the table has already been renamed, we're done
            if (TableExists(Constants.DatabaseSchema.Tables.ContentVersion))
                return;

            // add text column
            if (!ColumnExists(PreTables.ContentVersion, "text"))
                AddColumn<ContentVersionDto>(PreTables.ContentVersion, "text");

            // populate text column
            Execute.Sql($@"UPDATE {SqlSyntax.GetQuotedTableName(PreTables.ContentVersion)} cver
SET cver.text=doc.text
FROM {SqlSyntax.GetQuotedTableName(PreTables.Document)} doc WHERE cver.versionId=doc.versionId");

            // add current column
            if (!ColumnExists(PreTables.ContentVersion, "current"))
                AddColumn<ContentVersionDto>(PreTables.ContentVersion, "current");

            // populate current column => done during MigrateDocument

            // rename contentId column
            if (ColumnExists(PreTables.ContentVersion, "ContentId"))
                ReplaceColumn<ContentVersionDto>(PreTables.Content, "ContentId", "nodeId");

            // rename table
            Rename.Table(PreTables.ContentVersion).To(Constants.DatabaseSchema.Tables.ContentVersion);
        }

        private void MigrateDocumentVersion()
        {
            // if the table already exists, we're done
            if (TableExists(Constants.DatabaseSchema.Tables.DocumentVersion))
                return;

            // create table
            Create.Table<DocumentVersionDto>(withoutKeysAndIndexes: true);

            Execute.Sql($@"INSERT INTO {SqlSyntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.DocumentVersion)} (id, contentVersionId, templateId)
SELECT cver.Id, cver.versionId, doc.templateId
FROM {SqlSyntax.GetQuotedTableName(PreTables.ContentVersion)} cver
JOIN {SqlSyntax.GetQuotedTableName(PreTables.Document)} doc ON doc.versionId=cver.versionId");
        }

        private void MigrateDocument()
        {
            // if the table has already been renamed, we're done
            if (TableExists(Constants.DatabaseSchema.Tables.Document))
                return;

            // drop some columns
            Delete.Column("text").FromTable(PreTables.Document); // fixme usage
            Delete.Column("templateId").FromTable(PreTables.Document); // fixme usage
            Delete.Column("documentUser").FromTable(PreTables.Document); // fixme usage
            Delete.Column("updateDate").FromTable(PreTables.Document); // fixme usage

            // update PropertyData.Published for published versions
            if (Context.SqlContext.DatabaseType.IsMySql())
            {
                // FIXME does MySql support such update syntax?
                throw new NotSupportedException();
            }
            else
            {
                Execute.Sql($@"UPDATE pdata
SET pdata.published=1
FROM {SqlSyntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.PropertyData)} pdata
JOIN {SqlSyntax.GetQuotedTableName(PreTables.Document)} doc ON doc.versionId=pdata.versionId AND doc.published=1");
            }

            // collapse draft version into published version (if any)
            // ie we keep the published version and remove the draft one
            Execute.Code(context =>
            {
                var versions = context.Database.Fetch<dynamic>(@"SELECT
	doc1.versionId versionId1, doc1.newest newest1, doc1.published published1,
	doc2.versionId versionId2, doc2.newest newest2, doc2.published published2
FROM {SqlSyntax.GetQuotedTableName(PreTables.Document)} doc1
JOIN {SqlSyntax.GetQuotedTableName(PreTables.Document)} doc2
    ON doc1.nodeId=doc2.nodeId AND doc1.versionId<>doc2.versionId AND doc1.updateDate<doc2.updateDate
WHERE doc1.newest=0 AND doc1.published=1 AND doc2.newest=1 AND doc2.published=0
");
                foreach (var version in versions)
                {
                    context.Database.Execute($@"UPDATE {SqlSyntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.PropertyData)}
SET versionId='{version.versionId1}', published=1
WHERE versionId='{version.versionId2}'");
                    context.Database.Execute($@"DELETE FROM {SqlSyntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.PropertyData)}
WHERE versionId='{version.versionId2}'");
                    context.Database.Execute($@"DELETE FROM {SqlSyntax.GetQuotedTableName(PreTables.Document)}
WHERE versionId='{version.versionId2}'");
                    context.Database.Execute($@"UPDATE {SqlSyntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.Document)}
SET newest=1 WHERE versionId='{version.versionId1}'");
                }
                return string.Empty;
            });

            // update content version
            Execute.Sql($@"UPDATE {SqlSyntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.ContentVersion)}
SET current=1
FROM {SqlSyntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.ContentVersion)} ver
JOIN {SqlSyntax.GetQuotedTableName(PreTables.Document)} doc ON ver.versionId=doc.versionId
WHERE doc.newest=1");

            // keep only one row per document
            Execute.Code(context =>
            {
                var versions = context.Database.Fetch<dynamic>(@"SELECT
	doc.nodeId, doc.versionId versionId1
FROM {SqlSyntax.GetQuotedTableName(PreTables.Document)} doc
WHERE doc.newest=1
");
                foreach (var version in versions)
                {
                    context.Database.Execute($@"DELETE FROM {SqlSyntax.GetQuotedTableName(PreTables.Document)}
WHERE nodeId={version.nodeId} AND versionId<>{version.versionId}");
                }
                return string.Empty;
            });

            // drop some columns
            Delete.Column("versionId").FromTable(PreTables.Document); // fixme usage
            Delete.Column("newest").FromTable(PreTables.Document); // fixme usage

            // rename table
            Rename.Table(PreTables.Document).To(Constants.DatabaseSchema.Tables.Document);
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
