using Umbraco.Core.Configuration;
using System;
using System.ComponentModel;
using System.Web.Security;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Security;

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
        private readonly CacheHelper _nullCache;
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

                    //if the result is already a DeepCloneRuntimeCacheProvider then return it, otherwise
                    //wrap the result with a DeepCloneRuntimeCacheProvider
                    return cache is DeepCloneRuntimeCacheProvider
                        ? cache
                        : new DeepCloneRuntimeCacheProvider(cache);
                };
            }

            _nullCache = CacheHelper.NoCache;
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
            _nullCache = CacheHelper.NoCache;
        }

        [Obsolete("Use the ctor specifying all dependencies instead")]
        public RepositoryFactory(bool disableAllCache)
            : this(disableAllCache ? CacheHelper.NoCache : ApplicationContext.Current.ApplicationCache, LoggerResolver.Current.Logger, SqlSyntaxContext.SqlSyntaxProvider, UmbracoConfig.For.UmbracoSettings())
        {
        }


        #endregion

        public virtual IExternalLoginRepository CreateExternalLoginRepository(IScopeUnitOfWork uow)
        {
            return new ExternalLoginRepository(uow,
                _cacheHelper,
                _logger, _sqlSyntax);
        }

        public virtual IPublicAccessRepository CreatePublicAccessRepository(IScopeUnitOfWork uow)
        {
            return new PublicAccessRepository(uow,
                _cacheHelper,
                _logger, _sqlSyntax);
        }

        public virtual ITaskRepository CreateTaskRepository(IScopeUnitOfWork uow)
        {
            return new TaskRepository(uow,
                _nullCache, //never cache
                _logger, _sqlSyntax);
        }

        public virtual IAuditRepository CreateAuditRepository(IScopeUnitOfWork uow)
        {
            return new AuditRepository(uow,
                _nullCache, //never cache
                _logger, _sqlSyntax);
        }

        public virtual ITagRepository CreateTagRepository(IScopeUnitOfWork uow)
        {
            return new TagRepository(
                uow,
                _cacheHelper, _logger, _sqlSyntax);
        }

        public virtual IContentRepository CreateContentRepository(IScopeUnitOfWork uow)
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

        public virtual IContentRepository CreateContentBlueprintRepository(IScopeUnitOfWork uow)
        {
            return new ContentBlueprintRepository(
                uow,
                _cacheHelper,
                _logger,
                _sqlSyntax,
                CreateContentTypeRepository(uow),
                CreateTemplateRepository(uow),
                CreateTagRepository(uow),
                _settings.Content)
            {
                //duplicates are allowed
                EnsureUniqueNaming = false
            };
        }

        public virtual IContentTypeRepository CreateContentTypeRepository(IScopeUnitOfWork uow)
        {
            return new ContentTypeRepository(
                uow,
                _cacheHelper,
                _logger, _sqlSyntax,
                CreateTemplateRepository(uow));
        }

        public virtual IDataTypeDefinitionRepository CreateDataTypeDefinitionRepository(IScopeUnitOfWork uow)
        {
            return new DataTypeDefinitionRepository(
                uow,
                _cacheHelper,
                _logger, _sqlSyntax,
                CreateContentTypeRepository(uow));
        }

        public virtual IDictionaryRepository CreateDictionaryRepository(IScopeUnitOfWork uow)
        {
            return new DictionaryRepository(
                uow,
                _cacheHelper,
                _logger,
                _sqlSyntax);
        }

        public virtual ILanguageRepository CreateLanguageRepository(IScopeUnitOfWork uow)
        {
            return new LanguageRepository(
                uow,
                _cacheHelper,
                _logger, _sqlSyntax);
        }

        public virtual IMediaRepository CreateMediaRepository(IScopeUnitOfWork uow)
        {
            return new MediaRepository(
                uow,
                _cacheHelper,
                _logger, _sqlSyntax,
                CreateMediaTypeRepository(uow),
                CreateTagRepository(uow),
                _settings.Content);
        }

        public virtual IMediaTypeRepository CreateMediaTypeRepository(IScopeUnitOfWork uow)
        {
            return new MediaTypeRepository(
                uow,
                _cacheHelper,
                _logger, _sqlSyntax);
        }

        public virtual IRelationRepository CreateRelationRepository(IScopeUnitOfWork uow)
        {
            return new RelationRepository(
                uow,
                _nullCache,
                _logger, _sqlSyntax,
                CreateRelationTypeRepository(uow));
        }

        public virtual IRelationTypeRepository CreateRelationTypeRepository(IScopeUnitOfWork uow)
        {
            return new RelationTypeRepository(
                uow,
                _cacheHelper,
                _logger, _sqlSyntax);
        }

        public virtual IScriptRepository CreateScriptRepository(IUnitOfWork uow)
        {
            return new ScriptRepository(uow, FileSystemProviderManager.Current.ScriptsFileSystem, _settings.Content);
        }

        internal virtual IPartialViewRepository CreatePartialViewRepository(IUnitOfWork uow)
        {
            return new PartialViewRepository(uow, FileSystemProviderManager.Current.PartialViewsFileSystem);
        }

        internal virtual IPartialViewRepository CreatePartialViewMacroRepository(IUnitOfWork uow)
        {
            return new PartialViewMacroRepository(uow, FileSystemProviderManager.Current.MacroPartialsFileSystem);
        }

        [Obsolete("MacroScripts are obsolete - this is for backwards compatibility with upgraded sites.")]
        internal virtual IPartialViewRepository CreateMacroScriptRepository(IUnitOfWork uow)
        {
            return new MacroScriptRepository(uow, FileSystemProviderManager.Current.MacroScriptsFileSystem);
        }

        [Obsolete("UserControls are obsolete - this is for backwards compatibility with upgraded sites.")]
        internal virtual IUserControlRepository CreateUserControlRepository(IUnitOfWork uow)
        {
            return new UserControlRepository(uow, FileSystemProviderManager.Current.UserControlsFileSystem);
        }

        public virtual IStylesheetRepository CreateStylesheetRepository(IUnitOfWork uow)
        {
            return new StylesheetRepository(uow, FileSystemProviderManager.Current.StylesheetsFileSystem);
        }

        [Obsolete("Do not use this method, use the method with only the single unit of work parameter")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual IStylesheetRepository CreateStylesheetRepository(IUnitOfWork uow, IDatabaseUnitOfWork db)
        {
            // note: the second unit of work is ignored.
            return new StylesheetRepository(uow, FileSystemProviderManager.Current.StylesheetsFileSystem);
        }

        public virtual ITemplateRepository CreateTemplateRepository(IScopeUnitOfWork uow)
        {
            return new TemplateRepository(uow,
                _cacheHelper,
                _logger, _sqlSyntax,
                FileSystemProviderManager.Current.MasterPagesFileSystem,
                FileSystemProviderManager.Current.MvcViewsFileSystem,
                _settings.Templates);
        }

        public virtual IXsltFileRepository CreateXsltFileRepository(IUnitOfWork uow)
        {
            return new XsltFileRepository(uow, FileSystemProviderManager.Current.XsltFileSystem);
        }

        public virtual IMigrationEntryRepository CreateMigrationEntryRepository(IScopeUnitOfWork uow)
        {
            return new MigrationEntryRepository(
                uow,
                _nullCache, //never cache
                _logger, _sqlSyntax);
        }

        public virtual IServerRegistrationRepository CreateServerRegistrationRepository(IScopeUnitOfWork uow)
        {
            return new ServerRegistrationRepository(
                uow,
                _cacheHelper.StaticCache,
                _logger, _sqlSyntax);
        }

        public virtual IUserGroupRepository CreateUserGroupRepository(IScopeUnitOfWork uow)
        {
            return new UserGroupRepository(
                uow,
                _cacheHelper,
                _logger, _sqlSyntax);
        }

        public virtual IUserRepository CreateUserRepository(IScopeUnitOfWork uow)
        {
            var userMembershipProvider = MembershipProviderExtensions.GetUsersMembershipProvider();
            var passwordConfig = userMembershipProvider == null || userMembershipProvider.PasswordFormat != MembershipPasswordFormat.Hashed
                ? null
                : new System.Collections.Generic.Dictionary<string, string> {{"hashAlgorithm", Membership.HashAlgorithmType}};

            return new UserRepository(
                uow,
                //Need to cache users - we look up user information more than anything in the back office!
                _cacheHelper,
                _logger,
                _sqlSyntax,
                passwordConfig);
        }

        internal virtual IMacroRepository CreateMacroRepository(IScopeUnitOfWork uow)
        {
            return new MacroRepository(uow,
                _cacheHelper,
                _logger, _sqlSyntax);
        }

        public virtual IMemberRepository CreateMemberRepository(IScopeUnitOfWork uow)
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

        public virtual IMemberTypeRepository CreateMemberTypeRepository(IScopeUnitOfWork uow)
        {
            return new MemberTypeRepository(uow,
                _cacheHelper,
                _logger, _sqlSyntax);
        }

        public virtual IMemberGroupRepository CreateMemberGroupRepository(IScopeUnitOfWork uow)
        {
            return new MemberGroupRepository(uow,
                _cacheHelper,
                _logger, _sqlSyntax);
        }

        public virtual IEntityRepository CreateEntityRepository(IScopeUnitOfWork uow)
        {
            return new EntityRepository(uow);
        }

        public virtual IDomainRepository CreateDomainRepository(IScopeUnitOfWork uow)
        {
            return new DomainRepository(uow, _cacheHelper, _logger, _sqlSyntax);
        }

        public ITaskTypeRepository CreateTaskTypeRepository(IScopeUnitOfWork uow)
        {
            return new TaskTypeRepository(uow,
                _nullCache, //never cache
                _logger, _sqlSyntax);
        }

        internal virtual EntityContainerRepository CreateEntityContainerRepository(IScopeUnitOfWork uow, Guid containerObjectType)
        {
            return new EntityContainerRepository(
                uow,
                _cacheHelper,
                _logger, _sqlSyntax,
                containerObjectType);
        }

        public IRedirectUrlRepository CreateRedirectUrlRepository(IScopeUnitOfWork uow)
        {
            return new RedirectUrlRepository(
                uow,
                _cacheHelper,
                _logger,
                _sqlSyntax);
        }

        public IConsentRepository CreateConsentRepository(IScopeUnitOfWork uow)
        {
            return new ConsentRepository(uow, _cacheHelper, _logger, _sqlSyntax);
        }

        public IAuditEntryRepository CreateAuditEntryRepository(IScopeUnitOfWork uow)
        {
            return new AuditEntryRepository(uow, _cacheHelper, _logger, _sqlSyntax);
        }
    }
}
