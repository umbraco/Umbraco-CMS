using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Models;

namespace Umbraco.Cms.Persistence.EFCore.DbContexts
{
    public abstract class UmbracoDbContextBase : DbContext, IUmbracoDatabaseContract
    {
        protected UmbracoDbContextBase(DbContextOptions<UmbracoDbContext> options) : base(options)
        {
        }

        /// <inheritdoc/>
        public abstract IQueryable<CmsContentNu> CmsContentNus { get; }

        /// <inheritdoc/>
        public abstract IQueryable<CmsContentTypeAllowedContentType> CmsContentTypeAllowedContentTypes { get; }

        /// <inheritdoc/>
        public abstract IQueryable<CmsContentType> CmsContentTypes { get; }

        /// <inheritdoc/>
        public abstract IQueryable<CmsDictionary> CmsDictionaries { get; }

        /// <inheritdoc/>
        public abstract IQueryable<CmsDocumentType> CmsDocumentTypes { get; }

        /// <inheritdoc/>
        public abstract IQueryable<CmsLanguageText> CmsLanguageTexts { get; }

        /// <inheritdoc/>
        public abstract IQueryable<CmsMacroProperty> CmsMacroProperties { get; }

        /// <inheritdoc/>
        public abstract IQueryable<CmsMacro> CmsMacros { get; }

        /// <inheritdoc/>
        public abstract IQueryable<CmsMember> CmsMembers { get; }

        /// <inheritdoc/>
        public abstract IQueryable<CmsMemberType> CmsMemberTypes { get; }

        /// <inheritdoc/>
        public abstract IQueryable<CmsPropertyTypeGroup> CmsPropertyTypeGroups { get; }

        /// <inheritdoc/>
        public abstract IQueryable<CmsPropertyType> CmsPropertyTypes { get; }

        /// <inheritdoc/>
        public abstract IQueryable<CmsTagRelationship> CmsTagRelationships { get; }

        /// <inheritdoc/>
        public abstract IQueryable<CmsTag> CmsTags { get; }

        /// <inheritdoc/>
        public abstract IQueryable<CmsTemplate> CmsTemplates { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoAccess> UmbracoAccesses { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoAccessRule> UmbracoAccessRules { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoAudit> UmbracoAudits { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoCacheInstruction> UmbracoCacheInstructions { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoConsent> UmbracoConsents { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoContent> UmbracoContents { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoContentSchedule> UmbracoContentSchedules { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoContentVersionCleanupPolicy> UmbracoContentVersionCleanupPolicies { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoContentVersionCultureVariation> UmbracoContentVersionCultureVariations { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoContentVersion> UmbracoContentVersions { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoCreatedPackageSchema> UmbracoCreatedPackageSchemas { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoDataType> UmbracoDataTypes { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoDocumentCultureVariation> UmbracoDocumentCultureVariations { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoDocument> UmbracoDocuments { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoDocumentVersion> UmbracoDocumentVersions { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoDomain> UmbracoDomains { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoExternalLogin> UmbracoExternalLogins { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoExternalLoginToken> UmbracoExternalLoginTokens { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoKeyValue> UmbracoKeyValues { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoLanguage> UmbracoLanguages { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoLock> UmbracoLocks { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoLog> UmbracoLogs { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoLogViewerQuery> UmbracoLogViewerQueries { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoMediaVersion> UmbracoMediaVersions { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoNode> UmbracoNodes { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoPropertyDatum> UmbracoPropertyData { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoRedirectUrl> UmbracoRedirectUrls { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoRelation> UmbracoRelations { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoRelationType> UmbracoRelationTypes { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoServer> UmbracoServers { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoTwoFactorLogin> UmbracoTwoFactorLogins { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoUser2NodeNotify> UmbracoUser2NodeNotifies { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoUserGroup2App> UmbracoUserGroup2Apps { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoUserGroup2NodePermission> UmbracoUserGroup2NodePermissions { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoUserGroup> UmbracoUserGroups { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoUserLogin> UmbracoUserLogins { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoUser> UmbracoUsers { get; }

        /// <inheritdoc/>
        public abstract IQueryable<UmbracoUserStartNode> UmbracoUserStartNodes { get; }
    }
}
