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
                // xxx1 is published, non-newest
                // xxx2 is newest, non-published
                // we want to move data from 2 to 1
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
                    // move property data from 2 to 1, with published=0
                    context.Database.Execute($@"UPDATE {SqlSyntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.PropertyData)}
SET versionId='{version.versionId1}', published=0
WHERE versionId='{version.versionId2}'");
                    // and then there is no property data anymore for 2
                    // so we can delete the corresp. document table row
                    context.Database.Execute($@"DELETE FROM {SqlSyntax.GetQuotedTableName(PreTables.Document)}
WHERE versionId='{version.versionId2}'");
                    // and mark 1 as newest
                    context.Database.Execute($@"UPDATE {SqlSyntax.GetQuotedTableName(PreTables.Document)}
SET newest=1 WHERE versionId='{version.versionId1}'");
                }
                return string.Empty;
            });

            // update content version
            Execute.Sql($@"UPDATE {SqlSyntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.ContentVersion)}
SET current=doc.newest
FROM {SqlSyntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.ContentVersion)} ver
JOIN {SqlSyntax.GetQuotedTableName(PreTables.Document)} doc ON ver.versionId=doc.versionId");

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

            // ensure that every 'published' property data has a corresponding 'non-published' one
            // but only for the current version
            Execute.Sql($@"INSERT INTO {Constants.DatabaseSchema.Tables.PropertyData} (nodeId, versionId, propertyTypeId, languageId, segment, published, intValue, decimalValue, dateValue, varcharValue, textValue)
SELECT p1.nodeId, p1.versionId, p1.propertyTypeId, p1.languageId, p1.segment, 0, p1.intValue, p1.decimalValue, p1.dateValue, p1.varcharValue, p1.textValue
FROM {Constants.DatabaseSchema.Tables.PropertyData} p1
JOIN {Constants.DatabaseSchema.Tables.ContentVersion} ON p1.versionId=cver.versionId AND cver.current=1
WHERE NOT EXIST (
    SELECT p2.id
    FROM {Constants.DatabaseSchema.Tables.PropertyData} p2
    WHERE
        p1.nodeId=p2.nodeId AND p1.versionId=p2.versionId
        AND p1.propertyTypeId=p2.propertyTypeId
        AND p1.lang=p2.lang AND p1.segment=p2.segment
        AND p2.published=0
)");

            // create some columns
            if (!ColumnExists(PreTables.Document, "edits"))
            {
                AddColumn<DocumentDto>(PreTables.Document, "edits", out var notNull);
                Execute.Sql($"UPDATE {SqlSyntax.GetQuotedTableName(PreTables.Document)} SET edits=0");
                Execute.Sql(notNull);

                // set 'edits' to true whenever a 'non-published' property data is != a published one
                Execute.Sql($@"UPDATE {SqlSyntax.GetQuotedTableName(PreTables.Document)} SET edits=0");

                Execute.Sql($@"UPDATE {SqlSyntax.GetQuotedTableName(PreTables.Document)} SET edits=1 WHERE nodeId IN (
SELECT p1.nodeId
FROM {Constants.DatabaseSchema.Tables.PropertyData} p1
JOIN {Constants.DatabaseSchema.Tables.ContentVersion} ON p1.versionId=cver.versionId AND cver.current=1
JOIN {Constants.DatabaseSchema.Tables.PropertyData} p2
ON p1.nodeId=p2.nodeId AND p1.versionId=p2.versionId AND AND p1.propertyTypeId=p2.propertyTypeId AND p1.lang=p2.lang AND p1.segment=p2.segment AND p2.published=0
    AND (p1.intValue<>p2.intValue OR p1.decimalValue<>p2.decimalValue OR p1.dateValue<>p2.dateValue OR p1.varcharValue<>p2.varcharValue OR p1.textValue<>p2.textValue)
WHERE p1.published=1)");
            }

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
