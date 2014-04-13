using Umbraco.Core.Configuration;
using System;
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
        private readonly CacheHelper _cacheHelper;
        private readonly IUmbracoSettingsSection _settings;

        #region Ctors
        public RepositoryFactory()
            : this(false, UmbracoConfig.For.UmbracoSettings())
        {
        }

        public RepositoryFactory(CacheHelper cacheHelper)
            : this(false, UmbracoConfig.For.UmbracoSettings())
        {
            if (cacheHelper == null) throw new ArgumentNullException("cacheHelper");
            _disableAllCache = false;
            _cacheHelper = cacheHelper;
        }

        public RepositoryFactory(bool disableAllCache, CacheHelper cacheHelper)
            : this(disableAllCache, UmbracoConfig.For.UmbracoSettings())
        {
            if (cacheHelper == null) throw new ArgumentNullException("cacheHelper");
            _cacheHelper = cacheHelper;
        }

        public RepositoryFactory(bool disableAllCache)
            : this(disableAllCache, UmbracoConfig.For.UmbracoSettings())
        {

        }

        internal RepositoryFactory(bool disableAllCache, IUmbracoSettingsSection settings)
        {
            _disableAllCache = disableAllCache;
            _settings = settings;
            _cacheHelper = _disableAllCache ? CacheHelper.CreateDisabledCacheHelper() : ApplicationContext.Current.ApplicationCache;
        }

        internal RepositoryFactory(bool disableAllCache, IUmbracoSettingsSection settings, CacheHelper cacheHelper)
        {
            _disableAllCache = disableAllCache;
            _settings = settings;
            _cacheHelper = cacheHelper;
        } 
        #endregion

        public virtual ITagsRepository CreateTagsRepository(IDatabaseUnitOfWork uow)
        {
            return new TagsRepository(
                uow,
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current);
        }

        public virtual IContentRepository CreateContentRepository(IDatabaseUnitOfWork uow)
        {
            return new ContentRepository(
                uow,                
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current,
                CreateContentTypeRepository(uow),
                CreateTemplateRepository(uow),
                CreateTagsRepository(uow),
                _cacheHelper) { EnsureUniqueNaming = _settings.Content.EnsureUniqueNaming };
        }

        public virtual IContentTypeRepository CreateContentTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new ContentTypeRepository(
                uow,
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current,
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
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current,
                CreateLanguageRepository(uow));
        }

        public virtual ILanguageRepository CreateLanguageRepository(IDatabaseUnitOfWork uow)
        {
            return new LanguageRepository(
                uow,
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current);
        }

        public virtual IMediaRepository CreateMediaRepository(IDatabaseUnitOfWork uow)
        {
            return new MediaRepository(
                uow,
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current,
                CreateMediaTypeRepository(uow),
                CreateTagsRepository(uow)) { EnsureUniqueNaming = _settings.Content.EnsureUniqueNaming };
        }

        public virtual IMediaTypeRepository CreateMediaTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new MediaTypeRepository(
                uow,
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current);
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

        public virtual IUserTypeRepository CreateUserTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new UserTypeRepository(
                uow,
                //There's not many user types but we query on users all the time so the result needs to be cached
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current);
        }

        public virtual IUserRepository CreateUserRepository(IDatabaseUnitOfWork uow)
        {            
            return new UserRepository(
                uow,
                //Need to cache users - we look up user information more than anything in the back office!
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current,
                CreateUserTypeRepository(uow),
                _cacheHelper);
        }

        internal virtual IMacroRepository CreateMacroRepository(IDatabaseUnitOfWork uow)
        {
            return new MacroRepository(uow, _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current);
        }

        public virtual IMemberRepository CreateMemberRepository(IDatabaseUnitOfWork uow)
        {
            return new MemberRepository(
                uow,
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current,
                CreateMemberTypeRepository(uow),
                CreateMemberGroupRepository(uow),
                CreateTagsRepository(uow));
        }

        public virtual IMemberTypeRepository CreateMemberTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new MemberTypeRepository(uow, _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current);
        }

        public virtual IMemberGroupRepository CreateMemberGroupRepository(IDatabaseUnitOfWork uow)
        {
            return new MemberGroupRepository(uow, _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current, _cacheHelper);
        }

        public virtual IEntityRepository CreateEntityRepository(IDatabaseUnitOfWork uow)
        {
            return new EntityRepository(uow);
        }

        internal virtual RecycleBinRepository CreateRecycleBinRepository(IDatabaseUnitOfWork uow)
        {
            return new RecycleBinRepository(uow);
        }
    }
}