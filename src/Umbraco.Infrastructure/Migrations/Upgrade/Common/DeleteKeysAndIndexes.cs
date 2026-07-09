namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.Common;

/// <summary>
/// Represents a migration step that removes specified database keys and indexes during an upgrade process.
/// </summary>
public class DeleteKeysAndIndexes : MigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="T:Umbraco.Cms.Infrastructure.Migrations.Upgrade.Common.DeleteKeysAndIndexes"/> class.
    /// </summary>
    /// <param name="context">The <see cref="T:Umbraco.Cms.Infrastructure.Migrations.IMigrationContext"/> to use for the migration.</param>
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
