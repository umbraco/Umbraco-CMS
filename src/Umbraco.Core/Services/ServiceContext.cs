using System;
using log4net;
using Umbraco.Core.Logging;
using System.IO;
using Umbraco.Core.IO;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Publishing;
using umbraco.interfaces;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// The Umbraco ServiceContext, which provides access to the following services:
    /// <see cref="IContentService"/>, <see cref="IContentTypeService"/>, <see cref="IDataTypeService"/>,
    /// <see cref="IFileService"/>, <see cref="ILocalizationService"/> and <see cref="IMediaService"/>.
    /// </summary>
    public class ServiceContext
    {
        private Lazy<IPublicAccessService> _publicAccessService; 
        private Lazy<ITaskService> _taskService; 
        private Lazy<IDomainService> _domainService; 
        private Lazy<IAuditService> _auditService; 
        private Lazy<ILocalizedTextService> _localizedTextService;
        private Lazy<ITagService> _tagService;
        private Lazy<IContentService> _contentService;
        private Lazy<IUserService> _userService;
        private Lazy<IMemberService> _memberService;
        private Lazy<IMediaService> _mediaService;
        private Lazy<IContentTypeService> _contentTypeService;
        private Lazy<IDataTypeService> _dataTypeService;
        private Lazy<IFileService> _fileService;
        private Lazy<ILocalizationService> _localizationService;
        private Lazy<IPackagingService> _packagingService;
        private Lazy<ServerRegistrationService> _serverRegistrationService;
        private Lazy<IEntityService> _entityService;
        private Lazy<IRelationService> _relationService;
        private Lazy<IApplicationTreeService> _treeService;
        private Lazy<ISectionService> _sectionService;
        private Lazy<IMacroService> _macroService;
        private Lazy<IMemberTypeService> _memberTypeService;
        private Lazy<IMemberGroupService> _memberGroupService;
        private Lazy<INotificationService> _notificationService;

        /// <summary>
        /// public ctor - will generally just be used for unit testing all items are optional and if not specified, the defaults will be used
        /// </summary>
        /// <param name="contentService"></param>
        /// <param name="mediaService"></param>
        /// <param name="contentTypeService"></param>
        /// <param name="dataTypeService"></param>
        /// <param name="fileService"></param>
        /// <param name="localizationService"></param>
        /// <param name="packagingService"></param>
        /// <param name="entityService"></param>
        /// <param name="relationService"></param>
        /// <param name="memberGroupService"></param>
        /// <param name="memberTypeService"></param>
        /// <param name="memberService"></param>
        /// <param name="userService"></param>
        /// <param name="sectionService"></param>
        /// <param name="treeService"></param>
        /// <param name="tagService"></param>
        /// <param name="notificationService"></param>
        /// <param name="localizedTextService"></param>
        /// <param name="auditService"></param>
        /// <param name="domainService"></param>
        /// <param name="taskService"></param>
        /// <param name="macroService"></param>
        /// <param name="publicAccessService"></param>
        public ServiceContext(
            IContentService contentService = null,
            IMediaService mediaService = null,
            IContentTypeService contentTypeService = null,
            IDataTypeService dataTypeService = null,
            IFileService fileService = null,
            ILocalizationService localizationService = null,
            IPackagingService packagingService = null,
            IEntityService entityService = null,
            IRelationService relationService = null,
            IMemberGroupService memberGroupService = null,
            IMemberTypeService memberTypeService = null,
            IMemberService memberService = null,
            IUserService userService = null,
            ISectionService sectionService = null,
            IApplicationTreeService treeService = null,
            ITagService tagService = null,
            INotificationService notificationService = null,
            ILocalizedTextService localizedTextService = null,
            IAuditService auditService = null,
            IDomainService domainService = null,
            ITaskService taskService = null,
            IMacroService macroService = null,
            IPublicAccessService publicAccessService = null)
        {
            if (auditService != null) _auditService = new Lazy<IAuditService>(() => auditService);
            if (localizedTextService != null) _localizedTextService = new Lazy<ILocalizedTextService>(() => localizedTextService);
            if (tagService != null) _tagService = new Lazy<ITagService>(() => tagService);
            if (contentService != null) _contentService = new Lazy<IContentService>(() => contentService);
            if (mediaService != null) _mediaService = new Lazy<IMediaService>(() => mediaService);
            if (contentTypeService != null) _contentTypeService = new Lazy<IContentTypeService>(() => contentTypeService);
            if (dataTypeService != null) _dataTypeService = new Lazy<IDataTypeService>(() => dataTypeService);
            if (fileService != null) _fileService = new Lazy<IFileService>(() => fileService);
            if (localizationService != null) _localizationService = new Lazy<ILocalizationService>(() => localizationService);
            if (packagingService != null) _packagingService = new Lazy<IPackagingService>(() => packagingService);
            if (entityService != null) _entityService = new Lazy<IEntityService>(() => entityService);
            if (relationService != null) _relationService = new Lazy<IRelationService>(() => relationService);
            if (sectionService != null) _sectionService = new Lazy<ISectionService>(() => sectionService);
            if (memberGroupService != null) _memberGroupService = new Lazy<IMemberGroupService>(() => memberGroupService);
            if (memberTypeService != null) _memberTypeService = new Lazy<IMemberTypeService>(() => memberTypeService);
            if (treeService != null) _treeService = new Lazy<IApplicationTreeService>(() => treeService);
            if (memberService != null) _memberService = new Lazy<IMemberService>(() => memberService);
            if (userService != null) _userService = new Lazy<IUserService>(() => userService);
            if (notificationService != null) _notificationService = new Lazy<INotificationService>(() => notificationService);
            if (domainService != null) _domainService = new Lazy<IDomainService>(() => domainService);
            if (taskService != null) _taskService = new Lazy<ITaskService>(() => taskService);
            if (macroService != null) _macroService = new Lazy<IMacroService>(() => macroService);
            if (publicAccessService != null) _publicAccessService = new Lazy<IPublicAccessService>(() => publicAccessService);
        }

        internal ServiceContext(
            RepositoryFactory repositoryFactory,
            IDatabaseUnitOfWorkProvider dbUnitOfWorkProvider, 
            IUnitOfWorkProvider fileUnitOfWorkProvider, 
            BasePublishingStrategy publishingStrategy, 
            CacheHelper cache, 
            ILogger logger)
        {
            BuildServiceCache(dbUnitOfWorkProvider, fileUnitOfWorkProvider, publishingStrategy, cache,
                              repositoryFactory,
                              logger);
        }

        /// <summary>
        /// Builds the various services
        /// </summary>
        private void BuildServiceCache(
            IDatabaseUnitOfWorkProvider dbUnitOfWorkProvider,
            IUnitOfWorkProvider fileUnitOfWorkProvider,
            BasePublishingStrategy publishingStrategy,
            CacheHelper cache,
            RepositoryFactory repositoryFactory,
            ILogger logger)
        {
            var provider = dbUnitOfWorkProvider;
            var fileProvider = fileUnitOfWorkProvider;

            if (_publicAccessService == null)
                _publicAccessService = new Lazy<IPublicAccessService>(() => new PublicAccessService(provider, repositoryFactory, logger));

            if (_taskService == null)
                _taskService = new Lazy<ITaskService>(() => new TaskService(provider, repositoryFactory, logger));

            if (_domainService == null)
                _domainService = new Lazy<IDomainService>(() => new DomainService(provider, repositoryFactory, logger));

            if (_auditService == null)
                _auditService = new Lazy<IAuditService>(() => new AuditService(provider, repositoryFactory, logger));

            if (_localizedTextService == null)
                _localizedTextService = new Lazy<ILocalizedTextService>(() => new LocalizedTextService(
                    new LocalizedTextServiceFileSources(cache.RuntimeCache, new DirectoryInfo(IOHelper.MapPath(SystemDirectories.Umbraco + "/config/lang/"))),
                    logger));

            if (_notificationService == null)
                _notificationService = new Lazy<INotificationService>(() => new NotificationService(provider, _userService.Value, _contentService.Value, logger));

            if (_serverRegistrationService == null)
                _serverRegistrationService = new Lazy<ServerRegistrationService>(() => new ServerRegistrationService(provider, repositoryFactory, logger));

            if (_userService == null)
                _userService = new Lazy<IUserService>(() => new UserService(provider, repositoryFactory, logger));

            if (_memberService == null)
                _memberService = new Lazy<IMemberService>(() => new MemberService(provider, repositoryFactory, logger, _memberGroupService.Value, _dataTypeService.Value));

            if (_contentService == null)
                _contentService = new Lazy<IContentService>(() => new ContentService(provider, repositoryFactory, logger, publishingStrategy, _dataTypeService.Value, _userService.Value));

            if (_mediaService == null)
                _mediaService = new Lazy<IMediaService>(() => new MediaService(provider, repositoryFactory, logger, _dataTypeService.Value, _userService.Value));

            if (_contentTypeService == null)
                _contentTypeService = new Lazy<IContentTypeService>(() => new ContentTypeService(provider, repositoryFactory, logger, _contentService.Value, _mediaService.Value));

            if (_dataTypeService == null)
                _dataTypeService = new Lazy<IDataTypeService>(() => new DataTypeService(provider, repositoryFactory, logger));

            if (_fileService == null)
                _fileService = new Lazy<IFileService>(() => new FileService(fileProvider, provider, repositoryFactory));

            if (_localizationService == null)
                _localizationService = new Lazy<ILocalizationService>(() => new LocalizationService(provider, repositoryFactory, logger));

            if (_packagingService == null)
                _packagingService = new Lazy<IPackagingService>(() => new PackagingService(logger, _contentService.Value, _contentTypeService.Value, _mediaService.Value, _macroService.Value, _dataTypeService.Value, _fileService.Value, _localizationService.Value, _userService.Value, repositoryFactory, provider));

            if (_entityService == null)
                _entityService = new Lazy<IEntityService>(() => new EntityService(
                    provider, repositoryFactory, logger, 
                    _contentService.Value, _contentTypeService.Value, _mediaService.Value, _dataTypeService.Value, _memberService.Value, _memberTypeService.Value));

            if (_relationService == null)
                _relationService = new Lazy<IRelationService>(() => new RelationService(provider, repositoryFactory, logger, _entityService.Value));

            if (_treeService == null)
                _treeService = new Lazy<IApplicationTreeService>(() => new ApplicationTreeService(logger, cache));

            if (_sectionService == null)
                _sectionService = new Lazy<ISectionService>(() => new SectionService(_userService.Value, _treeService.Value, provider, cache));

            if (_macroService == null)
                _macroService = new Lazy<IMacroService>(() => new MacroService(provider, repositoryFactory, logger));

            if (_memberTypeService == null)
                _memberTypeService = new Lazy<IMemberTypeService>(() => new MemberTypeService(provider, repositoryFactory, logger, _memberService.Value));

            if (_tagService == null)
                _tagService = new Lazy<ITagService>(() => new TagService(provider, repositoryFactory, logger));

            if (_memberGroupService == null)
                _memberGroupService = new Lazy<IMemberGroupService>(() => new MemberGroupService(provider, repositoryFactory, logger));

        }

        /// <summary>
        /// Gets the <see cref="IPublicAccessService"/>
        /// </summary>
        public IPublicAccessService PublicAccessService
        {
            get { return _publicAccessService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="ITaskService"/>
        /// </summary>
        public ITaskService TaskService
        {
            get { return _taskService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="IDomainService"/>
        /// </summary>
        public IDomainService DomainService
        {
            get { return _domainService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="IAuditService"/>
        /// </summary>
        public IAuditService AuditService
        {
            get { return _auditService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="ILocalizedTextService"/>
        /// </summary>
        public ILocalizedTextService TextService
        {
            get { return _localizedTextService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="INotificationService"/>
        /// </summary>
        public INotificationService NotificationService
        {
            get { return _notificationService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="ServerRegistrationService"/>
        /// </summary>
        public ServerRegistrationService ServerRegistrationService
        {
            get { return _serverRegistrationService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="ITagService"/>
        /// </summary>
        public ITagService TagService
        {
            get { return _tagService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="IMacroService"/>
        /// </summary>
        public IMacroService MacroService
        {
            get { return _macroService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="IEntityService"/>
        /// </summary>
        public IEntityService EntityService
        {
            get { return _entityService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="IRelationService"/>
        /// </summary>
        public IRelationService RelationService
        {
            get { return _relationService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="IContentService"/>
        /// </summary>
        public IContentService ContentService
        {
            get { return _contentService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="IContentTypeService"/>
        /// </summary>
        public IContentTypeService ContentTypeService
        {
            get { return _contentTypeService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="IDataTypeService"/>
        /// </summary>
        public IDataTypeService DataTypeService
        {
            get { return _dataTypeService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="IFileService"/>
        /// </summary>
        public IFileService FileService
        {
            get { return _fileService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="ILocalizationService"/>
        /// </summary>
        public ILocalizationService LocalizationService
        {
            get { return _localizationService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="IMediaService"/>
        /// </summary>
        public IMediaService MediaService
        {
            get { return _mediaService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="PackagingService"/>
        /// </summary>
        public IPackagingService PackagingService
        {
            get { return _packagingService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="UserService"/>
        /// </summary>
        public IUserService UserService
        {
            get { return _userService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="MemberService"/>
        /// </summary>
        public IMemberService MemberService
        {
            get { return _memberService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="SectionService"/>
        /// </summary>
        public ISectionService SectionService
        {
            get { return _sectionService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="ApplicationTreeService"/>
        /// </summary>
        public IApplicationTreeService ApplicationTreeService
        {
            get { return _treeService.Value; }
        }

        /// <summary>
        /// Gets the MemberTypeService
        /// </summary>
        public IMemberTypeService MemberTypeService
        {
            get { return _memberTypeService.Value; }
        }

        /// <summary>
        /// Gets the MemberGroupService
        /// </summary>
        public IMemberGroupService MemberGroupService
        {
            get { return _memberGroupService.Value; }
        }

    }
}