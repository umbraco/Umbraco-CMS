using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.DependencyInjection;

/// <summary>
///     Composes repositories.
/// </summary>
public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Adds the Umbraco repositories
    /// </summary>
    internal static IUmbracoBuilder AddRepositories(this IUmbracoBuilder builder)
    {
        // repositories
        builder.Services.AddUnique<IAuditRepository, AuditRepository>();
        builder.Services.AddUnique<IAuditEntryRepository, AuditEntryRepository>();
        builder.Services.AddUnique<ICacheInstructionRepository, CacheInstructionRepository>();
        builder.Services.AddUnique<IContentTypeRepository, ContentTypeRepository>();
        builder.Services.AddUnique<IDataTypeContainerRepository, DataTypeContainerRepository>();
        builder.Services.AddUnique<IDataTypeRepository, DataTypeRepository>();
        builder.Services.AddUnique<IDictionaryRepository, DictionaryRepository>();
        builder.Services.AddUnique<IDocumentBlueprintRepository, DocumentBlueprintRepository>();
        builder.Services.AddUnique<IDocumentRepository, DocumentRepository>();
        builder.Services.AddUnique<IDocumentVersionRepository, DocumentVersionRepository>();
        builder.Services.AddUnique<IDocumentTypeContainerRepository, DocumentTypeContainerRepository>();
        builder.Services.AddUnique<IDomainRepository, DomainRepository>();
        builder.Services.AddMultipleUnique<IEntityRepository, IEntityRepositoryExtended, EntityRepository>();
        builder.Services.AddUnique<ITwoFactorLoginRepository, TwoFactorLoginRepository>();
        builder.Services.AddUnique<ExternalLoginRepository>();
        builder.Services.AddUnique<IExternalLoginRepository>(factory => factory.GetRequiredService<ExternalLoginRepository>());
        builder.Services.AddUnique<IExternalLoginWithKeyRepository>(factory => factory.GetRequiredService<ExternalLoginRepository>());
        builder.Services.AddUnique<ILanguageRepository, LanguageRepository>();
        builder.Services.AddUnique<IMacroRepository, MacroRepository>();
        builder.Services.AddUnique<IMediaRepository, MediaRepository>();
        builder.Services.AddUnique<IMediaTypeContainerRepository, MediaTypeContainerRepository>();
        builder.Services.AddUnique<IMediaTypeRepository, MediaTypeRepository>();
        builder.Services.AddUnique<IMemberGroupRepository, MemberGroupRepository>();
        builder.Services.AddUnique<IMemberRepository, MemberRepository>();
        builder.Services.AddUnique<IMemberTypeContainerRepository, MemberTypeContainerRepository>();
        builder.Services.AddUnique<IMemberTypeRepository, MemberTypeRepository>();
        builder.Services.AddUnique<INotificationsRepository, NotificationsRepository>();
        builder.Services.AddUnique<IPublicAccessRepository, PublicAccessRepository>();
        builder.Services.AddUnique<IRedirectUrlRepository, RedirectUrlRepository>();
        builder.Services.AddUnique<IRelationRepository, RelationRepository>();
        builder.Services.AddUnique<ITrackedReferencesRepository, TrackedReferencesRepository>();
        builder.Services.AddUnique<IRelationTypeRepository, RelationTypeRepository>();
        builder.Services.AddUnique<IServerRegistrationRepository, ServerRegistrationRepository>();
        builder.Services.AddUnique<ITagRepository, TagRepository>();
        builder.Services.AddUnique<ITemplateRepository, TemplateRepository>();
        builder.Services.AddUnique<IUserGroupRepository, UserGroupRepository>();
        builder.Services.AddUnique<IUserRepository, UserRepository>();
        builder.Services.AddUnique<IConsentRepository, ConsentRepository>();
        builder.Services.AddUnique<IPartialViewMacroRepository, PartialViewMacroRepository>();
        builder.Services.AddUnique<IPartialViewRepository, PartialViewRepository>();
        builder.Services.AddUnique<IScriptRepository, ScriptRepository>();
        builder.Services.AddUnique<IStylesheetRepository, StylesheetRepository>();
        builder.Services.AddUnique<IContentTypeCommonRepository, ContentTypeCommonRepository>();
        builder.Services.AddUnique<IKeyValueRepository, KeyValueRepository>();
        builder.Services.AddUnique<IInstallationRepository, InstallationRepository>();
        builder.Services.AddUnique<IUpgradeCheckRepository, UpgradeCheckRepository>();
        builder.Services.AddUnique<ILogViewerQueryRepository, LogViewerQueryRepository>();
        builder.Services.AddUnique<INodeCountRepository, NodeCountRepository>();
        builder.Services.AddUnique<IIdKeyMapRepository, IdKeyMapRepository>();
            builder.Services.AddUnique<IPropertyTypeUsageRepository, PropertyTypeUsageRepository>();
            builder.Services.AddUnique<IDataTypeUsageRepository, DataTypeUsageRepository>();

        return builder;
    }
}
