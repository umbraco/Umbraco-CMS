using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;

namespace Umbraco.Core.Composing.CompositionExtensions
{
    /// <summary>
    /// Composes repositories.
    /// </summary>
    internal static class Repositories
    {
        public static Composition ComposeRepositories(this Composition composition)
        {
            // repositories
            composition.Services.AddUnique<IAuditRepository, AuditRepository>();
            composition.Services.AddUnique<IAuditEntryRepository, AuditEntryRepository>();
            composition.Services.AddUnique<IContentTypeRepository, ContentTypeRepository>();
            composition.Services.AddUnique<IDataTypeContainerRepository, DataTypeContainerRepository>();
            composition.Services.AddUnique<IDataTypeRepository, DataTypeRepository>();
            composition.Services.AddUnique<IDictionaryRepository, DictionaryRepository>();
            composition.Services.AddUnique<IDocumentBlueprintRepository, DocumentBlueprintRepository>();
            composition.Services.AddUnique<IDocumentRepository, DocumentRepository>();
            composition.Services.AddUnique<IDocumentTypeContainerRepository, DocumentTypeContainerRepository>();
            composition.Services.AddUnique<IDomainRepository, DomainRepository>();
            composition.Services.AddUnique<IEntityRepository, EntityRepository>();
            composition.Services.AddUnique<IExternalLoginRepository, ExternalLoginRepository>();
            composition.Services.AddUnique<ILanguageRepository, LanguageRepository>();
            composition.Services.AddUnique<IMacroRepository, MacroRepository>();
            composition.Services.AddUnique<IMediaRepository, MediaRepository>();
            composition.Services.AddUnique<IMediaTypeContainerRepository, MediaTypeContainerRepository>();
            composition.Services.AddUnique<IMediaTypeRepository, MediaTypeRepository>();
            composition.Services.AddUnique<IMemberGroupRepository, MemberGroupRepository>();
            composition.Services.AddUnique<IMemberRepository, MemberRepository>();
            composition.Services.AddUnique<IMemberTypeRepository, MemberTypeRepository>();
            composition.Services.AddUnique<INotificationsRepository, NotificationsRepository>();
            composition.Services.AddUnique<IPublicAccessRepository, PublicAccessRepository>();
            composition.Services.AddUnique<IRedirectUrlRepository, RedirectUrlRepository>();
            composition.Services.AddUnique<IRelationRepository, RelationRepository>();
            composition.Services.AddUnique<IRelationTypeRepository, RelationTypeRepository>();
            composition.Services.AddUnique<IServerRegistrationRepository, ServerRegistrationRepository>();
            composition.Services.AddUnique<ITagRepository, TagRepository>();
            composition.Services.AddUnique<ITemplateRepository, TemplateRepository>();
            composition.Services.AddUnique<IUserGroupRepository, UserGroupRepository>();
            composition.Services.AddUnique<IUserRepository, UserRepository>();
            composition.Services.AddUnique<IConsentRepository, ConsentRepository>();
            composition.Services.AddUnique<IPartialViewMacroRepository, PartialViewMacroRepository>();
            composition.Services.AddUnique<IPartialViewRepository, PartialViewRepository>();
            composition.Services.AddUnique<IScriptRepository, ScriptRepository>();
            composition.Services.AddUnique<IStylesheetRepository, StylesheetRepository>();
            composition.Services.AddUnique<IContentTypeCommonRepository, ContentTypeCommonRepository>();
            composition.Services.AddUnique<IKeyValueRepository, KeyValueRepository>();
            composition.Services.AddUnique<IInstallationRepository, InstallationRepository>();
            composition.Services.AddUnique<IUpgradeCheckRepository, UpgradeCheckRepository>();

            return composition;
        }
    }
}
