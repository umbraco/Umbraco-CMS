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

        public RepositoryFactory()
            : this(false)
        {
            
        }

        public RepositoryFactory(bool disableAllCache)
        {
            _disableAllCache = disableAllCache;
        }


        public virtual IContentRepository CreateContentRepository(IDatabaseUnitOfWork uow)
        {
            return new ContentRepository(
                uow,
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current,
                CreateContentTypeRepository(uow),
                CreateTemplateRepository(uow)) { EnsureUniqueNaming = Umbraco.Core.Configuration.UmbracoSettings.EnsureUniqueNaming };
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
                CreateMediaTypeRepository(uow)) { EnsureUniqueNaming = Umbraco.Core.Configuration.UmbracoSettings.EnsureUniqueNaming };
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

        public virtual IStylesheetRepository CreateStylesheetRepository(IUnitOfWork uow, IDatabaseUnitOfWork db)
        {
            return new StylesheetRepository(uow, db);
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
                //There's not many user types but we query on users all the time so the result needs to be cached
                RuntimeCacheProvider.Current);
        }

        internal virtual IUserRepository CreateUserRepository(IDatabaseUnitOfWork uow)
        {
            //TODO: Should we cache users? we did in the legacy API, might be a good idea considering the amount we query for the current user but will
            // need to check that, in v7 with the new forms auth way we shouldn't be querying for a user a lot of times but now that we're wrapping in v6
            // we need to ensure that the constant user lookups are cached!
            return new UserRepository(
                uow,
                NullCacheProvider.Current,
                CreateUserTypeRepository(uow));
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