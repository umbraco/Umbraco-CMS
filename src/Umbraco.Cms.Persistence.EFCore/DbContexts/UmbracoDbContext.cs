using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Models;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.DbContexts;

/// <remarks>
/// To autogenerate migrations use the following commands
/// and insure the 'src/Umbraco.Web.UI/appsettings.json' have a connection string set with the right provider.
///
/// Create a migration for each provider
/// <code>dotnet ef migrations add %Name% -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.SqlServer -c _umbracoDbContext  -- --provider SqlServer</code>
///
/// <code>dotnet ef migrations add %Name% -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.Sqlite -c _umbracoDbContext  -- --provider Sqlite</code>
///
/// Remove the last migration for each provider
/// <code>dotnet ef migrations remove -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.SqlServer -c _umbracoDbContext -- --provider SqlServer</code>
///
/// <code>dotnet ef migrations remove -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.Sqlite -c _umbracoDbContext -- --provider Sqlite</code>
///
/// To find documentation about this way of working with the context see
/// https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/providers?tabs=dotnet-core-cli#using-one-context-type
/// </remarks>
public class UmbracoDbContext : UmbracoDbContextBase
{
    public UmbracoDbContext(DbContextOptions<UmbracoDbContext> options) : base(options)
    {
    }

    /// <inheritdoc/>
    public override DbSet<CmsContentNu> CmsContentNus => Set<CmsContentNu>();

    /// <inheritdoc/>
    public override DbSet<CmsContentType> CmsContentTypes => Set<CmsContentType>();

    /// <inheritdoc/>
    public override DbSet<CmsContentTypeAllowedContentType> CmsContentTypeAllowedContentTypes => Set<CmsContentTypeAllowedContentType>();

    /// <inheritdoc/>
    public override DbSet<CmsDictionary> CmsDictionaries => Set<CmsDictionary>();

    /// <inheritdoc/>
    public override DbSet<CmsDocumentType> CmsDocumentTypes => Set<CmsDocumentType>();

    /// <inheritdoc/>
    public override DbSet<CmsLanguageText> CmsLanguageTexts => Set<CmsLanguageText>();

    /// <inheritdoc/>
    public override DbSet<CmsMacro> CmsMacros => Set<CmsMacro>();

    /// <inheritdoc/>
    public override DbSet<CmsMacroProperty> CmsMacroProperties => Set<CmsMacroProperty>();

    /// <inheritdoc/>
    public override DbSet<CmsMember> CmsMembers => Set<CmsMember>();

    /// <inheritdoc/>
    public override DbSet<CmsMemberType> CmsMemberTypes => Set<CmsMemberType>();

    /// <inheritdoc/>
    public override DbSet<CmsPropertyType> CmsPropertyTypes => Set<CmsPropertyType>();

    /// <inheritdoc/>
    public override DbSet<CmsPropertyTypeGroup> CmsPropertyTypeGroups => Set<CmsPropertyTypeGroup>();

    /// <inheritdoc/>
    public override DbSet<CmsTag> CmsTags => Set<CmsTag>();

    /// <inheritdoc/>
    public override DbSet<CmsTagRelationship> CmsTagRelationships => Set<CmsTagRelationship>();

    /// <inheritdoc/>
    public override DbSet<CmsTemplate> CmsTemplates => Set<CmsTemplate>();

    /// <inheritdoc/>
    public override DbSet<UmbracoAccess> UmbracoAccesses => Set<UmbracoAccess>();

    /// <inheritdoc/>
    public override DbSet<UmbracoAccessRule> UmbracoAccessRules => Set<UmbracoAccessRule>();

    /// <inheritdoc/>
    public override DbSet<UmbracoAudit> UmbracoAudits => Set<UmbracoAudit>();

    /// <inheritdoc/>
    public override DbSet<UmbracoCacheInstruction> UmbracoCacheInstructions => Set<UmbracoCacheInstruction>();

    /// <inheritdoc/>
    public override DbSet<UmbracoConsent> UmbracoConsents => Set<UmbracoConsent>();

    /// <inheritdoc/>
    public override DbSet<UmbracoContent> UmbracoContents => Set<UmbracoContent>();

    /// <inheritdoc/>
    public override DbSet<UmbracoContentSchedule> UmbracoContentSchedules => Set<UmbracoContentSchedule>();

    /// <inheritdoc/>
    public override DbSet<UmbracoContentVersion> UmbracoContentVersions => Set<UmbracoContentVersion>();

    /// <inheritdoc/>
    public override DbSet<UmbracoContentVersionCleanupPolicy> UmbracoContentVersionCleanupPolicies => Set<UmbracoContentVersionCleanupPolicy>();

    /// <inheritdoc/>
    public override DbSet<UmbracoContentVersionCultureVariation> UmbracoContentVersionCultureVariations => Set<UmbracoContentVersionCultureVariation>();

    /// <inheritdoc/>
    public override DbSet<UmbracoCreatedPackageSchema> UmbracoCreatedPackageSchemas => Set<UmbracoCreatedPackageSchema>();

    /// <inheritdoc/>
    public override DbSet<UmbracoDataType> UmbracoDataTypes => Set<UmbracoDataType>();

    /// <inheritdoc/>
    public override DbSet<UmbracoDocument> UmbracoDocuments => Set<UmbracoDocument>();

    /// <inheritdoc/>
    public override DbSet<UmbracoDocumentCultureVariation> UmbracoDocumentCultureVariations => Set<UmbracoDocumentCultureVariation>();

    /// <inheritdoc/>
    public override DbSet<UmbracoDocumentVersion> UmbracoDocumentVersions => Set<UmbracoDocumentVersion>();

    /// <inheritdoc/>
    public override DbSet<UmbracoDomain> UmbracoDomains => Set<UmbracoDomain>();

    /// <inheritdoc/>
    public override DbSet<UmbracoExternalLogin> UmbracoExternalLogins => Set<UmbracoExternalLogin>();

    /// <inheritdoc/>
    public override DbSet<UmbracoExternalLoginToken> UmbracoExternalLoginTokens => Set<UmbracoExternalLoginToken>();

    /// <inheritdoc/>
    public override DbSet<UmbracoKeyValue> UmbracoKeyValues => Set<UmbracoKeyValue>();

    /// <inheritdoc/>
    public override DbSet<UmbracoLanguage> UmbracoLanguages => Set<UmbracoLanguage>();

    /// <inheritdoc/>
    public override DbSet<UmbracoLock> UmbracoLocks => Set<UmbracoLock>();

    /// <inheritdoc/>
    public override DbSet<UmbracoLog> UmbracoLogs => Set<UmbracoLog>();

    /// <inheritdoc/>
    public override DbSet<UmbracoLogViewerQuery> UmbracoLogViewerQueries => Set<UmbracoLogViewerQuery>();

    /// <inheritdoc/>
    public override DbSet<UmbracoMediaVersion> UmbracoMediaVersions => Set<UmbracoMediaVersion>();

    /// <inheritdoc/>
    public override DbSet<UmbracoNode> UmbracoNodes => Set<UmbracoNode>();

    /// <remarks>
    /// Not included in <see cref="IUmbracoDatabaseContract"/> because the model depends on <see cref="OpenIddict.EntityFrameworkCore"/>
    /// </remarks>
    public virtual DbSet<UmbracoOpenIddictApplication> UmbracoOpenIddictApplications => Set<UmbracoOpenIddictApplication>();

    /// <remarks>
    /// Not included in <see cref="IUmbracoDatabaseContract"/> because the model depends on <see cref="OpenIddict.EntityFrameworkCore"/>
    /// </remarks>
    public virtual DbSet<UmbracoOpenIddictAuthorization> UmbracoOpenIddictAuthorizations => Set<UmbracoOpenIddictAuthorization>();

    /// <remarks>
    /// Not included in <see cref="IUmbracoDatabaseContract"/> because the model depends on <see cref="OpenIddict.EntityFrameworkCore"/>
    /// </remarks>
    public virtual DbSet<UmbracoOpenIddictScope> UmbracoOpenIddictScopes => Set<UmbracoOpenIddictScope>();

    /// <remarks>
    /// Not included in <see cref="IUmbracoDatabaseContract"/> because the model depends on <see cref="OpenIddict.EntityFrameworkCore"/>
    /// </remarks>
    public virtual DbSet<UmbracoOpenIddictToken> UmbracoOpenIddictTokens => Set<UmbracoOpenIddictToken>();

    /// <inheritdoc/>
    public override DbSet<UmbracoPropertyDatum> UmbracoPropertyData => Set<UmbracoPropertyDatum>();

    /// <inheritdoc/>
    public override DbSet<UmbracoRedirectUrl> UmbracoRedirectUrls => Set<UmbracoRedirectUrl>();

    /// <inheritdoc/>
    public override DbSet<UmbracoRelation> UmbracoRelations => Set<UmbracoRelation>();

    /// <inheritdoc/>
    public override DbSet<UmbracoRelationType> UmbracoRelationTypes => Set<UmbracoRelationType>();

    /// <inheritdoc/>
    public override DbSet<UmbracoServer> UmbracoServers => Set<UmbracoServer>();

    /// <inheritdoc/>
    public override DbSet<UmbracoTwoFactorLogin> UmbracoTwoFactorLogins => Set<UmbracoTwoFactorLogin>();

    /// <inheritdoc/>
    public override DbSet<UmbracoUser> UmbracoUsers => Set<UmbracoUser>();

    /// <inheritdoc/>
    public override DbSet<UmbracoUser2NodeNotify> UmbracoUser2NodeNotifies => Set<UmbracoUser2NodeNotify>();

    /// <inheritdoc/>
    public override DbSet<UmbracoUserGroup> UmbracoUserGroups => Set<UmbracoUserGroup>();

    /// <inheritdoc/>
    public override DbSet<UmbracoUserGroup2App> UmbracoUserGroup2Apps => Set<UmbracoUserGroup2App>();

    /// <inheritdoc/>
    public override DbSet<UmbracoUserGroup2NodePermission> UmbracoUserGroup2NodePermissions => Set<UmbracoUserGroup2NodePermission>();

    /// <inheritdoc/>
    public override DbSet<UmbracoUserLogin> UmbracoUserLogins => Set<UmbracoUserLogin>();

    /// <inheritdoc/>
    public override DbSet<UmbracoUserStartNode> UmbracoUserStartNodes => Set<UmbracoUserStartNode>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetAssembly(typeof(UmbracoDbContext))!);
    }
}
