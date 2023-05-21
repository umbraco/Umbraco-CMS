namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.Common;

public class DeleteKeysAndIndexes : MigrationBase
{
    public DeleteKeysAndIndexes(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        // all v7.14 tables
        var tables = new[]
        {
            "cmsContent",
            "cmsContentType",
            "cmsContentType2ContentType",
            "cmsContentTypeAllowedContentType",
            "cmsContentVersion",
            "cmsContentXml",
            "cmsDataType",
            "cmsDataTypePreValues",
            "cmsDictionary",
            "cmsDocument",
            "cmsDocumentType",
            "cmsLanguageText",
            "cmsMacro",
            "cmsMacroProperty",
            "cmsMedia",
            "cmsMember",
            "cmsMember2MemberGroup",
            "cmsMemberType",
            "cmsPreviewXml",
            "cmsPropertyData",
            "cmsPropertyType",
            "cmsPropertyTypeGroup",
            "cmsTagRelationship",
            "cmsTags",
            "cmsTask",
            "cmsTaskType",
            "cmsTemplate",
            "umbracoAccess",
            "umbracoAccessRule",
            "umbracoAudit",
            "umbracoCacheInstruction",
            "umbracoConsent",
            "umbracoDomains",
            "umbracoExternalLogin",
            "umbracoLanguage",
            "umbracoLock",
            "umbracoLog",
            "umbracoMigration",
            "umbracoNode",
            "umbracoRedirectUrl",
            "umbracoRelation",
            "umbracoRelationType",
            "umbracoServer",
            "umbracoUser",
            "umbracoUser2NodeNotify",
            "umbracoUser2UserGroup",
            "umbracoUserGroup",
            "umbracoUserGroup2App",
            "umbracoUserGroup2NodePermission",
            "umbracoUserLogin",
            "umbracoUserStartNode",
        };

        // delete *all* keys and indexes - because of FKs
        // on known v7 tables only
        foreach (var table in tables)
        {
            Delete.KeysAndIndexes(table, false).Do();
        }

        foreach (var table in tables)
        {
            Delete.KeysAndIndexes(table, true, false).Do();
        }
    }
}
