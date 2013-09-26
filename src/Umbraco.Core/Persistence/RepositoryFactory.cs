using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Used to instantiate each repository type
    /// </summary>
    public class RepositoryFactory
    {
        private readonly bool _disableAllCache;
        private readonly IUmbracoSettingsSection _settings;

        public RepositoryFactory()
            : this(false, UmbracoConfig.For.UmbracoSettings())
        {
            
        }

        internal RepositoryFactory(bool disableAllCache)
            : this(disableAllCache, UmbracoConfig.For.UmbracoSettings())
        {

        }

        internal RepositoryFactory(bool disableAllCache, IUmbracoSettingsSection settings)
        {
            _disableAllCache = disableAllCache;
            _settings = settings;

        }


        public virtual IContentRepository CreateContentRepository(IDatabaseUnitOfWork uow)
        {
            return new ContentRepository(
                uow,
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current,
                CreateContentTypeRepository(uow),
                CreateTemplateRepository(uow)) { EnsureUniqueNaming = _settings.Content.EnsureUniqueNaming };
        }

        public virtual IContentTypeRepository CreateContentTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new ContentTypeRepository(
                uow,
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : InMemoryCacheProvider.Current,
                new TemplateRepository(uow, NullCacheProvider.Current));
        }

        public virtual IDataTypeDefinitionRepository CreateDataTypeDefinitionRepository(IDatabaseUnitOfWork uow)
        {
            return new DataTypeDefinitionRepository(
                uow,
                NullCacheProvider.Current);
        }

        public virtual IDictionaryRepository CreateDictionaryRepository(IDatabaseUnitOfWork uow)
        {
            return new DictionaryRepository(
                uow,
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : InMemoryCacheProvider.Current,
                CreateLanguageRepository(uow));
        }

        public virtual ILanguageRepository CreateLanguageRepository(IDatabaseUnitOfWork uow)
        {
            return new LanguageRepository(
                uow,
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : InMemoryCacheProvider.Current);
        }

        public virtual IMediaRepository CreateMediaRepository(IDatabaseUnitOfWork uow)
        {
            return new MediaRepository(
                uow,
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current,
                CreateMediaTypeRepository(uow)) { EnsureUniqueNaming = _settings.Content.EnsureUniqueNaming };
        }

        public virtual IMediaTypeRepository CreateMediaTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new MediaTypeRepository(
                uow,
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : InMemoryCacheProvider.Current);
        }

        public virtual IRelationRepository CreateRelationRepository(IDatabaseUnitOfWork uow)
        {
            return new RelationRepository(
                uow,
                NullCacheProvider.Current,
                CreateRelationTypeRepository(uow));
        }

        public virtual IRelationTypeRepository CreateRelationTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new RelationTypeRepository(
                uow,
                NullCacheProvider.Current);
        }

        public virtual IScriptRepository CreateScriptRepository(IUnitOfWork uow)
        {
            return new ScriptRepository(uow);
        }

        public virtual IStylesheetRepository CreateStylesheetRepository(IUnitOfWork uow)
        {
            return new StylesheetRepository(uow);
        }

        public virtual ITemplateRepository CreateTemplateRepository(IDatabaseUnitOfWork uow)
        {
            return new TemplateRepository(uow, _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current);
        }

        internal virtual ServerRegistrationRepository CreateServerRegistrationRepository(IDatabaseUnitOfWork uow)
        {
            return new ServerRegistrationRepository(
                uow,
                NullCacheProvider.Current);
        }

        internal virtual IUserTypeRepository CreateUserTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new UserTypeRepository(
                uow,
                NullCacheProvider.Current);
        }

        internal virtual IUserRepository CreateUserRepository(IDatabaseUnitOfWork uow)
        {
            return new UserRepository(
                uow,
                NullCacheProvider.Current,
                CreateUserTypeRepository(uow));
        }

        internal virtual IMacroRepository CreateMacroRepository(IDatabaseUnitOfWork uow)
        {
            return new MacroRepository(uow, _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current);
        }

        internal virtual IMemberRepository CreateMemberRepository(IDatabaseUnitOfWork uow)
        {
            return new MemberRepository(uow, _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current, CreateMemberTypeRepository(uow));
        }

        internal virtual IMemberTypeRepository CreateMemberTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new MemberTypeRepository(uow, _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : InMemoryCacheProvider.Current);
        }

        internal virtual IEntityRepository CreateEntityRepository(IDatabaseUnitOfWork uow)
        {
            return new EntityRepository(uow);
        }

        internal virtual RecycleBinRepository CreateRecycleBinRepository(IDatabaseUnitOfWork uow)
        {
            return new RecycleBinRepository(uow);
        }
    }
}