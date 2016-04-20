using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Web.Services;

namespace Umbraco.Tests.TestHelpers
{
    class ServiceContextHelper
    {
        public static ServiceContext GetServiceContext(RepositoryFactory repositoryFactory,
            IDatabaseUnitOfWorkProvider dbUnitOfWorkProvider,
            IUnitOfWorkProvider fileUnitOfWorkProvider,
            IPublishingStrategy publishingStrategy,
            CacheHelper cache,
            ILogger logger,
            IEventMessagesFactory eventMessagesFactory,
            IEnumerable<IUrlSegmentProvider> urlSegmentProviders)
        {
            if (repositoryFactory == null) throw new ArgumentNullException(nameof(repositoryFactory));
            if (dbUnitOfWorkProvider == null) throw new ArgumentNullException(nameof(dbUnitOfWorkProvider));
            if (fileUnitOfWorkProvider == null) throw new ArgumentNullException(nameof(fileUnitOfWorkProvider));
            if (publishingStrategy == null) throw new ArgumentNullException(nameof(publishingStrategy));
            if (cache == null) throw new ArgumentNullException(nameof(cache));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (eventMessagesFactory == null) throw new ArgumentNullException(nameof(eventMessagesFactory));

            var provider = dbUnitOfWorkProvider;
            var fileProvider = fileUnitOfWorkProvider;

            var migrationEntryService = new Lazy<IMigrationEntryService>(() => new MigrationEntryService(provider, repositoryFactory, logger, eventMessagesFactory));
            var externalLoginService = new Lazy<IExternalLoginService>(() => new ExternalLoginService(provider, repositoryFactory, logger, eventMessagesFactory));
            var publicAccessService = new Lazy<IPublicAccessService>(() => new PublicAccessService(provider, repositoryFactory, logger, eventMessagesFactory));
            var taskService = new Lazy<ITaskService>(() => new TaskService(provider, repositoryFactory, logger, eventMessagesFactory));
            var domainService = new Lazy<IDomainService>(() => new DomainService(provider, repositoryFactory, logger, eventMessagesFactory));
            var auditService = new Lazy<IAuditService>(() => new AuditService(provider, repositoryFactory, logger, eventMessagesFactory));

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

            var userService = new Lazy<IUserService>(() => new UserService(provider, repositoryFactory, logger, eventMessagesFactory));
            var dataTypeService = new Lazy<IDataTypeService>(() => new DataTypeService(provider, repositoryFactory, logger, eventMessagesFactory));
            var contentService = new Lazy<IContentService>(() => new ContentService(provider, repositoryFactory, logger, eventMessagesFactory, publishingStrategy, dataTypeService.Value, userService.Value, urlSegmentProviders));
            var notificationService = new Lazy<INotificationService>(() => new NotificationService(provider, userService.Value, contentService.Value, repositoryFactory, logger));
            var serverRegistrationService = new Lazy<IServerRegistrationService>(() => new ServerRegistrationService(provider, repositoryFactory, logger, eventMessagesFactory));
            var memberGroupService = new Lazy<IMemberGroupService>(() => new MemberGroupService(provider, repositoryFactory, logger, eventMessagesFactory));
            var memberService = new Lazy<IMemberService>(() => new MemberService(provider, repositoryFactory, logger, eventMessagesFactory, memberGroupService.Value, dataTypeService.Value));
            var mediaService = new Lazy<IMediaService>(() => new MediaService(provider, repositoryFactory, logger, eventMessagesFactory, dataTypeService.Value, userService.Value, urlSegmentProviders));
            var contentTypeService = new Lazy<IContentTypeService>(() => new ContentTypeService(provider, repositoryFactory, logger, eventMessagesFactory, contentService.Value, mediaService.Value));
            var fileService = new Lazy<IFileService>(() => new FileService(fileProvider, provider, repositoryFactory, logger, eventMessagesFactory));
            var localizationService = new Lazy<ILocalizationService>(() => new LocalizationService(provider, repositoryFactory, logger, eventMessagesFactory));

            var memberTypeService = new Lazy<IMemberTypeService>(() => new MemberTypeService(provider, repositoryFactory, logger, eventMessagesFactory, memberService.Value));
            var entityService = new Lazy<IEntityService>(() => new EntityService(
                    provider, repositoryFactory, logger, eventMessagesFactory,
                    contentService.Value, contentTypeService.Value, mediaService.Value, dataTypeService.Value, memberService.Value, memberTypeService.Value,
                    //TODO: Consider making this an isolated cache instead of using the global one
                    cache.RuntimeCache));

            var macroService = new Lazy<IMacroService>(() => new MacroService(provider, repositoryFactory, logger, eventMessagesFactory));
            var packagingService = new Lazy<IPackagingService>(() => new PackagingService(logger, contentService.Value, contentTypeService.Value, mediaService.Value, macroService.Value, dataTypeService.Value, fileService.Value, localizationService.Value, entityService.Value, userService.Value, repositoryFactory, provider, urlSegmentProviders));
            var relationService = new Lazy<IRelationService>(() => new RelationService(provider, repositoryFactory, logger, eventMessagesFactory, entityService.Value));
            var treeService = new Lazy<IApplicationTreeService>(() => new ApplicationTreeService(logger, cache));
            var tagService = new Lazy<ITagService>(() => new TagService(provider, repositoryFactory, logger, eventMessagesFactory));
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
    }
}
