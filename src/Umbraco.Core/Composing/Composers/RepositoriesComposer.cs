using LightInject;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;

namespace Umbraco.Core.Composing.Composers
{
    /// <summary>
    /// Composes repositories.
    /// </summary>
    public static class RepositoriesComposer
    {
        public static IServiceRegistry ComposeRepositories(this IServiceRegistry registry)
        {
            // repositories
            registry.RegisterSingleton<IAuditRepository, AuditRepository>();
            registry.RegisterSingleton<IAuditEntryRepository, AuditEntryRepository>();
            registry.RegisterSingleton<IContentTypeRepository, ContentTypeRepository>();
            registry.RegisterSingleton<IDataTypeContainerRepository, DataTypeContainerRepository>();
            registry.RegisterSingleton<IDataTypeRepository, DataTypeRepository>();
            registry.RegisterSingleton<IDictionaryRepository, DictionaryRepository>();
            registry.RegisterSingleton<IDocumentBlueprintRepository, DocumentBlueprintRepository>();
            registry.RegisterSingleton<IDocumentRepository, DocumentRepository>();
            registry.RegisterSingleton<IDocumentTypeContainerRepository, DocumentTypeContainerRepository>();
            registry.RegisterSingleton<IDomainRepository, DomainRepository>();
            registry.RegisterSingleton<IEntityRepository, EntityRepository>();
            registry.RegisterSingleton<IExternalLoginRepository, ExternalLoginRepository>();
            registry.RegisterSingleton<ILanguageRepository, LanguageRepository>();
            registry.RegisterSingleton<IMacroRepository, MacroRepository>();
            registry.RegisterSingleton<IMediaRepository, MediaRepository>();
            registry.RegisterSingleton<IMediaTypeContainerRepository, MediaTypeContainerRepository>();
            registry.RegisterSingleton<IMediaTypeRepository, MediaTypeRepository>();
            registry.RegisterSingleton<IMemberGroupRepository, MemberGroupRepository>();
            registry.RegisterSingleton<IMemberRepository, MemberRepository>();
            registry.RegisterSingleton<IMemberTypeRepository, MemberTypeRepository>();
            registry.RegisterSingleton<INotificationsRepository, NotificationsRepository>();
            registry.RegisterSingleton<IPublicAccessRepository, PublicAccessRepository>();
            registry.RegisterSingleton<IRedirectUrlRepository, RedirectUrlRepository>();
            registry.RegisterSingleton<IRelationRepository, RelationRepository>();
            registry.RegisterSingleton<IRelationTypeRepository, RelationTypeRepository>();
            registry.RegisterSingleton<IServerRegistrationRepository, ServerRegistrationRepository>();
            registry.RegisterSingleton<ITagRepository, TagRepository>();
            registry.RegisterSingleton<ITaskRepository, TaskRepository>();
            registry.RegisterSingleton<ITaskTypeRepository, TaskTypeRepository>();
            registry.RegisterSingleton<ITemplateRepository, TemplateRepository>();
            registry.RegisterSingleton<IUserGroupRepository, UserGroupRepository>();
            registry.RegisterSingleton<IUserRepository, UserRepository>();
            registry.RegisterSingleton<IConsentRepository, ConsentRepository>();
            registry.RegisterSingleton<IPartialViewMacroRepository, PartialViewMacroRepository>();
            registry.RegisterSingleton<IPartialViewRepository, PartialViewRepository>();
            registry.RegisterSingleton<IScriptRepository, ScriptRepository>();
            registry.RegisterSingleton<IStylesheetRepository, StylesheetRepository>();

            return registry;
        }
    }
}
