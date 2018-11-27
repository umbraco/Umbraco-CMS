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
            var container = composition.Container;

            // repositories
            container.RegisterSingleton<IAuditRepository, AuditRepository>();
            container.RegisterSingleton<IAuditEntryRepository, AuditEntryRepository>();
            container.RegisterSingleton<IContentTypeRepository, ContentTypeRepository>();
            container.RegisterSingleton<IDataTypeContainerRepository, DataTypeContainerRepository>();
            container.RegisterSingleton<IDataTypeRepository, DataTypeRepository>();
            container.RegisterSingleton<IDictionaryRepository, DictionaryRepository>();
            container.RegisterSingleton<IDocumentBlueprintRepository, DocumentBlueprintRepository>();
            container.RegisterSingleton<IDocumentRepository, DocumentRepository>();
            container.RegisterSingleton<IDocumentTypeContainerRepository, DocumentTypeContainerRepository>();
            container.RegisterSingleton<IDomainRepository, DomainRepository>();
            container.RegisterSingleton<IEntityRepository, EntityRepository>();
            container.RegisterSingleton<IExternalLoginRepository, ExternalLoginRepository>();
            container.RegisterSingleton<ILanguageRepository, LanguageRepository>();
            container.RegisterSingleton<IMacroRepository, MacroRepository>();
            container.RegisterSingleton<IMediaRepository, MediaRepository>();
            container.RegisterSingleton<IMediaTypeContainerRepository, MediaTypeContainerRepository>();
            container.RegisterSingleton<IMediaTypeRepository, MediaTypeRepository>();
            container.RegisterSingleton<IMemberGroupRepository, MemberGroupRepository>();
            container.RegisterSingleton<IMemberRepository, MemberRepository>();
            container.RegisterSingleton<IMemberTypeRepository, MemberTypeRepository>();
            container.RegisterSingleton<INotificationsRepository, NotificationsRepository>();
            container.RegisterSingleton<IPublicAccessRepository, PublicAccessRepository>();
            container.RegisterSingleton<IRedirectUrlRepository, RedirectUrlRepository>();
            container.RegisterSingleton<IRelationRepository, RelationRepository>();
            container.RegisterSingleton<IRelationTypeRepository, RelationTypeRepository>();
            container.RegisterSingleton<IServerRegistrationRepository, ServerRegistrationRepository>();
            container.RegisterSingleton<ITagRepository, TagRepository>();
            container.RegisterSingleton<ITemplateRepository, TemplateRepository>();
            container.RegisterSingleton<IUserGroupRepository, UserGroupRepository>();
            container.RegisterSingleton<IUserRepository, UserRepository>();
            container.RegisterSingleton<IConsentRepository, ConsentRepository>();
            container.RegisterSingleton<IPartialViewMacroRepository, PartialViewMacroRepository>();
            container.RegisterSingleton<IPartialViewRepository, PartialViewRepository>();
            container.RegisterSingleton<IScriptRepository, ScriptRepository>();
            container.RegisterSingleton<IStylesheetRepository, StylesheetRepository>();

            return composition;
        }
    }
}
