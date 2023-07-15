using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Persistence.EFCore.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.EFCore;

/// <remarks>
/// To autogenerate migrations use the following commands
/// and insure the 'src/Umbraco.Web.UI/appsettings.json' have a connection string set with the right provider.
///
/// Create a migration for each provider
/// <code>dotnet ef migrations add %Name% -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.SqlServer -c UmbracoDbContext  -- --provider SqlServer</code>
///
/// <code>dotnet ef migrations add %Name% -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.Sqlite -c UmbracoDbContext  -- --provider Sqlite</code>
///
/// Remove the last migration for each provider
/// <code>dotnet ef migrations remove -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.SqlServer -c UmbracoDbContext -- --provider SqlServer</code>
///
/// <code>dotnet ef migrations remove -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.Sqlite -c UmbracoDbContext -- --provider Sqlite</code>
///
/// To find documentation about this way of working with the context see
/// https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/providers?tabs=dotnet-core-cli#using-one-context-type
/// </remarks>
public class UmbracoDbContext : DbContext
{
    public UmbracoDbContext(DbContextOptions<UmbracoDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CmsContentNu> CmsContentNus { get; set; }

    public virtual DbSet<CmsContentType> CmsContentTypes { get; set; }

    public virtual DbSet<CmsContentTypeAllowedContentType> CmsContentTypeAllowedContentTypes { get; set; }

    public virtual DbSet<CmsDictionary> CmsDictionaries { get; set; }

    public virtual DbSet<CmsDocumentType> CmsDocumentTypes { get; set; }

    public virtual DbSet<CmsLanguageText> CmsLanguageTexts { get; set; }

    public virtual DbSet<CmsMacro> CmsMacros { get; set; }

    public virtual DbSet<CmsMacroProperty> CmsMacroProperties { get; set; }

    public virtual DbSet<CmsMember> CmsMembers { get; set; }

    public virtual DbSet<CmsMemberType> CmsMemberTypes { get; set; }

    public virtual DbSet<CmsPropertyType> CmsPropertyTypes { get; set; }

    public virtual DbSet<CmsPropertyTypeGroup> CmsPropertyTypeGroups { get; set; }

    public virtual DbSet<CmsTag> CmsTags { get; set; }

    public virtual DbSet<CmsTagRelationship> CmsTagRelationships { get; set; }

    public virtual DbSet<CmsTemplate> CmsTemplates { get; set; }

    public virtual DbSet<UmbracoAccess> UmbracoAccesses { get; set; }

    public virtual DbSet<UmbracoAccessRule> UmbracoAccessRules { get; set; }

    public virtual DbSet<UmbracoAudit> UmbracoAudits { get; set; }

    public virtual DbSet<UmbracoCacheInstruction> UmbracoCacheInstructions { get; set; }

    public virtual DbSet<UmbracoConsent> UmbracoConsents { get; set; }

    public virtual DbSet<UmbracoContent> UmbracoContents { get; set; }

    public virtual DbSet<UmbracoContentSchedule> UmbracoContentSchedules { get; set; }

    public virtual DbSet<UmbracoContentVersion> UmbracoContentVersions { get; set; }

    public virtual DbSet<UmbracoContentVersionCleanupPolicy> UmbracoContentVersionCleanupPolicies { get; set; }

    public virtual DbSet<UmbracoContentVersionCultureVariation> UmbracoContentVersionCultureVariations { get; set; }

    public virtual DbSet<UmbracoCreatedPackageSchema> UmbracoCreatedPackageSchemas { get; set; }

    public virtual DbSet<UmbracoDataType> UmbracoDataTypes { get; set; }

    public virtual DbSet<UmbracoDocument> UmbracoDocuments { get; set; }

    public virtual DbSet<UmbracoDocumentCultureVariation> UmbracoDocumentCultureVariations { get; set; }

    public virtual DbSet<UmbracoDocumentVersion> UmbracoDocumentVersions { get; set; }

    public virtual DbSet<UmbracoDomain> UmbracoDomains { get; set; }

    public virtual DbSet<UmbracoExternalLogin> UmbracoExternalLogins { get; set; }

    public virtual DbSet<UmbracoExternalLoginToken> UmbracoExternalLoginTokens { get; set; }

    public virtual DbSet<UmbracoKeyValue> UmbracoKeyValues { get; set; }

    public virtual DbSet<UmbracoLanguage> UmbracoLanguages { get; set; }

    public virtual DbSet<UmbracoLock> UmbracoLocks { get; set; }

    public virtual DbSet<UmbracoLog> UmbracoLogs { get; set; }

    public virtual DbSet<UmbracoLogViewerQuery> UmbracoLogViewerQueries { get; set; }

    public virtual DbSet<UmbracoMediaVersion> UmbracoMediaVersions { get; set; }

    public virtual DbSet<UmbracoNode> UmbracoNodes { get; set; }

    public virtual DbSet<UmbracoOpenIddictApplication> UmbracoOpenIddictApplications { get; set; }

    public virtual DbSet<UmbracoOpenIddictAuthorization> UmbracoOpenIddictAuthorizations { get; set; }

    public virtual DbSet<UmbracoOpenIddictScope> UmbracoOpenIddictScopes { get; set; }

    public virtual DbSet<UmbracoOpenIddictToken> UmbracoOpenIddictTokens { get; set; }

    public virtual DbSet<UmbracoPropertyDatum> UmbracoPropertyData { get; set; }

    public virtual DbSet<UmbracoRedirectUrl> UmbracoRedirectUrls { get; set; }

    public virtual DbSet<UmbracoRelation> UmbracoRelations { get; set; }

    public virtual DbSet<UmbracoRelationType> UmbracoRelationTypes { get; set; }

    public virtual DbSet<UmbracoServer> UmbracoServers { get; set; }

    public virtual DbSet<UmbracoTwoFactorLogin> UmbracoTwoFactorLogins { get; set; }

    public virtual DbSet<UmbracoUser> UmbracoUsers { get; set; }

    public virtual DbSet<UmbracoUser2NodeNotify> UmbracoUser2NodeNotifies { get; set; }

    public virtual DbSet<UmbracoUserGroup> UmbracoUserGroups { get; set; }

    public virtual DbSet<UmbracoUserGroup2App> UmbracoUserGroup2Apps { get; set; }

    public virtual DbSet<UmbracoUserGroup2NodePermission> UmbracoUserGroup2NodePermissions { get; set; }

    public virtual DbSet<UmbracoUserLogin> UmbracoUserLogins { get; set; }

    public virtual DbSet<UmbracoUserStartNode> UmbracoUserStartNodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
