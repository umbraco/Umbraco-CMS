using System;
using LightInject;
using Umbraco.Core.Cache;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;

namespace Umbraco.Core.Composing.CompositionRoots
{
    /// <summary>
    /// Sets the IoC container for the umbraco data layer/repositories/sql/database/etc...
    /// </summary>
    public sealed class RepositoryCompositionRoot : ICompositionRoot
    {
        public const string DisabledCache = "DisabledCache";

        public void Compose(IServiceRegistry container)
        {
            // register cache helpers
            // the main cache helper is registered by CoreBootManager and is used by most repositories
            // the disabled one is used by those repositories that have an annotated ctor parameter
            container.RegisterSingleton(factory => CacheHelper.CreateDisabledCacheHelper(), DisabledCache);

            // resolve ctor dependency from GetInstance() runtimeArguments, if possible - 'factory' is
            // the container, 'info' describes the ctor argument, and 'args' contains the args that
            // were passed to GetInstance() - use first arg if it is the right type,
            //
            // for ...
            //container.RegisterConstructorDependency((factory, info, args) =>
            //{
            //    if (info.Member.DeclaringType != typeof(EntityContainerRepository)) return default;
            //    return args.Length > 0 && args[0] is Guid guid ? guid : default;
            //});

            // register repositories
            // repos depend on various things,
            // some repositories have an annotated ctor parameter to pick the right cache helper

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

            // repositories that depend on a filesystem
            // these have an annotated ctor parameter to pick the right file system
            container.RegisterSingleton<IPartialViewMacroRepository, PartialViewMacroRepository>();
            container.RegisterSingleton<IPartialViewRepository, PartialViewRepository>();
            container.RegisterSingleton<IScriptRepository, ScriptRepository>();
            container.RegisterSingleton<IStylesheetRepository, StylesheetRepository>();
        }
    }
}
