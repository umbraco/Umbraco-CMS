using Umbraco.Core.Configuration;
using System;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
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
        private readonly ILogger _logger;
        private readonly CacheHelper _cacheHelper;
        private readonly IUmbracoSettingsSection _settings;

        #region Ctors

        public RepositoryFactory(CacheHelper cacheHelper, ILogger logger, IUmbracoSettingsSection settings)
        {
            if (cacheHelper == null) throw new ArgumentNullException("cacheHelper");
            if (logger == null) throw new ArgumentNullException("logger");
            if (settings == null) throw new ArgumentNullException("settings");
            _disableAllCache = false;
            _cacheHelper = cacheHelper;
            _logger = logger;
            _settings = settings;
        }

        public RepositoryFactory(bool disableAllCache, ILogger logger, IUmbracoSettingsSection settings)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (settings == null) throw new ArgumentNullException("settings");
            _disableAllCache = disableAllCache;
            _logger = logger;
            _settings = settings;
            _cacheHelper = _disableAllCache ? CacheHelper.CreateDisabledCacheHelper() : ApplicationContext.Current.ApplicationCache;
        }

        [Obsolete("Use the ctor specifying all dependencies instead")]
        public RepositoryFactory()
            : this(false, LoggerResolver.Current.Logger, UmbracoConfig.For.UmbracoSettings())
        {
        }

        [Obsolete("Use the ctor specifying an ILogger instead")]
        public RepositoryFactory(CacheHelper cacheHelper)
            : this(false, LoggerResolver.Current.Logger, UmbracoConfig.For.UmbracoSettings())
        {
            if (cacheHelper == null) throw new ArgumentNullException("cacheHelper");
            _disableAllCache = false;
            _cacheHelper = cacheHelper;
        }

        [Obsolete("Use the ctor specifying an ILogger instead")]
        public RepositoryFactory(bool disableAllCache, CacheHelper cacheHelper)
            : this(disableAllCache, LoggerResolver.Current.Logger, UmbracoConfig.For.UmbracoSettings())
        {
            if (cacheHelper == null) throw new ArgumentNullException("cacheHelper");
            _cacheHelper = cacheHelper;
        }

        [Obsolete("Use the ctor specifying an ILogger instead")]
        public RepositoryFactory(bool disableAllCache)
            : this(disableAllCache, LoggerResolver.Current.Logger, UmbracoConfig.For.UmbracoSettings())
        {

        }

     
        #endregion

        public virtual ITagRepository CreateTagRepository(IDatabaseUnitOfWork uow)
        {
            return new TagRepository(
                uow,
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current, _logger);
        }

        public virtual IContentRepository CreateContentRepository(IDatabaseUnitOfWork uow)
        {
            return new ContentRepository(
                uow,                
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current,
                _logger,
                CreateContentTypeRepository(uow),
                CreateTemplateRepository(uow),
                CreateTagRepository(uow),
                _cacheHelper) { EnsureUniqueNaming = _settings.Content.EnsureUniqueNaming };
        }

        public virtual IContentTypeRepository CreateContentTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new ContentTypeRepository(
                uow,
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current,
                _logger,
                CreateTemplateRepository(uow));
        }

        public virtual IDataTypeDefinitionRepository CreateDataTypeDefinitionRepository(IDatabaseUnitOfWork uow)
        {
            return new DataTypeDefinitionRepository(
                uow,
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current,                
                _cacheHelper,
                _logger,
                CreateContentTypeRepository(uow));
        }

        public virtual IDictionaryRepository CreateDictionaryRepository(IDatabaseUnitOfWork uow)
        {
            return new DictionaryRepository(
                uow,
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current,
                _logger,
                CreateLanguageRepository(uow));
        }

        public virtual ILanguageRepository CreateLanguageRepository(IDatabaseUnitOfWork uow)
        {
            return new LanguageRepository(
                uow,
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current,
                _logger);
        }

        public virtual IMediaRepository CreateMediaRepository(IDatabaseUnitOfWork uow)
        {
            return new MediaRepository(
                uow,
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current,
                _logger,
                CreateMediaTypeRepository(uow),
                CreateTagRepository(uow)) { EnsureUniqueNaming = _settings.Content.EnsureUniqueNaming };
        }

        public virtual IMediaTypeRepository CreateMediaTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new MediaTypeRepository(
                uow,
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current,
                _logger);
        }

        public virtual IRelationRepository CreateRelationRepository(IDatabaseUnitOfWork uow)
        {
            return new RelationRepository(
                uow,
                NullCacheProvider.Current,
                _logger,
                CreateRelationTypeRepository(uow));
        }

        public virtual IRelationTypeRepository CreateRelationTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new RelationTypeRepository(
                uow,
                NullCacheProvider.Current,
                _logger);
        }

        public virtual IScriptRepository CreateScriptRepository(IUnitOfWork uow)
        {
            return new ScriptRepository(uow);
        }

        internal virtual IPartialViewRepository CreatePartialViewRepository(IUnitOfWork uow)
        {
            return new PartialViewRepository(uow);
        }

        internal virtual IPartialViewRepository CreatePartialViewMacroRepository(IUnitOfWork uow)
        {
            return new PartialViewMacroRepository(uow);
        }

        public virtual IStylesheetRepository CreateStylesheetRepository(IUnitOfWork uow, IDatabaseUnitOfWork db)
        {
            return new StylesheetRepository(uow, db);
        }

        public virtual ITemplateRepository CreateTemplateRepository(IDatabaseUnitOfWork uow)
        {
            return new TemplateRepository(uow, 
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current, 
                _logger,
                new PhysicalFileSystem(SystemDirectories.Masterpages),
                new PhysicalFileSystem(SystemDirectories.MvcViews),
                _settings.Templates);
        }

        internal virtual ServerRegistrationRepository CreateServerRegistrationRepository(IDatabaseUnitOfWork uow)
        {
            return new ServerRegistrationRepository(
                uow,
                NullCacheProvider.Current,
                _logger);
        }

        public virtual IUserTypeRepository CreateUserTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new UserTypeRepository(
                uow,
                //There's not many user types but we query on users all the time so the result needs to be cached
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current,
                _logger);
        }

        public virtual IUserRepository CreateUserRepository(IDatabaseUnitOfWork uow)
        {            
            return new UserRepository(
                uow,
                //Need to cache users - we look up user information more than anything in the back office!
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current,
                _logger,
                CreateUserTypeRepository(uow),
                _cacheHelper);
        }

        internal virtual IMacroRepository CreateMacroRepository(IDatabaseUnitOfWork uow)
        {
            return new MacroRepository(uow, 
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current,
                _logger);
        }

        public virtual IMemberRepository CreateMemberRepository(IDatabaseUnitOfWork uow)
        {
            return new MemberRepository(
                uow,
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current,
                _logger,
                CreateMemberTypeRepository(uow),
                CreateMemberGroupRepository(uow),
                CreateTagRepository(uow));
        }

        public virtual IMemberTypeRepository CreateMemberTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new MemberTypeRepository(uow, 
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current,
                _logger);
        }

        public virtual IMemberGroupRepository CreateMemberGroupRepository(IDatabaseUnitOfWork uow)
        {
            return new MemberGroupRepository(uow, 
                _disableAllCache ? (IRepositoryCacheProvider)NullCacheProvider.Current : RuntimeCacheProvider.Current, 
                _logger,
                _cacheHelper);
        }

        public virtual IEntityRepository CreateEntityRepository(IDatabaseUnitOfWork uow)
        {
            return new EntityRepository(uow);
        }

    }
}