using System;
using System.Diagnostics;
using System.Linq;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class VariantsMigration : MigrationBase
    {
        public VariantsMigration(IMigrationContext context)
            : base(context)
        { }

        // notes
        // do NOT use Rename.Column as it's borked on SQLCE - use ReplaceColumn instead

        public override void Migrate()
        {
            MigratePropertyData();
            MigrateContentAndPropertyTypes();
            MigrateContent();
            MigrateVersions();

            if (Database.Fetch<dynamic>($@"SELECT {Constants.DatabaseSchema.Tables.ContentVersion}.nodeId, COUNT({Constants.DatabaseSchema.Tables.ContentVersion}.id)
FROM {Constants.DatabaseSchema.Tables.ContentVersion}
JOIN {Constants.DatabaseSchema.Tables.DocumentVersion} ON {Constants.DatabaseSchema.Tables.ContentVersion}.id={Constants.DatabaseSchema.Tables.DocumentVersion}.id
WHERE {Constants.DatabaseSchema.Tables.DocumentVersion}.published=1
GROUP BY {Constants.DatabaseSchema.Tables.ContentVersion}.nodeId
HAVING COUNT({Constants.DatabaseSchema.Tables.ContentVersion}.id) > 1").Any())
            {
                Debugger.Break();
                throw new Exception("Migration failed: duplicate 'published' document versions.");
            }

            if (Database.Fetch<dynamic>($@"SELECT v1.nodeId, v1.id, COUNT(v2.id)
FROM {Constants.DatabaseSchema.Tables.ContentVersion} v1
LEFT JOIN {Constants.DatabaseSchema.Tables.ContentVersion} v2 ON v1.nodeId=v2.nodeId AND v2.[current]=1
GROUP BY v1.nodeId, v1.id
HAVING COUNT(v2.id) <> 1").Any())
            {
                Debugger.Break();
                throw new Exception("Migration failed: missing or duplicate 'current' content versions.");
            }
        }

        private void MigratePropertyData()
        {
            // if the table has already been renamed, we're done
            if (TableExists(Constants.DatabaseSchema.Tables.PropertyData))
                return;

            // add columns
            if (!ColumnExists(PreTables.PropertyData, "languageId"))
                AddColumn<PropertyDataDto>(PreTables.PropertyData, "languageId");
            if (!ColumnExists(PreTables.PropertyData, "segment"))
                AddColumn<PropertyDataDto>(PreTables.PropertyData, "segment");

            // rename columns
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

            // transform column versionId from guid to integer (contentVersion.id)
            if (ColumnType(PreTables.PropertyData, "versionId") == "uniqueidentifier")
            {
                Alter.Table(PreTables.PropertyData).AddColumn("versionId2").AsInt32().Nullable().Do();

                // SQLCE does not support UPDATE...FROM
                var temp = Database.Fetch<dynamic>($"SELECT id, versionId FROM {PreTables.ContentVersion}");
                foreach (var t in temp)
                    Database.Execute($"UPDATE {PreTables.PropertyData} SET versionId2=@v2 WHERE versionId=@v1", new { v1 = t.versionId, v2 = t.id });
                Delete.Column("versionId").FromTable(PreTables.PropertyData).Do();
                ReplaceColumn<PropertyDataDto>(PreTables.PropertyData, "versionId2", "versionId");
            }

            // drop column
            if (ColumnExists(PreTables.PropertyData, "contentNodeId"))
                Delete.Column("contentNodeId").FromTable(PreTables.PropertyData).Do();

            // rename table
            Rename.Table(PreTables.PropertyData).To(Constants.DatabaseSchema.Tables.PropertyData).Do();
        }

        private void MigrateContentAndPropertyTypes()
        {
            if (!ColumnExists(PreTables.ContentType, "variations"))
                AddColumn<ContentTypeDto>(PreTables.ContentType, "variations");
            if (!ColumnExists(PreTables.PropertyType, "variations"))
                AddColumn<PropertyTypeDto>(PreTables.PropertyType, "variations");
        }

        private void MigrateContent()
        {
            // if the table has already been renamed, we're done
            if (TableExists(Constants.DatabaseSchema.Tables.Content))
                return;

            // rename columns
            if (ColumnExists(PreTables.Content, "contentType"))
                ReplaceColumn<ContentDto>(PreTables.Content, "contentType", "contentTypeId");

            // drop columns
            if (ColumnExists(PreTables.Content, "pk"))
                Delete.Column("pk").FromTable(PreTables.Content).Do();

            // rename table
            Rename.Table(PreTables.Content).To(Constants.DatabaseSchema.Tables.Content).Do();
        }

        private void MigrateVersions()
        {
            // if the table has already been renamed, we're done
            if (TableExists(Constants.DatabaseSchema.Tables.ContentVersion))
                return;

            // if the table already exists, we're done
            if (TableExists(Constants.DatabaseSchema.Tables.DocumentVersion))
                return;

            // if the table has already been renamed, we're done
            if (TableExists(Constants.DatabaseSchema.Tables.Document))
                return;

            // do it all at once

            // add contentVersion columns
            if (!ColumnExists(PreTables.ContentVersion, "text"))
                AddColumn<ContentVersionDto>(PreTables.ContentVersion, "text");
            if (!ColumnExists(PreTables.ContentVersion, "current"))
            {
                AddColumn<ContentVersionDto>(PreTables.ContentVersion, "current", out var sqls);
                Database.Execute($@"UPDATE {SqlSyntax.GetQuotedTableName(PreTables.ContentVersion)} SET {SqlSyntax.GetQuotedColumnName("current")}=0");
                foreach (var sql in sqls) Database.Execute(sql);
            }
            if (!ColumnExists(PreTables.ContentVersion, "userId"))
            {
                AddColumn<ContentVersionDto>(PreTables.ContentVersion, "userId", out var sqls);
                Database.Execute($@"UPDATE {SqlSyntax.GetQuotedTableName(PreTables.ContentVersion)} SET userId=0");
                foreach (var sql in sqls) Database.Execute(sql);
            }

            // rename contentVersion contentId column
            if (ColumnExists(PreTables.ContentVersion, "ContentId"))
                ReplaceColumn<ContentVersionDto>(PreTables.ContentVersion, "ContentId", "nodeId");

            // populate contentVersion text, current and userId columns for documents
            // SQLCE does not support UPDATE...FROM
            var temp1 = Database.Fetch<dynamic>($"SELECT versionId, text, newest, documentUser FROM {PreTables.Document}");
            foreach (var t in temp1)
                Database.Execute($@"UPDATE {PreTables.ContentVersion} SET text=@text, {SqlSyntax.GetQuotedColumnName("current")}=@current, userId=@userId WHERE versionId=@versionId",
                    new { text = t.text, current = t.newest, userId=t.documentUser, versionId=t.versionId });

            // populate contentVersion text and current columns for non-documents, userId is default
            // SQLCE does not support UPDATE...FROM
            var temp2 = Database.Fetch<dynamic>($@"SELECT cver.versionId, n.text
FROM {PreTables.ContentVersion} cver
JOIN {SqlSyntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.Node)} n ON cver.nodeId=n.id
WHERE cver.versionId NOT IN (SELECT versionId FROM {SqlSyntax.GetQuotedTableName(PreTables.Document)})");

            foreach (var t in temp2)
                Database.Execute($@"UPDATE {PreTables.ContentVersion} SET text=@text, {SqlSyntax.GetQuotedColumnName("current")}=1, userId=0 WHERE versionId=@versionId",
                    new { text = t.text, versionId=t.versionId });

            // create table
            Create.Table<DocumentVersionDto>(withoutKeysAndIndexes: true).Do();

            // every document row becomes a document version
            Database.Execute($@"INSERT INTO {SqlSyntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.DocumentVersion)} (id, templateId, published)
SELECT cver.id, doc.templateId, doc.published
FROM {SqlSyntax.GetQuotedTableName(PreTables.ContentVersion)} cver
JOIN {SqlSyntax.GetQuotedTableName(PreTables.Document)} doc ON doc.nodeId=cver.nodeId AND doc.versionId=cver.versionId");

            // need to add extra rows for where published=newest
            // 'cos INSERT above has inserted the 'published' document version
            // and v8 always has a 'edited' document version too
            var temp3 = Database.Fetch<dynamic>($@"SELECT doc.nodeId, doc.updateDate, doc.documentUser, doc.text, doc.templateId, cver.id versionId
FROM {SqlSyntax.GetQuotedTableName(PreTables.Document)} doc
JOIN {SqlSyntax.GetQuotedTableName(PreTables.ContentVersion)} cver ON doc.nodeId=cver.nodeId AND doc.versionId=cver.versionId
WHERE doc.newest=1 AND doc.published=1");
            var getIdentity = "@@@@IDENTITY";
            foreach (var t in temp3)
            {
                Database.Execute($@"INSERT INTO {SqlSyntax.GetQuotedTableName(PreTables.ContentVersion)} (nodeId, versionId, versionDate, userId, {SqlSyntax.GetQuotedColumnName("current")}, text)
VALUES (@nodeId, @versionId, @versionDate, @userId, 1, @text)", new { nodeId=t.nodeId, versionId=Guid.NewGuid(), versionDate=t.updateDate, userId=t.documentUser, text=t.text });
                var id = Database.ExecuteScalar<int>("SELECT " + getIdentity);
                Database.Execute($"UPDATE {SqlSyntax.GetQuotedTableName(PreTables.ContentVersion)} SET {SqlSyntax.GetQuotedColumnName("current")}=0 WHERE nodeId=@0 AND id<>@1", (int) t.nodeId, id);
                Database.Execute($@"INSERT INTO {SqlSyntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.DocumentVersion)} (id, templateId, published)
VALUES (@id, @templateId, 0)", new { id=id, templateId=t.templateId });

                var versionId = (int) t.versionId;
                var pdatas = Database.Fetch<PropertyDataDto>(Sql().Select<PropertyDataDto>().From<PropertyDataDto>().Where<PropertyDataDto>(x => x.VersionId == versionId));
                foreach (var pdata in pdatas)
                {
                    pdata.VersionId = id;
                    Database.Insert(pdata);
                }
            }

            // reduce document to 1 row per content
            Database.Execute($@"DELETE FROM {PreTables.Document}
WHERE versionId NOT IN (SELECT (versionId) FROM {PreTables.ContentVersion} WHERE {SqlSyntax.GetQuotedColumnName("current")} = 1) AND (published<>1 OR newest<>1)");

            // drop some document columns
            Delete.Column("text").FromTable(PreTables.Document).Do();
            Delete.Column("templateId").FromTable(PreTables.Document).Do();
            Delete.Column("documentUser").FromTable(PreTables.Document).Do();
            Delete.DefaultConstraint().OnTable(PreTables.Document).OnColumn("updateDate").Do();
            Delete.Column("updateDate").FromTable(PreTables.Document).Do();
            Delete.Column("versionId").FromTable(PreTables.Document).Do();
            Delete.DefaultConstraint().OnTable(PreTables.Document).OnColumn("newest").Do();
            Delete.Column("newest").FromTable(PreTables.Document).Do();

            // add and populate edited column
            if (!ColumnExists(PreTables.Document, "edited"))
            {
                AddColumn<DocumentDto>(PreTables.Document, "edited", out var sqls);
                Database.Execute($"UPDATE {SqlSyntax.GetQuotedTableName(PreTables.Document)} SET edited=0");
                foreach (var sql in sqls) Database.Execute(sql);
            }

            // set 'edited' to true whenever a 'non-published' property data is != a published one
            // cannot compare NTEXT values in TSQL
            // cannot cast NTEXT to NVARCHAR(MAX) in SQLCE
            // ... bah ...
            var temp = Database.Fetch<dynamic>($@"SELECT n.id,
v1.intValue intValue1, v1.decimalValue decimalValue1, v1.dateValue dateValue1, v1.varcharValue varcharValue1, v1.textValue textValue1,
v2.intValue intValue2, v2.decimalValue decimalValue2, v2.dateValue dateValue2, v2.varcharValue varcharValue2, v2.textValue textValue2
FROM {Constants.DatabaseSchema.Tables.Node} n
JOIN {PreTables.ContentVersion} cv1 ON n.id=cv1.nodeId AND cv1.{SqlSyntax.GetQuotedColumnName("current")}=1
JOIN {Constants.DatabaseSchema.Tables.PropertyData} v1 ON cv1.id=v1.versionId
JOIN {PreTables.ContentVersion} cv2 ON n.id=cv2.nodeId
JOIN {Constants.DatabaseSchema.Tables.DocumentVersion} dv ON cv2.id=dv.id AND dv.published=1
JOIN {Constants.DatabaseSchema.Tables.PropertyData} v2 ON cv2.id=v2.versionId
WHERE v1.propertyTypeId=v2.propertyTypeId AND v1.languageId=v2.languageId AND v1.segment=v2.segment");

            foreach (var t in temp)
                if (t.intValue1 != t.intValue2 || t.decimalValue1 != t.decimalValue2 || t.dateValue1 != t.dateValue2 || t.varcharValue1 != t.varcharValue2 || t.textValue1 != t.textValue2)
                    Database.Execute("UPDATE {SqlSyntax.GetQuotedTableName(PreTables.Document)} SET edited=1 WHERE nodeId=@nodeIdd", new { t.id });

            // drop more columns
            Delete.Column("versionId").FromTable(PreTables.ContentVersion).Do();

            // rename tables
            Rename.Table(PreTables.ContentVersion).To(Constants.DatabaseSchema.Tables.ContentVersion).Do();
            Rename.Table(PreTables.Document).To(Constants.DatabaseSchema.Tables.Document).Do();
        }

        private static class PreTables
        {
            // ReSharper disable UnusedMember.Local
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

            // ReSharper restore UnusedMember.Local
        }
    }
}
