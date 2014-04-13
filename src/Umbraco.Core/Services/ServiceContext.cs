using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Publishing;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// The Umbraco ServiceContext, which provides access to the following services:
    /// <see cref="IContentService"/>, <see cref="IContentTypeService"/>, <see cref="IDataTypeService"/>,
    /// <see cref="IFileService"/>, <see cref="ILocalizationService"/> and <see cref="IMediaService"/>.
    /// </summary>
    public class ServiceContext
    {
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
        /// public ctor - will generally just be used for unit testing
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
        /// <param name="sectionService"></param>
        /// <param name="treeService"></param>
        /// <param name="tagService"></param>
        /// <param name="memberGroupService"></param>
        public ServiceContext(
            IContentService contentService, 
            IMediaService mediaService, 
            IContentTypeService contentTypeService, 
            IDataTypeService dataTypeService, 
            IFileService fileService, 
            ILocalizationService localizationService, 
            IPackagingService packagingService, 
            IEntityService entityService,
            IRelationService relationService,
            IMemberGroupService memberGroupService,
            ISectionService sectionService,
            IApplicationTreeService treeService,
            ITagService tagService)
        {
            _tagService = new Lazy<ITagService>(() => tagService);     
            _contentService = new Lazy<IContentService>(() => contentService);        
            _mediaService = new Lazy<IMediaService>(() => mediaService);
            _contentTypeService = new Lazy<IContentTypeService>(() => contentTypeService);
            _dataTypeService = new Lazy<IDataTypeService>(() => dataTypeService);
            _fileService = new Lazy<IFileService>(() => fileService);
            _localizationService = new Lazy<ILocalizationService>(() => localizationService);
            _packagingService = new Lazy<IPackagingService>(() => packagingService);
            _entityService = new Lazy<IEntityService>(() => entityService);
            _relationService = new Lazy<IRelationService>(() => relationService);
            _sectionService = new Lazy<ISectionService>(() => sectionService);
            _memberGroupService = new Lazy<IMemberGroupService>(() => memberGroupService);
            _treeService = new Lazy<IApplicationTreeService>(() => treeService);
        }

        /// <summary>
        /// Constructor used to instantiate the core services
        /// </summary>
        /// <param name="dbUnitOfWorkProvider"></param>
        /// <param name="fileUnitOfWorkProvider"></param>
        /// <param name="publishingStrategy"></param>
        /// <param name="cache"></param>
        internal ServiceContext(IDatabaseUnitOfWorkProvider dbUnitOfWorkProvider, IUnitOfWorkProvider fileUnitOfWorkProvider, BasePublishingStrategy publishingStrategy, CacheHelper cache)
        {
			BuildServiceCache(dbUnitOfWorkProvider, fileUnitOfWorkProvider, publishingStrategy, cache,
                              //this needs to be lazy because when we create the service context it's generally before the
                              //resolvers have been initialized!
                              new Lazy<RepositoryFactory>(() => RepositoryResolver.Current.Factory));
        }

        /// <summary>
        /// Builds the various services
        /// </summary>
        private void BuildServiceCache(
            IDatabaseUnitOfWorkProvider dbUnitOfWorkProvider,
            IUnitOfWorkProvider fileUnitOfWorkProvider,
            BasePublishingStrategy publishingStrategy,
            CacheHelper cache,
            Lazy<RepositoryFactory> repositoryFactory)
        {
            var provider = dbUnitOfWorkProvider;
            var fileProvider = fileUnitOfWorkProvider;

            if (_notificationService == null)
                _notificationService = new Lazy<INotificationService>(() => new NotificationService(provider, _userService.Value, _contentService.Value));

            if (_serverRegistrationService == null)
                _serverRegistrationService = new Lazy<ServerRegistrationService>(() => new ServerRegistrationService(provider, repositoryFactory.Value));

            if (_userService == null)
                _userService = new Lazy<IUserService>(() => new UserService(provider, repositoryFactory.Value));

            if (_memberService == null)
                _memberService = new Lazy<IMemberService>(() => new MemberService(provider, repositoryFactory.Value, _memberGroupService.Value));

            if (_contentService == null)
                _contentService = new Lazy<IContentService>(() => new ContentService(provider, repositoryFactory.Value, publishingStrategy));

            if (_mediaService == null)
                _mediaService = new Lazy<IMediaService>(() => new MediaService(provider, repositoryFactory.Value));

            if (_contentTypeService == null)
                _contentTypeService = new Lazy<IContentTypeService>(() => new ContentTypeService(provider, repositoryFactory.Value, _contentService.Value, _mediaService.Value));

            if (_dataTypeService == null)
                _dataTypeService = new Lazy<IDataTypeService>(() => new DataTypeService(provider, repositoryFactory.Value));

            if (_fileService == null)
                _fileService = new Lazy<IFileService>(() => new FileService(fileProvider, provider, repositoryFactory.Value));

            if (_localizationService == null)
                _localizationService = new Lazy<ILocalizationService>(() => new LocalizationService(provider, repositoryFactory.Value));

            if (_packagingService == null)
                _packagingService = new Lazy<IPackagingService>(() => new PackagingService(_contentService.Value, _contentTypeService.Value, _mediaService.Value, _macroService.Value, _dataTypeService.Value, _fileService.Value, _localizationService.Value, repositoryFactory.Value, provider));

            if (_entityService == null)
                _entityService = new Lazy<IEntityService>(() => new EntityService(provider, repositoryFactory.Value, _contentService.Value, _contentTypeService.Value, _mediaService.Value, _dataTypeService.Value));

            if (_relationService == null)
                _relationService = new Lazy<IRelationService>(() => new RelationService(provider, repositoryFactory.Value, _entityService.Value));

            if (_treeService == null)
                _treeService = new Lazy<IApplicationTreeService>(() => new ApplicationTreeService(cache));

            if (_sectionService == null)
                _sectionService = new Lazy<ISectionService>(() => new SectionService(_userService.Value, _treeService.Value, cache));

            if (_macroService == null)
                _macroService = new Lazy<IMacroService>(() => new MacroService(provider, repositoryFactory.Value));

            if (_memberTypeService == null)
                _memberTypeService = new Lazy<IMemberTypeService>(() => new MemberTypeService(provider, repositoryFactory.Value, _memberService.Value));

            if (_tagService == null)
                _tagService = new Lazy<ITagService>(() => new TagService(provider, repositoryFactory.Value));
            if (_memberGroupService == null)
                _memberGroupService = new Lazy<IMemberGroupService>(() => new MemberGroupService(provider, repositoryFactory.Value));
            
        }

        /// <summary>
        /// Gets the <see cref="INotificationService"/>
        /// </summary>
        internal INotificationService NotificationService
        {
            get { return _notificationService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="ServerRegistrationService"/>
        /// </summary>
        internal ServerRegistrationService ServerRegistrationService
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