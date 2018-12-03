using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LightInject;
using Moq;
using NPoco;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Core.Strings;
using Umbraco.Web.Services;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// Provides objects for tests.
    /// </summary>
    internal partial class TestObjects
    {
        private readonly IServiceContainer _container;

        public TestObjects(IServiceContainer container)
        {
            _container = container;
        }

        /// <summary>
        /// Gets the default ISqlSyntaxProvider objects.
        /// </summary>
        /// <param name="logger">A logger.</param>
        /// <param name="lazyScopeProvider">A (lazy) scope provider.</param>
        /// <returns>The default ISqlSyntaxProvider objects.</returns>
        public IEnumerable<ISqlSyntaxProvider> GetDefaultSqlSyntaxProviders(ILogger logger, Lazy<IScopeProvider> lazyScopeProvider = null)
        {
            return new ISqlSyntaxProvider[]
            {
                new MySqlSyntaxProvider(logger),
                new SqlCeSyntaxProvider(),
                new SqlServerSyntaxProvider(lazyScopeProvider ?? new Lazy<IScopeProvider>(() => null))
            };
        }

        /// <summary>
        /// Gets an UmbracoDatabase.
        /// </summary>
        /// <param name="logger">A logger.</param>
        /// <returns>An UmbracoDatabase.</returns>
        /// <remarks>This is just a void database that has no actual database but pretends to have an open connection
        /// that can begin a transaction.</remarks>
        public UmbracoDatabase GetUmbracoSqlCeDatabase(ILogger logger)
        {
            var syntax = new SqlCeSyntaxProvider();
            var connection = GetDbConnection();
            var sqlContext = new SqlContext(syntax, DatabaseType.SQLCe, Mock.Of<IPocoDataFactory>());
            return new UmbracoDatabase(connection, sqlContext, logger);
        }

        /// <summary>
        /// Gets an UmbracoDatabase.
        /// </summary>
        /// <param name="logger">A logger.</param>
        /// <returns>An UmbracoDatabase.</returns>
        /// <remarks>This is just a void database that has no actual database but pretends to have an open connection
        /// that can begin a transaction.</remarks>
        public UmbracoDatabase GetUmbracoSqlServerDatabase(ILogger logger)
        {
            var syntax = new SqlServerSyntaxProvider(new Lazy<IScopeProvider>(() => null)); // do NOT try to get the server's version!
            var connection = GetDbConnection();
            var sqlContext = new SqlContext(syntax, DatabaseType.SqlServer2008, Mock.Of<IPocoDataFactory>());
            return new UmbracoDatabase(connection, sqlContext, logger);
        }

        public void RegisterServices(IServiceContainer container)
        { }

        /// <summary>
        /// Gets a ServiceContext.
        /// </summary>
        /// <param name="scopeAccessor"></param>
        /// <param name="cache">A cache.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="globalSettings"></param>
        /// <param name="eventMessagesFactory">An event messages factory.</param>
        /// <param name="urlSegmentProviders">Some url segment providers.</param>
        /// <param name="container">A container.</param>
        /// <param name="scopeProvider"></param>
        /// <returns>A ServiceContext.</returns>
        /// <remarks>Should be used sparingly for integration tests only - for unit tests
        /// just mock the services to be passed to the ctor of the ServiceContext.</remarks>
        public ServiceContext GetServiceContext(
            IScopeProvider scopeProvider, IScopeAccessor scopeAccessor,
            CacheHelper cache,
            ILogger logger,
            IGlobalSettings globalSettings,
            IUmbracoSettingsSection umbracoSettings,
            IEventMessagesFactory eventMessagesFactory,
            IEnumerable<IUrlSegmentProvider> urlSegmentProviders,
            TypeLoader typeLoader,
            IServiceFactory container = null)
        {
            if (scopeProvider == null) throw new ArgumentNullException(nameof(scopeProvider));
            if (scopeAccessor == null) throw new ArgumentNullException(nameof(scopeAccessor));
            if (cache == null) throw new ArgumentNullException(nameof(cache));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (eventMessagesFactory == null) throw new ArgumentNullException(nameof(eventMessagesFactory));

            var mediaFileSystem = new MediaFileSystem(Mock.Of<IFileSystem>());

            var externalLoginService = GetLazyService<IExternalLoginService>(container, c => new ExternalLoginService(scopeProvider, logger, eventMessagesFactory, GetRepo<IExternalLoginRepository>(c)));
            var publicAccessService = GetLazyService<IPublicAccessService>(container, c => new PublicAccessService(scopeProvider, logger, eventMessagesFactory, GetRepo<IPublicAccessRepository>(c)));
            var domainService = GetLazyService<IDomainService>(container, c => new DomainService(scopeProvider, logger, eventMessagesFactory, GetRepo<IDomainRepository>(c)));
            var auditService = GetLazyService<IAuditService>(container, c => new AuditService(scopeProvider, logger, eventMessagesFactory, GetRepo<IAuditRepository>(c), GetRepo<IAuditEntryRepository>(c)));

            var localizedTextService = GetLazyService<ILocalizedTextService>(container, c => new LocalizedTextService(
                    new Lazy<LocalizedTextServiceFileSources>(() =>
                    {
                        var mainLangFolder = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.Umbraco + "/config/lang/"));
                        var appPlugins = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.AppPlugins));
                        var configLangFolder = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.Config + "/lang/"));

                        var pluginLangFolders = appPlugins.Exists == false
                            ? Enumerable.Empty<LocalizedTextServiceSupplementaryFileSource>()
                            : appPlugins.GetDirectories()
                                .SelectMany(x => x.GetDirectories("Lang"))
                                .SelectMany(x => x.GetFiles("*.xml", SearchOption.TopDirectoryOnly))
                                .Where(x => Path.GetFileNameWithoutExtension(x.FullName).Length == 5)
                                .Select(x => new LocalizedTextServiceSupplementaryFileSource(x, false));

                        //user defined langs that overwrite the default, these should not be used by plugin creators
                        var userLangFolders = configLangFolder.Exists == false
                            ? Enumerable.Empty<LocalizedTextServiceSupplementaryFileSource>()
                            : configLangFolder
                                .GetFiles("*.user.xml", SearchOption.TopDirectoryOnly)
                                .Where(x => Path.GetFileNameWithoutExtension(x.FullName).Length == 10)
                                .Select(x => new LocalizedTextServiceSupplementaryFileSource(x, true));

                        return new LocalizedTextServiceFileSources(
                            logger,
                            cache.RuntimeCache,
                            mainLangFolder,
                            pluginLangFolders.Concat(userLangFolders));

                    }),
                    logger));

            var runtimeState = Mock.Of<IRuntimeState>();
            var idkMap = new IdkMap(scopeProvider);

            var localizationService = GetLazyService<ILocalizationService>(container, c => new LocalizationService(scopeProvider, logger, eventMessagesFactory, GetRepo<IDictionaryRepository>(c), GetRepo<IAuditRepository>(c), GetRepo<ILanguageRepository>(c)));
            var userService = GetLazyService<IUserService>(container, c => new UserService(scopeProvider, logger, eventMessagesFactory, runtimeState, GetRepo<IUserRepository>(c), GetRepo<IUserGroupRepository>(c),globalSettings));
            var dataTypeService = GetLazyService<IDataTypeService>(container, c => new DataTypeService(scopeProvider, logger, eventMessagesFactory, GetRepo<IDataTypeRepository>(c), GetRepo<IDataTypeContainerRepository>(c), GetRepo<IAuditRepository>(c), GetRepo<IEntityRepository>(c), GetRepo<IContentTypeRepository>(c)));
            var contentService = GetLazyService<IContentService>(container, c => new ContentService(scopeProvider, logger, eventMessagesFactory, mediaFileSystem, GetRepo<IDocumentRepository>(c), GetRepo<IEntityRepository>(c), GetRepo<IAuditRepository>(c), GetRepo<IContentTypeRepository>(c), GetRepo<IDocumentBlueprintRepository>(c), GetRepo<ILanguageRepository>(c)));
            var notificationService = GetLazyService<INotificationService>(container, c => new NotificationService(scopeProvider, userService.Value, contentService.Value, localizationService.Value, logger, GetRepo<INotificationsRepository>(c), globalSettings, umbracoSettings.Content));
            var serverRegistrationService = GetLazyService<IServerRegistrationService>(container, c => new ServerRegistrationService(scopeProvider, logger, eventMessagesFactory, GetRepo<IServerRegistrationRepository>(c)));
            var memberGroupService = GetLazyService<IMemberGroupService>(container, c => new MemberGroupService(scopeProvider, logger, eventMessagesFactory, GetRepo<IMemberGroupRepository>(c)));
            var memberService = GetLazyService<IMemberService>(container, c => new MemberService(scopeProvider, logger, eventMessagesFactory, memberGroupService.Value, mediaFileSystem, GetRepo<IMemberRepository>(c), GetRepo<IMemberTypeRepository>(c), GetRepo<IMemberGroupRepository>(c), GetRepo<IAuditRepository>(c)));
            var mediaService = GetLazyService<IMediaService>(container, c => new MediaService(scopeProvider, mediaFileSystem, logger, eventMessagesFactory, GetRepo<IMediaRepository>(c), GetRepo<IAuditRepository>(c), GetRepo<IMediaTypeRepository>(c), GetRepo<IEntityRepository>(c)));
            var contentTypeService = GetLazyService<IContentTypeService>(container, c => new ContentTypeService(scopeProvider, logger, eventMessagesFactory, contentService.Value, GetRepo<IContentTypeRepository>(c), GetRepo<IAuditRepository>(c), GetRepo<IDocumentTypeContainerRepository>(c), GetRepo<IEntityRepository>(c)));
            var mediaTypeService = GetLazyService<IMediaTypeService>(container, c => new MediaTypeService(scopeProvider, logger, eventMessagesFactory, mediaService.Value, GetRepo<IMediaTypeRepository>(c), GetRepo<IAuditRepository>(c), GetRepo<IMediaTypeContainerRepository>(c), GetRepo<IEntityRepository>(c)));
            var fileService = GetLazyService<IFileService>(container, c => new FileService(scopeProvider, logger, eventMessagesFactory, GetRepo<IStylesheetRepository>(c), GetRepo<IScriptRepository>(c), GetRepo<ITemplateRepository>(c), GetRepo<IPartialViewRepository>(c), GetRepo<IPartialViewMacroRepository>(c), GetRepo<IAuditRepository>(c)));

            var memberTypeService = GetLazyService<IMemberTypeService>(container, c => new MemberTypeService(scopeProvider, logger, eventMessagesFactory, memberService.Value, GetRepo<IMemberTypeRepository>(c), GetRepo<IAuditRepository>(c), GetRepo<IEntityRepository>(c)));
            var entityService = GetLazyService<IEntityService>(container, c => new EntityService(
                scopeProvider, logger, eventMessagesFactory,
                    contentService.Value, contentTypeService.Value, mediaService.Value, mediaTypeService.Value, dataTypeService.Value, memberService.Value, memberTypeService.Value,
                    idkMap,
                    GetRepo<IEntityRepository>(c)));

            var macroService = GetLazyService<IMacroService>(container, c => new MacroService(scopeProvider, logger, eventMessagesFactory, GetRepo<IMacroRepository>(c), GetRepo<IAuditRepository>(c)));
            var packagingService = GetLazyService<IPackagingService>(container, c => new PackagingService(logger, contentService.Value, contentTypeService.Value, mediaService.Value, macroService.Value, dataTypeService.Value, fileService.Value, localizationService.Value, entityService.Value, userService.Value, scopeProvider, urlSegmentProviders, GetRepo<IAuditRepository>(c), GetRepo<IContentTypeRepository>(c), new PropertyEditorCollection(new DataEditorCollection(Enumerable.Empty<DataEditor>()))));
            var relationService = GetLazyService<IRelationService>(container, c => new RelationService(scopeProvider, logger, eventMessagesFactory, entityService.Value, GetRepo<IRelationRepository>(c), GetRepo<IRelationTypeRepository>(c)));
            var treeService = GetLazyService<IApplicationTreeService>(container, c => new ApplicationTreeService(logger, cache, typeLoader));
            var tagService = GetLazyService<ITagService>(container, c => new TagService(scopeProvider, logger, eventMessagesFactory, GetRepo<ITagRepository>(c)));
            var sectionService = GetLazyService<ISectionService>(container, c => new SectionService(userService.Value, treeService.Value, scopeProvider, cache));
            var redirectUrlService = GetLazyService<IRedirectUrlService>(container, c => new RedirectUrlService(scopeProvider, logger, eventMessagesFactory, GetRepo<IRedirectUrlRepository>(c)));
            var consentService = GetLazyService<IConsentService>(container, c => new ConsentService(scopeProvider, logger, eventMessagesFactory, GetRepo<IConsentRepository>(c)));

            return new ServiceContext(
                publicAccessService,
                domainService,
                auditService,
                localizedTextService,
                tagService,
                contentService,
                userService,
                memberService,
                mediaService,
                contentTypeService,
                mediaTypeService,
                dataTypeService,
                fileService,
                localizationService,
                packagingService,
                serverRegistrationService,
                entityService,
                relationService,
                treeService,
                sectionService,
                macroService,
                memberTypeService,
                memberGroupService,
                notificationService,
                externalLoginService,
                redirectUrlService,
                consentService);
        }

        private Lazy<T> GetLazyService<T>(IServiceFactory container, Func<IServiceFactory, T> ctor)
            where T : class
        {
            return new Lazy<T>(() => container?.TryGetInstance<T>() ?? ctor(container));
        }

        private T GetRepo<T>(IServiceFactory container)
            where T : class, IRepository
        {
            return container?.TryGetInstance<T>() ?? Mock.Of<T>();
        }

        public IScopeProvider GetScopeProvider(ILogger logger, FileSystems fileSystems = null, IUmbracoDatabaseFactory databaseFactory = null)
        {
            if (databaseFactory == null)
            {
                //var mappersBuilder = new MapperCollectionBuilder(Current.Container); // fixme
                //mappersBuilder.AddCore();
                //var mappers = mappersBuilder.CreateCollection();
                var mappers = Current.Container.GetInstance<IMapperCollection>();
                databaseFactory = new UmbracoDatabaseFactory(Constants.System.UmbracoConnectionName, GetDefaultSqlSyntaxProviders(logger), logger, mappers);
            }

            fileSystems = fileSystems ?? new FileSystems(logger);
            var scopeProvider = new ScopeProvider(databaseFactory, fileSystems, logger);
            return scopeProvider;
        }
    }
}
