using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;

namespace Umbraco.Core.Composing.CompositionExtensions
{
    /// <summary>
    /// Composes repositories.
    /// </summary>
    public static class Repositories
    {
        public static Composition ComposeRepositories(this Composition composition)
        {
            // repositories
            composition.RegisterUnique<IAuditRepository, AuditRepository>();
            composition.RegisterUnique<IAuditEntryRepository, AuditEntryRepository>();
            composition.RegisterUnique<IContentTypeRepository, ContentTypeRepository>();
            composition.RegisterUnique<IDataTypeContainerRepository, DataTypeContainerRepository>();
            composition.RegisterUnique<IDataTypeRepository, DataTypeRepository>();
            composition.RegisterUnique<IDictionaryRepository, DictionaryRepository>();
            composition.RegisterUnique<IDocumentBlueprintRepository, DocumentBlueprintRepository>();
            composition.RegisterUnique<IDocumentRepository, DocumentRepository>();
            composition.RegisterUnique<IDocumentTypeContainerRepository, DocumentTypeContainerRepository>();
            composition.RegisterUnique<IDomainRepository, DomainRepository>();
            composition.RegisterUnique<IEntityRepository, EntityRepository>();
            composition.RegisterUnique<IExternalLoginRepository, ExternalLoginRepository>();
            composition.RegisterUnique<ILanguageRepository, LanguageRepository>();
            composition.RegisterUnique<IMacroRepository, MacroRepository>();
            composition.RegisterUnique<IMediaRepository, MediaRepository>();
            composition.RegisterUnique<IMediaTypeContainerRepository, MediaTypeContainerRepository>();
            composition.RegisterUnique<IMediaTypeRepository, MediaTypeRepository>();
            composition.RegisterUnique<IMemberGroupRepository, MemberGroupRepository>();
            composition.RegisterUnique<IMemberRepository, MemberRepository>();
            composition.RegisterUnique<IMemberTypeRepository, MemberTypeRepository>();
            composition.RegisterUnique<INotificationsRepository, NotificationsRepository>();
            composition.RegisterUnique<IPublicAccessRepository, PublicAccessRepository>();
            composition.RegisterUnique<IRedirectUrlRepository, RedirectUrlRepository>();
            composition.RegisterUnique<IRelationRepository, RelationRepository>();
            composition.RegisterUnique<IRelationTypeRepository, RelationTypeRepository>();
            composition.RegisterUnique<IServerRegistrationRepository, ServerRegistrationRepository>();
            composition.RegisterUnique<ITagRepository, TagRepository>();
            composition.RegisterUnique<ITemplateRepository, TemplateRepository>();
            composition.RegisterUnique<IUserGroupRepository, UserGroupRepository>();
            composition.RegisterUnique<IUserRepository, UserRepository>();
            composition.RegisterUnique<IConsentRepository, ConsentRepository>();
            composition.RegisterUnique<IPartialViewMacroRepository, PartialViewMacroRepository>();
            composition.RegisterUnique<IPartialViewRepository, PartialViewRepository>();
            composition.RegisterUnique<IScriptRepository, ScriptRepository>();
            composition.RegisterUnique<IStylesheetRepository, StylesheetRepository>();
            composition.RegisterUnique<IContentTypeCommonRepository, ContentTypeCommonRepository>();
            composition.RegisterUnique<IInstallationRepository, InstallationRepository>();
            composition.RegisterUnique<IUpgradeCheckRepository, UpgradeCheckRepository>();
            composition.RegisterUnique<IDocumentVersionRepository, DocumentVersionRepository>();

            return composition;
        }
    }
}
