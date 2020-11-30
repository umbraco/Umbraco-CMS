using System;
using System.IO;
using System.Linq;
using Moq;
using NPoco;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Events;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Packaging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers.Stubs;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// Provides objects for tests.
    /// </summary>
    internal partial class TestObjects
    {
        private readonly IRegister _register;

        public TestObjects(IRegister register)
        {
            _register = register;
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
            return new UmbracoDatabase(connection, sqlContext, logger, TestHelper.BulkSqlInsertProvider);
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
            var syntax = new SqlServerSyntaxProvider(); // do NOT try to get the server's version!
            var connection = GetDbConnection();
            var sqlContext = new SqlContext(syntax, DatabaseType.SqlServer2008, Mock.Of<IPocoDataFactory>());
            return new UmbracoDatabase(connection, sqlContext, logger, TestHelper.BulkSqlInsertProvider);
        }

        public void RegisterServices(IRegister register)
        { }

        /// <summary>
        /// Gets a ServiceContext.
        /// </summary>
        /// <param name="scopeProvider"></param>
        /// <param name="scopeAccessor"></param>
        /// <param name="cache">A cache.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="ioHelper">An io helper.</param>
        /// <param name="globalSettings"></param>
        /// <param name="contentSettings"></param>
        /// <param name="eventMessagesFactory">An event messages factory.</param>
        /// <param name="urlSegmentProviders">Some URL segment providers.</param>
        /// <param name="umbracoVersion">An Umbraco Version.</param>
        /// <param name="factory">A container.</param>
        /// <returns>A ServiceContext.</returns>
        /// <remarks>Should be used sparingly for integration tests only - for unit tests
        /// just mock the services to be passed to the ctor of the ServiceContext.</remarks>
        public ServiceContext GetServiceContext(IScopeProvider scopeProvider, IScopeAccessor scopeAccessor,
            AppCaches cache,
            ILogger logger,
            IIOHelper ioHelper,
            IGlobalSettings globalSettings,
            IContentSettings contentSettings,
            IEventMessagesFactory eventMessagesFactory,
            UrlSegmentProviderCollection urlSegmentProviders,
            IUmbracoVersion umbracoVersion,
            IFactory factory = null)
        {
            if (scopeProvider == null) throw new ArgumentNullException(nameof(scopeProvider));
            if (scopeAccessor == null) throw new ArgumentNullException(nameof(scopeAccessor));
            if (cache == null) throw new ArgumentNullException(nameof(cache));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (eventMessagesFactory == null) throw new ArgumentNullException(nameof(eventMessagesFactory));

            var scheme = Mock.Of<IMediaPathScheme>();

            var shortStringHelper = Mock.Of<IShortStringHelper>();

            var mediaFileSystem = new MediaFileSystem(Mock.Of<IFileSystem>(), scheme, logger, shortStringHelper);

            var externalLoginService = GetLazyService<IExternalLoginService>(factory, c => new ExternalLoginService(scopeProvider, logger, eventMessagesFactory, GetRepo<IExternalLoginRepository>(c)));
            var publicAccessService = GetLazyService<IPublicAccessService>(factory, c => new PublicAccessService(scopeProvider, logger, eventMessagesFactory, GetRepo<IPublicAccessRepository>(c)));
            var domainService = GetLazyService<IDomainService>(factory, c => new DomainService(scopeProvider, logger, eventMessagesFactory, GetRepo<IDomainRepository>(c)));
            var auditService = GetLazyService<IAuditService>(factory, c => new AuditService(scopeProvider, logger, eventMessagesFactory, GetRepo<IAuditRepository>(c), GetRepo<IAuditEntryRepository>(c)));

            var localizedTextService = GetLazyService<ILocalizedTextService>(factory, c => new LocalizedTextService(
                    new Lazy<LocalizedTextServiceFileSources>(() =>
                    {
                        var mainLangFolder = new DirectoryInfo(ioHelper.MapPath(Current.Configs.Global().UmbracoPath + "/config/lang/"));
                        var appPlugins = new DirectoryInfo(ioHelper.MapPath(Constants.SystemDirectories.AppPlugins));
                        var configLangFolder = new DirectoryInfo(ioHelper.MapPath(Constants.SystemDirectories.Config + "/lang/"));

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
                            cache,
                            mainLangFolder,
                            pluginLangFolders.Concat(userLangFolders));

                    }),
                    logger));

            var runtimeState = Mock.Of<IRuntimeState>();
            var idkMap = new IdKeyMap(scopeProvider);

            var propertyEditorCollection = new PropertyEditorCollection(new DataEditorCollection(Enumerable.Empty<DataEditor>()));

            var localizationService = GetLazyService<ILocalizationService>(factory, c => new LocalizationService(scopeProvider, logger, eventMessagesFactory, GetRepo<IDictionaryRepository>(c), GetRepo<IAuditRepository>(c), GetRepo<ILanguageRepository>(c)));
            var userService = GetLazyService<IUserService>(factory, c => new UserService(scopeProvider, logger, eventMessagesFactory, runtimeState, GetRepo<IUserRepository>(c), GetRepo<IUserGroupRepository>(c),globalSettings));
            var dataTypeService = GetLazyService<IDataTypeService>(factory, c => new DataTypeService(scopeProvider, logger, eventMessagesFactory, GetRepo<IDataTypeRepository>(c), GetRepo<IDataTypeContainerRepository>(c), GetRepo<IAuditRepository>(c), GetRepo<IEntityRepository>(c), GetRepo<IContentTypeRepository>(c), ioHelper, localizedTextService.Value, localizationService.Value, TestHelper.ShortStringHelper));
            var propertyValidationService = new Lazy<IPropertyValidationService>(() => new PropertyValidationService(propertyEditorCollection, dataTypeService.Value, localizedTextService.Value));
            var contentService = GetLazyService<IContentService>(factory, c => new ContentService(scopeProvider, logger, eventMessagesFactory, GetRepo<IDocumentRepository>(c), GetRepo<IEntityRepository>(c), GetRepo<IAuditRepository>(c), GetRepo<IContentTypeRepository>(c), GetRepo<IDocumentBlueprintRepository>(c), GetRepo<ILanguageRepository>(c), propertyValidationService, TestHelper.ShortStringHelper));
            var notificationService = GetLazyService<INotificationService>(factory, c => new NotificationService(scopeProvider, userService.Value, contentService.Value, localizationService.Value, logger, ioHelper, GetRepo<INotificationsRepository>(c), globalSettings, contentSettings));
            var serverRegistrationService = GetLazyService<IServerRegistrationService>(factory, c => new ServerRegistrationService(scopeProvider, logger, eventMessagesFactory, GetRepo<IServerRegistrationRepository>(c), TestHelper.GetHostingEnvironment()));
            var memberGroupService = GetLazyService<IMemberGroupService>(factory, c => new MemberGroupService(scopeProvider, logger, eventMessagesFactory, GetRepo<IMemberGroupRepository>(c)));
            var memberService = GetLazyService<IMemberService>(factory, c => new MemberService(scopeProvider, logger, eventMessagesFactory, memberGroupService.Value, GetRepo<IMemberRepository>(c), GetRepo<IMemberTypeRepository>(c), GetRepo<IMemberGroupRepository>(c), GetRepo<IAuditRepository>(c)));
            var mediaService = GetLazyService<IMediaService>(factory, c => new MediaService(scopeProvider, mediaFileSystem, logger, eventMessagesFactory, GetRepo<IMediaRepository>(c), GetRepo<IAuditRepository>(c), GetRepo<IMediaTypeRepository>(c), GetRepo<IEntityRepository>(c), TestHelper.ShortStringHelper));
            var contentTypeService = GetLazyService<IContentTypeService>(factory, c => new ContentTypeService(scopeProvider, logger, eventMessagesFactory, contentService.Value, GetRepo<IContentTypeRepository>(c), GetRepo<IAuditRepository>(c), GetRepo<IDocumentTypeContainerRepository>(c), GetRepo<IEntityRepository>(c)));
            var mediaTypeService = GetLazyService<IMediaTypeService>(factory, c => new MediaTypeService(scopeProvider, logger, eventMessagesFactory, mediaService.Value, GetRepo<IMediaTypeRepository>(c), GetRepo<IAuditRepository>(c), GetRepo<IMediaTypeContainerRepository>(c), GetRepo<IEntityRepository>(c)));
            var fileService = GetLazyService<IFileService>(factory, c => new FileService(scopeProvider, ioHelper, logger, eventMessagesFactory, GetRepo<IStylesheetRepository>(c), GetRepo<IScriptRepository>(c), GetRepo<ITemplateRepository>(c), GetRepo<IPartialViewRepository>(c), GetRepo<IPartialViewMacroRepository>(c), GetRepo<IAuditRepository>(c), TestHelper.ShortStringHelper, globalSettings));

            var memberTypeService = GetLazyService<IMemberTypeService>(factory, c => new MemberTypeService(scopeProvider, logger, eventMessagesFactory, memberService.Value, GetRepo<IMemberTypeRepository>(c), GetRepo<IAuditRepository>(c), GetRepo<IEntityRepository>(c)));
            var entityService = GetLazyService<IEntityService>(factory, c => new EntityService(scopeProvider, logger, eventMessagesFactory, idkMap, GetRepo<IEntityRepository>(c)));

            var macroService = GetLazyService<IMacroService>(factory, c => new MacroService(scopeProvider, logger, eventMessagesFactory, GetRepo<IMacroRepository>(c), GetRepo<IAuditRepository>(c)));
            var packagingService = GetLazyService<IPackagingService>(factory, c =>
            {
                var compiledPackageXmlParser = new CompiledPackageXmlParser(new ConflictingPackageData(macroService.Value, fileService.Value), globalSettings);
                return new PackagingService(
                    auditService.Value,
                    new PackagesRepository(contentService.Value, contentTypeService.Value, dataTypeService.Value, fileService.Value, macroService.Value, localizationService.Value, ioHelper,
                        new EntityXmlSerializer(contentService.Value, mediaService.Value, dataTypeService.Value, userService.Value, localizationService.Value, contentTypeService.Value, urlSegmentProviders, TestHelper.ShortStringHelper, propertyEditorCollection), logger, umbracoVersion, globalSettings, "createdPackages.config"),
                    new PackagesRepository(contentService.Value, contentTypeService.Value, dataTypeService.Value, fileService.Value, macroService.Value, localizationService.Value, ioHelper,
                        new EntityXmlSerializer(contentService.Value, mediaService.Value, dataTypeService.Value, userService.Value, localizationService.Value, contentTypeService.Value, urlSegmentProviders, TestHelper.ShortStringHelper, propertyEditorCollection), logger, umbracoVersion,  globalSettings, "installedPackages.config"),
                    new PackageInstallation(
                        new PackageDataInstallation(logger, fileService.Value, macroService.Value, localizationService.Value, dataTypeService.Value, entityService.Value, contentTypeService.Value, contentService.Value, propertyEditorCollection, scopeProvider, shortStringHelper, GetGlobalSettings(), localizedTextService.Value),
                        new PackageFileInstallation(compiledPackageXmlParser, ioHelper, new ProfilingLogger(logger, new TestProfiler())),
                        compiledPackageXmlParser, Mock.Of<IPackageActionRunner>(),
                        Mock.Of<IHostingEnvironment>(x => x.ApplicationPhysicalPath == ioHelper.MapPath("~"))),
                    ioHelper);
            });
            var relationService = GetLazyService<IRelationService>(factory, c => new RelationService(scopeProvider, logger, eventMessagesFactory, entityService.Value, GetRepo<IRelationRepository>(c), GetRepo<IRelationTypeRepository>(c), GetRepo<IAuditRepository>(c)));
            var tagService = GetLazyService<ITagService>(factory, c => new TagService(scopeProvider, logger, eventMessagesFactory, GetRepo<ITagRepository>(c)));
            var redirectUrlService = GetLazyService<IRedirectUrlService>(factory, c => new RedirectUrlService(scopeProvider, logger, eventMessagesFactory, GetRepo<IRedirectUrlRepository>(c)));
            var consentService = GetLazyService<IConsentService>(factory, c => new ConsentService(scopeProvider, logger, eventMessagesFactory, GetRepo<IConsentRepository>(c)));
            var keyValueService = GetLazyService<IKeyValueService>(factory, c => new KeyValueService(scopeProvider, GetRepo<IKeyValueRepository>(c)));
            var contentTypeServiceBaseFactory = GetLazyService<IContentTypeBaseServiceProvider>(factory, c => new ContentTypeBaseServiceProvider(factory.GetInstance<IContentTypeService>(),factory.GetInstance<IMediaTypeService>(),factory.GetInstance<IMemberTypeService>()));

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
                macroService,
                memberTypeService,
                memberGroupService,
                notificationService,
                externalLoginService,
                redirectUrlService,
                consentService,
                keyValueService,
                contentTypeServiceBaseFactory);
        }

        private Lazy<T> GetLazyService<T>(IFactory container, Func<IFactory, T> ctor)
            where T : class
        {
            return new Lazy<T>(() => container?.TryGetInstance<T>() ?? ctor(container));
        }

        private T GetRepo<T>(IFactory container)
            where T : class, IRepository
        {
            return container?.TryGetInstance<T>() ?? Mock.Of<T>();
        }

        public IScopeProvider GetScopeProvider(ILogger logger, ITypeFinder typeFinder = null, FileSystems fileSystems = null, IUmbracoDatabaseFactory databaseFactory = null)
        {
            if (databaseFactory == null)
            {
                // var mappersBuilder = new MapperCollectionBuilder(Current.Container); // FIXME:
                // mappersBuilder.AddCore();
                // var mappers = mappersBuilder.CreateCollection();
                var mappers = Current.Factory.GetInstance<IMapperCollection>();
                databaseFactory = new UmbracoDatabaseFactory(logger,
                    SettingsForTests.DefaultGlobalSettings,
                    new ConnectionStrings(),
                    Constants.System.UmbracoConnectionName,
                    new Lazy<IMapperCollection>(() => mappers),
                    TestHelper.DbProviderFactoryCreator);
            }

            typeFinder ??= new TypeFinder(logger, new DefaultUmbracoAssemblyProvider(GetType().Assembly), new VaryingRuntimeHash());
            fileSystems ??= new FileSystems(Current.Factory, logger, TestHelper.IOHelper, SettingsForTests.GenerateMockGlobalSettings());
            var coreDebug = TestHelper.CoreDebugSettings;
            var mediaFileSystem = Mock.Of<IMediaFileSystem>();
            var scopeProvider = new ScopeProvider(databaseFactory, fileSystems, coreDebug, mediaFileSystem, logger, typeFinder, NoAppCache.Instance);
            return scopeProvider;
        }

    }
}
