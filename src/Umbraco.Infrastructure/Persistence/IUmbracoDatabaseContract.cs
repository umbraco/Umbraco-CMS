using Umbraco.Cms.Infrastructure.Persistence.Models;

namespace Umbraco.Cms.Infrastructure.Persistence
{
    public interface IUmbracoDatabaseContract
    {
        IQueryable<CmsContentNu> CmsContentNus { get; }
        IQueryable<CmsContentTypeAllowedContentType> CmsContentTypeAllowedContentTypes { get; }
        IQueryable<CmsContentType> CmsContentTypes { get; }
        IQueryable<CmsDictionary> CmsDictionaries { get; }
        IQueryable<CmsDocumentType> CmsDocumentTypes { get; }
        IQueryable<CmsLanguageText> CmsLanguageTexts { get; }
        IQueryable<CmsMacroProperty> CmsMacroProperties { get; }
        IQueryable<CmsMacro> CmsMacros { get; }
        IQueryable<CmsMember> CmsMembers { get; }
        IQueryable<CmsMemberType> CmsMemberTypes { get; }
        IQueryable<CmsPropertyTypeGroup> CmsPropertyTypeGroups { get; }
        IQueryable<CmsPropertyType> CmsPropertyTypes { get; }
        IQueryable<CmsTagRelationship> CmsTagRelationships { get; }
        IQueryable<CmsTag> CmsTags { get; }
        IQueryable<CmsTemplate> CmsTemplates { get; }
        IQueryable<UmbracoAccess> UmbracoAccesses { get; }
        IQueryable<UmbracoAccessRule> UmbracoAccessRules { get; }
        IQueryable<UmbracoAudit> UmbracoAudits { get; }
        IQueryable<UmbracoCacheInstruction> UmbracoCacheInstructions { get; }
        IQueryable<UmbracoConsent> UmbracoConsents { get; }
        IQueryable<UmbracoContent> UmbracoContents { get; }
        IQueryable<UmbracoContentSchedule> UmbracoContentSchedules { get; }
        IQueryable<UmbracoContentVersionCleanupPolicy> UmbracoContentVersionCleanupPolicies { get; }
        IQueryable<UmbracoContentVersionCultureVariation> UmbracoContentVersionCultureVariations { get; }
        IQueryable<UmbracoContentVersion> UmbracoContentVersions { get; }
        IQueryable<UmbracoCreatedPackageSchema> UmbracoCreatedPackageSchemas { get; }
        IQueryable<UmbracoDataType> UmbracoDataTypes { get; }
        IQueryable<UmbracoDocumentCultureVariation> UmbracoDocumentCultureVariations { get; }
        IQueryable<UmbracoDocument> UmbracoDocuments { get; }
        IQueryable<UmbracoDocumentVersion> UmbracoDocumentVersions { get; }
        IQueryable<UmbracoDomain> UmbracoDomains { get; }
        IQueryable<UmbracoExternalLogin> UmbracoExternalLogins { get; }
        IQueryable<UmbracoExternalLoginToken> UmbracoExternalLoginTokens { get; }
        IQueryable<UmbracoKeyValue> UmbracoKeyValues { get; }
        IQueryable<UmbracoLanguage> UmbracoLanguages { get; }
        IQueryable<UmbracoLock> UmbracoLocks { get; }
        IQueryable<UmbracoLog> UmbracoLogs { get; }
        IQueryable<UmbracoLogViewerQuery> UmbracoLogViewerQueries { get; }
        IQueryable<UmbracoMediaVersion> UmbracoMediaVersions { get; }
        IQueryable<UmbracoNode> UmbracoNodes { get; }
        IQueryable<UmbracoPropertyDatum> UmbracoPropertyData { get; }
        IQueryable<UmbracoRedirectUrl> UmbracoRedirectUrls { get; }
        IQueryable<UmbracoRelation> UmbracoRelations { get; }
        IQueryable<UmbracoRelationType> UmbracoRelationTypes { get; }
        IQueryable<UmbracoServer> UmbracoServers { get; }
        IQueryable<UmbracoTwoFactorLogin> UmbracoTwoFactorLogins { get; }
        IQueryable<UmbracoUser2NodeNotify> UmbracoUser2NodeNotifies { get; }
        IQueryable<UmbracoUserGroup2App> UmbracoUserGroup2Apps { get; }
        IQueryable<UmbracoUserGroup2NodePermission> UmbracoUserGroup2NodePermissions { get; }
        IQueryable<UmbracoUserGroup> UmbracoUserGroups { get; }
        IQueryable<UmbracoUserLogin> UmbracoUserLogins { get; }
        IQueryable<UmbracoUser> UmbracoUsers { get; }
        IQueryable<UmbracoUserStartNode> UmbracoUserStartNodes { get; }
    }
}