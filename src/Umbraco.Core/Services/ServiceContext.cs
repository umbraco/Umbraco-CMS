using System;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the Umbraco Service context, which provides access to all services.
    /// </summary>
    public class ServiceContext
    {
        private readonly Lazy<IMigrationEntryService> _migrationEntryService; 
        private readonly Lazy<IPublicAccessService> _publicAccessService; 
        private readonly Lazy<ITaskService> _taskService; 
        private readonly Lazy<IDomainService> _domainService; 
        private readonly Lazy<IAuditService> _auditService; 
        private readonly Lazy<ILocalizedTextService> _localizedTextService;
        private readonly Lazy<ITagService> _tagService;
        private readonly Lazy<IContentService> _contentService;
        private readonly Lazy<IUserService> _userService;
        private readonly Lazy<IMemberService> _memberService;
        private readonly Lazy<IMediaService> _mediaService;
        private readonly Lazy<IContentTypeService> _contentTypeService;
        private readonly Lazy<IMediaTypeService> _mediaTypeService;
        private readonly Lazy<IDataTypeService> _dataTypeService;
        private readonly Lazy<IFileService> _fileService;
        private readonly Lazy<ILocalizationService> _localizationService;
        private readonly Lazy<IPackagingService> _packagingService;
        private readonly Lazy<IServerRegistrationService> _serverRegistrationService;
        private readonly Lazy<IEntityService> _entityService;
        private readonly Lazy<IRelationService> _relationService;
        private readonly Lazy<IApplicationTreeService> _treeService;
        private readonly Lazy<ISectionService> _sectionService;
        private readonly Lazy<IMacroService> _macroService;
        private readonly Lazy<IMemberTypeService> _memberTypeService;
        private readonly Lazy<IMemberGroupService> _memberGroupService;
        private readonly Lazy<INotificationService> _notificationService;
        private readonly Lazy<IExternalLoginService> _externalLoginService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceContext"/> class with lazy services.
        /// </summary>
        /// <remarks>Used by IoC. Note that LightInject will favor lazy args when picking a constructor.</remarks>
        public ServiceContext(Lazy<IMigrationEntryService> migrationEntryService, Lazy<IPublicAccessService> publicAccessService, Lazy<ITaskService> taskService, Lazy<IDomainService> domainService, Lazy<IAuditService> auditService, Lazy<ILocalizedTextService> localizedTextService, Lazy<ITagService> tagService, Lazy<IContentService> contentService, Lazy<IUserService> userService, Lazy<IMemberService> memberService, Lazy<IMediaService> mediaService, Lazy<IContentTypeService> contentTypeService, Lazy<IMediaTypeService> mediaTypeService, Lazy<IDataTypeService> dataTypeService, Lazy<IFileService> fileService, Lazy<ILocalizationService> localizationService, Lazy<IPackagingService> packagingService, Lazy<IServerRegistrationService> serverRegistrationService, Lazy<IEntityService> entityService, Lazy<IRelationService> relationService, Lazy<IApplicationTreeService> treeService, Lazy<ISectionService> sectionService, Lazy<IMacroService> macroService, Lazy<IMemberTypeService> memberTypeService, Lazy<IMemberGroupService> memberGroupService, Lazy<INotificationService> notificationService, Lazy<IExternalLoginService> externalLoginService)
        {
            _migrationEntryService = migrationEntryService;
            _publicAccessService = publicAccessService;
            _taskService = taskService;
            _domainService = domainService;
            _auditService = auditService;
            _localizedTextService = localizedTextService;
            _tagService = tagService;
            _contentService = contentService;
            _userService = userService;
            _memberService = memberService;
            _mediaService = mediaService;
            _contentTypeService = contentTypeService;
            _mediaTypeService = mediaTypeService;
            _dataTypeService = dataTypeService;
            _fileService = fileService;
            _localizationService = localizationService;
            _packagingService = packagingService;
            _serverRegistrationService = serverRegistrationService;
            _entityService = entityService;
            _relationService = relationService;
            _treeService = treeService;
            _sectionService = sectionService;
            _macroService = macroService;
            _memberTypeService = memberTypeService;
            _memberGroupService = memberGroupService;
            _notificationService = notificationService;
            _externalLoginService = externalLoginService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceContext"/> class with services.
        /// </summary>
        /// <remarks>Used in tests. All items are optional and remain null if not specified.</remarks>
        public ServiceContext(
            IContentService contentService = null,
            IMediaService mediaService = null,
            IContentTypeService contentTypeService = null,
            IMediaTypeService mediaTypeService = null,
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
            IPublicAccessService publicAccessService = null,
            IExternalLoginService externalLoginService = null,
            IMigrationEntryService migrationEntryService = null,
            IServerRegistrationService serverRegistrationService = null)
        {
            if (serverRegistrationService != null) _serverRegistrationService = new Lazy<IServerRegistrationService>(() => serverRegistrationService);
            if (migrationEntryService != null) _migrationEntryService = new Lazy<IMigrationEntryService>(() => migrationEntryService);
            if (externalLoginService != null) _externalLoginService = new Lazy<IExternalLoginService>(() => externalLoginService);
            if (auditService != null) _auditService = new Lazy<IAuditService>(() => auditService);
            if (localizedTextService != null) _localizedTextService = new Lazy<ILocalizedTextService>(() => localizedTextService);
            if (tagService != null) _tagService = new Lazy<ITagService>(() => tagService);
            if (contentService != null) _contentService = new Lazy<IContentService>(() => contentService);
            if (mediaService != null) _mediaService = new Lazy<IMediaService>(() => mediaService);
            if (contentTypeService != null) _contentTypeService = new Lazy<IContentTypeService>(() => contentTypeService);
            if (mediaTypeService != null) _mediaTypeService = new Lazy<IMediaTypeService>(() => mediaTypeService);
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

        /// <summary>
        /// Gets the <see cref="IMigrationEntryService"/>
        /// </summary>
        public IMigrationEntryService MigrationEntryService => _migrationEntryService.Value;

        /// <summary>
        /// Gets the <see cref="IPublicAccessService"/>
        /// </summary>
        public IPublicAccessService PublicAccessService => _publicAccessService.Value;

        /// <summary>
        /// Gets the <see cref="ITaskService"/>
        /// </summary>
        public ITaskService TaskService => _taskService.Value;

        /// <summary>
        /// Gets the <see cref="IDomainService"/>
        /// </summary>
        public IDomainService DomainService => _domainService.Value;

        /// <summary>
        /// Gets the <see cref="IAuditService"/>
        /// </summary>
        public IAuditService AuditService => _auditService.Value;

        /// <summary>
        /// Gets the <see cref="ILocalizedTextService"/>
        /// </summary>
        public ILocalizedTextService TextService => _localizedTextService.Value;

        /// <summary>
        /// Gets the <see cref="INotificationService"/>
        /// </summary>
        public INotificationService NotificationService => _notificationService.Value;

        /// <summary>
        /// Gets the <see cref="ServerRegistrationService"/>
        /// </summary>
        public IServerRegistrationService ServerRegistrationService => _serverRegistrationService.Value;

        /// <summary>
        /// Gets the <see cref="ITagService"/>
        /// </summary>
        public ITagService TagService => _tagService.Value;

        /// <summary>
        /// Gets the <see cref="IMacroService"/>
        /// </summary>
        public IMacroService MacroService => _macroService.Value;

        /// <summary>
        /// Gets the <see cref="IEntityService"/>
        /// </summary>
        public IEntityService EntityService => _entityService.Value;

        /// <summary>
        /// Gets the <see cref="IRelationService"/>
        /// </summary>
        public IRelationService RelationService => _relationService.Value;

        /// <summary>
        /// Gets the <see cref="IContentService"/>
        /// </summary>
        public IContentService ContentService => _contentService.Value;

        /// <summary>
        /// Gets the <see cref="IContentTypeService"/>
        /// </summary>
        public IContentTypeService ContentTypeService => _contentTypeService.Value;

        /// <summary>
        /// Gets the <see cref="IMediaTypeService"/>
        /// </summary>
        public IMediaTypeService MediaTypeService => _mediaTypeService.Value;

        /// <summary>
        /// Gets the <see cref="IDataTypeService"/>
        /// </summary>
        public IDataTypeService DataTypeService => _dataTypeService.Value;

        /// <summary>
        /// Gets the <see cref="IFileService"/>
        /// </summary>
        public IFileService FileService => _fileService.Value;

        /// <summary>
        /// Gets the <see cref="ILocalizationService"/>
        /// </summary>
        public ILocalizationService LocalizationService => _localizationService.Value;

        /// <summary>
        /// Gets the <see cref="IMediaService"/>
        /// </summary>
        public IMediaService MediaService => _mediaService.Value;

        /// <summary>
        /// Gets the <see cref="PackagingService"/>
        /// </summary>
        public IPackagingService PackagingService => _packagingService.Value;

        /// <summary>
        /// Gets the <see cref="UserService"/>
        /// </summary>
        public IUserService UserService => _userService.Value;

        /// <summary>
        /// Gets the <see cref="MemberService"/>
        /// </summary>
        public IMemberService MemberService => _memberService.Value;

        /// <summary>
        /// Gets the <see cref="SectionService"/>
        /// </summary>
        public ISectionService SectionService => _sectionService.Value;

        /// <summary>
        /// Gets the <see cref="ApplicationTreeService"/>
        /// </summary>
        public IApplicationTreeService ApplicationTreeService => _treeService.Value;

        /// <summary>
        /// Gets the MemberTypeService
        /// </summary>
        public IMemberTypeService MemberTypeService => _memberTypeService.Value;

        /// <summary>
        /// Gets the MemberGroupService
        /// </summary>
        public IMemberGroupService MemberGroupService => _memberGroupService.Value;

        public IExternalLoginService ExternalLoginService => _externalLoginService.Value;
    }
}