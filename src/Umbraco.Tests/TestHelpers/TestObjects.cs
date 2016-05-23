using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using LightInject;
using Moq;
using NPoco;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Web.Services;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// Provides objects for tests.
    /// </summary>
    static partial class TestObjects
    {
        /// <summary>
        /// Gets the default ISqlSyntaxProvider objects.
        /// </summary>
        /// <param name="logger">A logger.</param>
        /// <param name="lazyFactory">A (lazy) database factory.</param>
        /// <returns>The default ISqlSyntaxProvider objects.</returns>
        public static IEnumerable<ISqlSyntaxProvider> GetDefaultSqlSyntaxProviders(ILogger logger, Lazy<IDatabaseFactory> lazyFactory = null)
        {
            return new ISqlSyntaxProvider[]
            {
                new MySqlSyntaxProvider(logger),
                new SqlCeSyntaxProvider(),
                new SqlServerSyntaxProvider(lazyFactory ?? new Lazy<IDatabaseFactory>(() => null))
            };
        }

        /// <summary>
        /// Gets an UmbracoDatabase.
        /// </summary>
        /// <param name="logger">A logger.</param>
        /// <returns>An UmbracoDatabase.</returns>
        /// <remarks>This is just a void database that has no actual database but pretends to have an open connection
        /// that can begin a transaction.</remarks>
        public static UmbracoDatabase GetUmbracoSqlCeDatabase(ILogger logger)
        {
            var syntax = new SqlCeSyntaxProvider();
            var dbProviderFactory = DbProviderFactories.GetFactory(Constants.DbProviderNames.SqlCe);
            var connection = TestObjects.GetDbConnection();
            return new UmbracoDatabase(connection, syntax, DatabaseType.SQLCe, dbProviderFactory, logger);
        }

        /// <summary>
        /// Gets an UmbracoDatabase.
        /// </summary>
        /// <param name="logger">A logger.</param>
        /// <returns>An UmbracoDatabase.</returns>
        /// <remarks>This is just a void database that has no actual database but pretends to have an open connection
        /// that can begin a transaction.</remarks>
        public static UmbracoDatabase GetUmbracoSqlServerDatabase(ILogger logger)
        {
            var syntax = new SqlServerSyntaxProvider(new Lazy<IDatabaseFactory>(() => null)); // do NOT try to get the server's version!
            var dbProviderFactory = DbProviderFactories.GetFactory(Constants.DbProviderNames.SqlServer);
            var connection = TestObjects.GetDbConnection();
            return new UmbracoDatabase(connection, syntax, DatabaseType.SqlServer2008, dbProviderFactory, logger);
        }

        /// <summary>
        /// Gets a ServiceContext.
        /// </summary>
        /// <param name="repositoryFactory">A repository factory.</param>
        /// <param name="dbUnitOfWorkProvider">A database unit of work provider.</param>
        /// <param name="fileUnitOfWorkProvider">A file unit of work provider.</param>
        /// <param name="cache">A cache.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="eventMessagesFactory">An event messages factory.</param>
        /// <param name="urlSegmentProviders">Some url segment providers.</param>
        /// <returns>A ServiceContext.</returns>
        /// <remarks>Should be used sparingly for integration tests only - for unit tests
        /// just mock the services to be passed to the ctor of the ServiceContext.</remarks>
        public static ServiceContext GetServiceContext(RepositoryFactory repositoryFactory,
            IDatabaseUnitOfWorkProvider dbUnitOfWorkProvider,
            IUnitOfWorkProvider fileUnitOfWorkProvider,
            CacheHelper cache,
            ILogger logger,
            IEventMessagesFactory eventMessagesFactory,
            IEnumerable<IUrlSegmentProvider> urlSegmentProviders)
        {
            if (repositoryFactory == null) throw new ArgumentNullException(nameof(repositoryFactory));
            if (dbUnitOfWorkProvider == null) throw new ArgumentNullException(nameof(dbUnitOfWorkProvider));
            if (fileUnitOfWorkProvider == null) throw new ArgumentNullException(nameof(fileUnitOfWorkProvider));
            if (cache == null) throw new ArgumentNullException(nameof(cache));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (eventMessagesFactory == null) throw new ArgumentNullException(nameof(eventMessagesFactory));

            var provider = dbUnitOfWorkProvider;
            var fileProvider = fileUnitOfWorkProvider;

            var migrationEntryService = new Lazy<IMigrationEntryService>(() => new MigrationEntryService(provider, logger, eventMessagesFactory));
            var externalLoginService = new Lazy<IExternalLoginService>(() => new ExternalLoginService(provider, logger, eventMessagesFactory));
            var publicAccessService = new Lazy<IPublicAccessService>(() => new PublicAccessService(provider, logger, eventMessagesFactory));
            var taskService = new Lazy<ITaskService>(() => new TaskService(provider, logger, eventMessagesFactory));
            var domainService = new Lazy<IDomainService>(() => new DomainService(provider, logger, eventMessagesFactory));
            var auditService = new Lazy<IAuditService>(() => new AuditService(provider, logger, eventMessagesFactory));

            var localizedTextService = new Lazy<ILocalizedTextService>(() => new LocalizedTextService(
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

            var userService = new Lazy<IUserService>(() => new UserService(provider, logger, eventMessagesFactory));
            var dataTypeService = new Lazy<IDataTypeService>(() => new DataTypeService(provider, logger, eventMessagesFactory));
            var contentService = new Lazy<IContentService>(() => new ContentService(provider, logger, eventMessagesFactory, dataTypeService.Value, userService.Value, urlSegmentProviders));
            var notificationService = new Lazy<INotificationService>(() => new NotificationService(provider, userService.Value, contentService.Value, repositoryFactory, logger));
            var serverRegistrationService = new Lazy<IServerRegistrationService>(() => new ServerRegistrationService(provider, logger, eventMessagesFactory));
            var memberGroupService = new Lazy<IMemberGroupService>(() => new MemberGroupService(provider, logger, eventMessagesFactory));
            var memberService = new Lazy<IMemberService>(() => new MemberService(provider, logger, eventMessagesFactory, memberGroupService.Value, dataTypeService.Value));
            var mediaService = new Lazy<IMediaService>(() => new MediaService(provider, logger, eventMessagesFactory, dataTypeService.Value, userService.Value, urlSegmentProviders));
            var contentTypeService = new Lazy<IContentTypeService>(() => new ContentTypeService(provider, logger, eventMessagesFactory, contentService.Value));
            var mediaTypeService = new Lazy<IMediaTypeService>(() => new MediaTypeService(provider, logger, eventMessagesFactory, mediaService.Value));
            var fileService = new Lazy<IFileService>(() => new FileService(fileProvider, provider, logger, eventMessagesFactory));
            var localizationService = new Lazy<ILocalizationService>(() => new LocalizationService(provider, logger, eventMessagesFactory));

            var memberTypeService = new Lazy<IMemberTypeService>(() => new MemberTypeService(provider, logger, eventMessagesFactory, memberService.Value));
            var entityService = new Lazy<IEntityService>(() => new EntityService(
                    provider, logger, eventMessagesFactory,
                    contentService.Value, contentTypeService.Value, mediaService.Value, mediaTypeService.Value, dataTypeService.Value, memberService.Value, memberTypeService.Value,
                    //TODO: Consider making this an isolated cache instead of using the global one
                    cache.RuntimeCache));

            var macroService = new Lazy<IMacroService>(() => new MacroService(provider, logger, eventMessagesFactory));
            var packagingService = new Lazy<IPackagingService>(() => new PackagingService(logger, contentService.Value, contentTypeService.Value, mediaService.Value, macroService.Value, dataTypeService.Value, fileService.Value, localizationService.Value, entityService.Value, userService.Value, repositoryFactory, provider, urlSegmentProviders));
            var relationService = new Lazy<IRelationService>(() => new RelationService(provider, logger, eventMessagesFactory, entityService.Value));
            var treeService = new Lazy<IApplicationTreeService>(() => new ApplicationTreeService(logger, cache));
            var tagService = new Lazy<ITagService>(() => new TagService(provider, logger, eventMessagesFactory));
            var sectionService = new Lazy<ISectionService>(() => new SectionService(userService.Value, treeService.Value, provider, cache));

            return new ServiceContext(
                migrationEntryService,
                publicAccessService,
                taskService,
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
                externalLoginService);
        }

        public static IDatabaseUnitOfWorkProvider GetDatabaseUnitOfWorkProvider(ILogger logger)
        {
            var adapter = new DefaultScopeContextAdapter();
            var mappingResolver = Mock.Of<IMappingResolver>();
            var databaseFactory = new DefaultDatabaseFactory(GlobalSettings.UmbracoConnectionName, GetDefaultSqlSyntaxProviders(logger), logger, adapter, mappingResolver);
            var repositoryFactory = new RepositoryFactory(Mock.Of<IServiceContainer>());
            return new NPocoUnitOfWorkProvider(databaseFactory, repositoryFactory);
        }
    }
}
