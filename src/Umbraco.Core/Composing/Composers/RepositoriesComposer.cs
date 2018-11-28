using Umbraco.Core.Components;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;

namespace Umbraco.Core.Composing.Composers
{
    /// <summary>
    /// Composes repositories.
    /// </summary>
    public static class RepositoriesComposer
    {
        public static Composition ComposeRepositories(this Composition composition)
        {
            // repositories
            composition.RegisterSingleton<IAuditRepository, AuditRepository>();
            composition.RegisterSingleton<IAuditEntryRepository, AuditEntryRepository>();
            composition.RegisterSingleton<IContentTypeRepository, ContentTypeRepository>();
            composition.RegisterSingleton<IDataTypeContainerRepository, DataTypeContainerRepository>();
            composition.RegisterSingleton<IDataTypeRepository, DataTypeRepository>();
            composition.RegisterSingleton<IDictionaryRepository, DictionaryRepository>();
            composition.RegisterSingleton<IDocumentBlueprintRepository, DocumentBlueprintRepository>();
            composition.RegisterSingleton<IDocumentRepository, DocumentRepository>();
            composition.RegisterSingleton<IDocumentTypeContainerRepository, DocumentTypeContainerRepository>();
            composition.RegisterSingleton<IDomainRepository, DomainRepository>();
            composition.RegisterSingleton<IEntityRepository, EntityRepository>();
            composition.RegisterSingleton<IExternalLoginRepository, ExternalLoginRepository>();
            composition.RegisterSingleton<ILanguageRepository, LanguageRepository>();
            composition.RegisterSingleton<IMacroRepository, MacroRepository>();
            composition.RegisterSingleton<IMediaRepository, MediaRepository>();
            composition.RegisterSingleton<IMediaTypeContainerRepository, MediaTypeContainerRepository>();
            composition.RegisterSingleton<IMediaTypeRepository, MediaTypeRepository>();
            composition.RegisterSingleton<IMemberGroupRepository, MemberGroupRepository>();
            composition.RegisterSingleton<IMemberRepository, MemberRepository>();
            composition.RegisterSingleton<IMemberTypeRepository, MemberTypeRepository>();
            composition.RegisterSingleton<INotificationsRepository, NotificationsRepository>();
            composition.RegisterSingleton<IPublicAccessRepository, PublicAccessRepository>();
            composition.RegisterSingleton<IRedirectUrlRepository, RedirectUrlRepository>();
            composition.RegisterSingleton<IRelationRepository, RelationRepository>();
            composition.RegisterSingleton<IRelationTypeRepository, RelationTypeRepository>();
            composition.RegisterSingleton<IServerRegistrationRepository, ServerRegistrationRepository>();
            composition.RegisterSingleton<ITagRepository, TagRepository>();
            composition.RegisterSingleton<ITemplateRepository, TemplateRepository>();
            composition.RegisterSingleton<IUserGroupRepository, UserGroupRepository>();
            composition.RegisterSingleton<IUserRepository, UserRepository>();
            composition.RegisterSingleton<IConsentRepository, ConsentRepository>();
            composition.RegisterSingleton<IPartialViewMacroRepository, PartialViewMacroRepository>();
            composition.RegisterSingleton<IPartialViewRepository, PartialViewRepository>();
            composition.RegisterSingleton<IScriptRepository, ScriptRepository>();
            composition.RegisterSingleton<IStylesheetRepository, StylesheetRepository>();

            return composition;
        }
    }
}
