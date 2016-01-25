using Umbraco.Core.Configuration;
using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Used to instantiate each repository type
    /// </summary>
    public class RepositoryFactory
    {
        private readonly ILogger _logger;
        private readonly ISqlSyntaxProvider _sqlSyntax;
        private readonly CacheHelper _cacheHelper;
        private readonly CacheHelper _noCache;
        private readonly IUmbracoSettingsSection _settings;

        #region Ctors

        public RepositoryFactory(CacheHelper cacheHelper, ILogger logger, ISqlSyntaxProvider sqlSyntax, IUmbracoSettingsSection settings)
        {
            if (cacheHelper == null) throw new ArgumentNullException("cacheHelper");
            if (logger == null) throw new ArgumentNullException("logger");
            //if (sqlSyntax == null) throw new ArgumentNullException("sqlSyntax");
            if (settings == null) throw new ArgumentNullException("settings");

            _cacheHelper = cacheHelper;            

            //IMPORTANT: We will force the DeepCloneRuntimeCacheProvider to be used here which is a wrapper for the underlying
            // runtime cache to ensure that anything that can be deep cloned in/out is done so, this also ensures that our tracks
            // changes entities are reset.
            if ((_cacheHelper.RuntimeCache is DeepCloneRuntimeCacheProvider) == false)
            {
                var origRuntimeCache = cacheHelper.RuntimeCache;
                _cacheHelper.RuntimeCache = new DeepCloneRuntimeCacheProvider(origRuntimeCache);
            }
            //If the factory for isolated cache doesn't return DeepCloneRuntimeCacheProvider, then ensure it does
            if (_cacheHelper.IsolatedRuntimeCache.CacheFactory.Method.ReturnType != typeof (DeepCloneRuntimeCacheProvider))
            {
                var origFactory = cacheHelper.IsolatedRuntimeCache.CacheFactory;
                _cacheHelper.IsolatedRuntimeCache.CacheFactory = type =>
                {
                    var cache = origFactory(type);
                    return new DeepCloneRuntimeCacheProvider(cache);
                };
            }

            _noCache = CacheHelper.CreateDisabledCacheHelper();
            _logger = logger;
            _sqlSyntax = sqlSyntax;
            _settings = settings;
        }

        [Obsolete("Use the ctor specifying all dependencies instead")]
        public RepositoryFactory()
            : this(ApplicationContext.Current.ApplicationCache, LoggerResolver.Current.Logger, SqlSyntaxContext.SqlSyntaxProvider, UmbracoConfig.For.UmbracoSettings())
        {
        }

        [Obsolete("Use the ctor specifying all dependencies instead")]
        public RepositoryFactory(CacheHelper cacheHelper)
            : this(cacheHelper, LoggerResolver.Current.Logger, SqlSyntaxContext.SqlSyntaxProvider, UmbracoConfig.For.UmbracoSettings())
        {
        }

        [Obsolete("Use the ctor specifying all dependencies instead, NOTE: disableAllCache has zero effect")]        
        public RepositoryFactory(bool disableAllCache, CacheHelper cacheHelper)
            : this(cacheHelper, LoggerResolver.Current.Logger, SqlSyntaxContext.SqlSyntaxProvider, UmbracoConfig.For.UmbracoSettings())
        {
            if (cacheHelper == null) throw new ArgumentNullException("cacheHelper");
            _cacheHelper = cacheHelper;
            _noCache = CacheHelper.CreateDisabledCacheHelper();
        }

        [Obsolete("Use the ctor specifying all dependencies instead")]
        public RepositoryFactory(bool disableAllCache)
            : this(disableAllCache ? CacheHelper.CreateDisabledCacheHelper() : ApplicationContext.Current.ApplicationCache, LoggerResolver.Current.Logger, SqlSyntaxContext.SqlSyntaxProvider, UmbracoConfig.For.UmbracoSettings())
        {
        }

     
        #endregion

        public virtual IExternalLoginRepository CreateExternalLoginRepository(IDatabaseUnitOfWork uow)
        {
            return new ExternalLoginRepository(uow,
                _cacheHelper,
                _logger, _sqlSyntax);
        }

        public virtual IPublicAccessRepository CreatePublicAccessRepository(IDatabaseUnitOfWork uow)
        {
            return new PublicAccessRepository(uow,
                _cacheHelper,
                _logger, _sqlSyntax);
        }

        public virtual ITaskRepository CreateTaskRepository(IDatabaseUnitOfWork uow)
        {
            return new TaskRepository(uow,
                _noCache, //never cache
                _logger, _sqlSyntax);
        }

        public virtual IAuditRepository CreateAuditRepository(IDatabaseUnitOfWork uow)
        {
            return new AuditRepository(uow,
                _noCache, //never cache
                _logger, _sqlSyntax);
        }

        public virtual ITagRepository CreateTagRepository(IDatabaseUnitOfWork uow)
        {
            return new TagRepository(
                uow,
                _cacheHelper, _logger, _sqlSyntax);
        }

        public virtual IContentRepository CreateContentRepository(IDatabaseUnitOfWork uow)
        {
            return new ContentRepository(
                uow,
                _cacheHelper,
                _logger,
                _sqlSyntax,
                CreateContentTypeRepository(uow),
                CreateTemplateRepository(uow),
                CreateTagRepository(uow),
                _settings.Content)
            {
                EnsureUniqueNaming = _settings.Content.EnsureUniqueNaming
            };
        }

        public virtual IContentTypeRepository CreateContentTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new ContentTypeRepository(
                uow,
                _cacheHelper,
                _logger, _sqlSyntax,
                CreateTemplateRepository(uow));
        }

        public virtual IDataTypeDefinitionRepository CreateDataTypeDefinitionRepository(IDatabaseUnitOfWork uow)
        {
            return new DataTypeDefinitionRepository(
                uow,
                _cacheHelper,
                _logger, _sqlSyntax,
                CreateContentTypeRepository(uow));
        }

        public virtual IDictionaryRepository CreateDictionaryRepository(IDatabaseUnitOfWork uow)
        {
            return new DictionaryRepository(
                uow,
                _cacheHelper,
                _logger,
                _sqlSyntax);
        }

        public virtual ILanguageRepository CreateLanguageRepository(IDatabaseUnitOfWork uow)
        {
            return new LanguageRepository(
                uow,
                _cacheHelper,
                _logger, _sqlSyntax);
        }

        public virtual IMediaRepository CreateMediaRepository(IDatabaseUnitOfWork uow)
        {
            return new MediaRepository(
                uow,
                _cacheHelper,
                _logger, _sqlSyntax,
                CreateMediaTypeRepository(uow),
                CreateTagRepository(uow),
                _settings.Content);
        }

        public virtual IMediaTypeRepository CreateMediaTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new MediaTypeRepository(
                uow,
                _cacheHelper,
                _logger, _sqlSyntax);
        }

        public virtual IRelationRepository CreateRelationRepository(IDatabaseUnitOfWork uow)
        {
            return new RelationRepository(
                uow,
                _noCache, //never cache
                _logger, _sqlSyntax,
                CreateRelationTypeRepository(uow));
        }

        public virtual IRelationTypeRepository CreateRelationTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new RelationTypeRepository(
                uow,
                _noCache, //never cache
                _logger, _sqlSyntax);
        }

        public virtual IScriptRepository CreateScriptRepository(IUnitOfWork uow)
        {
            return new ScriptRepository(uow, new PhysicalFileSystem(SystemDirectories.Scripts), _settings.Content);
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
            return new StylesheetRepository(uow, new PhysicalFileSystem(SystemDirectories.Css));
        }

        public virtual ITemplateRepository CreateTemplateRepository(IDatabaseUnitOfWork uow)
        {
            return new TemplateRepository(uow, 
                _cacheHelper,
                _logger, _sqlSyntax,
                new PhysicalFileSystem(SystemDirectories.Masterpages),
                new PhysicalFileSystem(SystemDirectories.MvcViews),
                _settings.Templates);
        }

        public virtual IMigrationEntryRepository CreateMigrationEntryRepository(IDatabaseUnitOfWork uow)
        {
            return new MigrationEntryRepository(
                uow,
                _noCache, //never cache
                _logger, _sqlSyntax);
        }

        public virtual IServerRegistrationRepository CreateServerRegistrationRepository(IDatabaseUnitOfWork uow)
        {
            return new ServerRegistrationRepository(
                uow,
                _cacheHelper.StaticCache,
                _logger, _sqlSyntax);
        }

        public virtual IUserTypeRepository CreateUserTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new UserTypeRepository(
                uow,
                //There's not many user types but we query on users all the time so the result needs to be cached
                _cacheHelper,
                _logger, _sqlSyntax);
        }

        public virtual IUserRepository CreateUserRepository(IDatabaseUnitOfWork uow)
        {            
            return new UserRepository(
                uow,
                //Need to cache users - we look up user information more than anything in the back office!
                _cacheHelper,
                _logger, _sqlSyntax,
                CreateUserTypeRepository(uow));
        }

        internal virtual IMacroRepository CreateMacroRepository(IDatabaseUnitOfWork uow)
        {
            return new MacroRepository(uow,
                _cacheHelper,
                _logger, _sqlSyntax);
        }

        public virtual IMemberRepository CreateMemberRepository(IDatabaseUnitOfWork uow)
        {
            return new MemberRepository(
                uow,
                _cacheHelper,
                _logger, _sqlSyntax,
                CreateMemberTypeRepository(uow),
                CreateMemberGroupRepository(uow),
                CreateTagRepository(uow),
                _settings.Content);
        }

        public virtual IMemberTypeRepository CreateMemberTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new MemberTypeRepository(uow,
                _cacheHelper,
                _logger, _sqlSyntax);
        }

        public virtual IMemberGroupRepository CreateMemberGroupRepository(IDatabaseUnitOfWork uow)
        {
            return new MemberGroupRepository(uow,
                _cacheHelper,
                _logger, _sqlSyntax);
        }

        public virtual IEntityRepository CreateEntityRepository(IDatabaseUnitOfWork uow)
        {
            return new EntityRepository(uow);
        }

        public virtual IDomainRepository CreateDomainRepository(IDatabaseUnitOfWork uow)
        {
            return new DomainRepository(uow, _cacheHelper, _logger, _sqlSyntax);
        }

        public ITaskTypeRepository CreateTaskTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new TaskTypeRepository(uow,
                _noCache, //never cache
                _logger, _sqlSyntax);
        }

        internal virtual EntityContainerRepository CreateEntityContainerRepository(IDatabaseUnitOfWork uow, Guid containerObjectType)
        {
            return new EntityContainerRepository(
                uow,
                _cacheHelper,
                _logger, _sqlSyntax,
                containerObjectType);
        }
    }
}