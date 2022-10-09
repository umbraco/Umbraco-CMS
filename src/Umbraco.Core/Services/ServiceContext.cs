namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Represents the Umbraco Service context, which provides access to all services.
/// </summary>
public class ServiceContext
{
    private readonly Lazy<IAuditService>? _auditService;
    private readonly Lazy<IConsentService>? _consentService;
    private readonly Lazy<IContentService>? _contentService;
    private readonly Lazy<IContentTypeBaseServiceProvider>? _contentTypeBaseServiceProvider;
    private readonly Lazy<IContentTypeService>? _contentTypeService;
    private readonly Lazy<IDataTypeService>? _dataTypeService;
    private readonly Lazy<IDomainService>? _domainService;
    private readonly Lazy<IEntityService>? _entityService;
    private readonly Lazy<IExternalLoginService>? _externalLoginService;
    private readonly Lazy<IFileService>? _fileService;
    private readonly Lazy<IKeyValueService>? _keyValueService;
    private readonly Lazy<ILocalizationService>? _localizationService;
    private readonly Lazy<ILocalizedTextService>? _localizedTextService;
    private readonly Lazy<IMacroService>? _macroService;
    private readonly Lazy<IMediaService>? _mediaService;
    private readonly Lazy<IMediaTypeService>? _mediaTypeService;
    private readonly Lazy<IMemberGroupService>? _memberGroupService;
    private readonly Lazy<IMemberService>? _memberService;
    private readonly Lazy<IMemberTypeService>? _memberTypeService;
    private readonly Lazy<INotificationService>? _notificationService;
    private readonly Lazy<IPackagingService>? _packagingService;
    private readonly Lazy<IPublicAccessService>? _publicAccessService;
    private readonly Lazy<IRedirectUrlService>? _redirectUrlService;
    private readonly Lazy<IRelationService>? _relationService;
    private readonly Lazy<IServerRegistrationService>? _serverRegistrationService;
    private readonly Lazy<ITagService>? _tagService;
    private readonly Lazy<IUserService>? _userService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ServiceContext" /> class with lazy services.
    /// </summary>
    public ServiceContext(
        Lazy<IPublicAccessService>? publicAccessService,
        Lazy<IDomainService>? domainService,
        Lazy<IAuditService>? auditService,
        Lazy<ILocalizedTextService>? localizedTextService,
        Lazy<ITagService>? tagService,
        Lazy<IContentService>? contentService,
        Lazy<IUserService>? userService,
        Lazy<IMemberService>? memberService,
        Lazy<IMediaService>? mediaService,
        Lazy<IContentTypeService>? contentTypeService,
        Lazy<IMediaTypeService>? mediaTypeService,
        Lazy<IDataTypeService>? dataTypeService,
        Lazy<IFileService>? fileService,
        Lazy<ILocalizationService>? localizationService,
        Lazy<IPackagingService>? packagingService,
        Lazy<IServerRegistrationService>? serverRegistrationService,
        Lazy<IEntityService>? entityService,
        Lazy<IRelationService>? relationService,
        Lazy<IMacroService>? macroService,
        Lazy<IMemberTypeService>? memberTypeService,
        Lazy<IMemberGroupService>? memberGroupService,
        Lazy<INotificationService>? notificationService,
        Lazy<IExternalLoginService>? externalLoginService,
        Lazy<IRedirectUrlService>? redirectUrlService,
        Lazy<IConsentService>? consentService,
        Lazy<IKeyValueService>? keyValueService,
        Lazy<IContentTypeBaseServiceProvider>? contentTypeBaseServiceProvider)
    {
        _publicAccessService = publicAccessService;
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
        _macroService = macroService;
        _memberTypeService = memberTypeService;
        _memberGroupService = memberGroupService;
        _notificationService = notificationService;
        _externalLoginService = externalLoginService;
        _redirectUrlService = redirectUrlService;
        _consentService = consentService;
        _keyValueService = keyValueService;
        _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
    }

    /// <summary>
    ///     Gets the <see cref="IPublicAccessService" />
    /// </summary>
    public IPublicAccessService? PublicAccessService => _publicAccessService?.Value;

    /// <summary>
    ///     Gets the <see cref="IDomainService" />
    /// </summary>
    public IDomainService? DomainService => _domainService?.Value;

    /// <summary>
    ///     Gets the <see cref="IAuditService" />
    /// </summary>
    public IAuditService? AuditService => _auditService?.Value;

    /// <summary>
    ///     Gets the <see cref="ILocalizedTextService" />
    /// </summary>
    public ILocalizedTextService? TextService => _localizedTextService?.Value;

    /// <summary>
    ///     Gets the <see cref="INotificationService" />
    /// </summary>
    public INotificationService? NotificationService => _notificationService?.Value;

    /// <summary>
    ///     Gets the <see cref="ServerRegistrationService" />
    /// </summary>
    public IServerRegistrationService? ServerRegistrationService => _serverRegistrationService?.Value;

    /// <summary>
    ///     Gets the <see cref="ITagService" />
    /// </summary>
    public ITagService? TagService => _tagService?.Value;

    /// <summary>
    ///     Gets the <see cref="IMacroService" />
    /// </summary>
    public IMacroService? MacroService => _macroService?.Value;

    /// <summary>
    ///     Gets the <see cref="IEntityService" />
    /// </summary>
    public IEntityService? EntityService => _entityService?.Value;

    /// <summary>
    ///     Gets the <see cref="IRelationService" />
    /// </summary>
    public IRelationService? RelationService => _relationService?.Value;

    /// <summary>
    ///     Gets the <see cref="IContentService" />
    /// </summary>
    public IContentService? ContentService => _contentService?.Value;

    /// <summary>
    ///     Gets the <see cref="IContentTypeService" />
    /// </summary>
    public IContentTypeService? ContentTypeService => _contentTypeService?.Value;

    /// <summary>
    ///     Gets the <see cref="IMediaTypeService" />
    /// </summary>
    public IMediaTypeService? MediaTypeService => _mediaTypeService?.Value;

    /// <summary>
    ///     Gets the <see cref="IDataTypeService" />
    /// </summary>
    public IDataTypeService? DataTypeService => _dataTypeService?.Value;

    /// <summary>
    ///     Gets the <see cref="IFileService" />
    /// </summary>
    public IFileService? FileService => _fileService?.Value;

    /// <summary>
    ///     Gets the <see cref="ILocalizationService" />
    /// </summary>
    public ILocalizationService? LocalizationService => _localizationService?.Value;

    /// <summary>
    ///     Gets the <see cref="IMediaService" />
    /// </summary>
    public IMediaService? MediaService => _mediaService?.Value;

    /// <summary>
    ///     Gets the <see cref="PackagingService" />
    /// </summary>
    public IPackagingService? PackagingService => _packagingService?.Value;

    /// <summary>
    ///     Gets the <see cref="UserService" />
    /// </summary>
    public IUserService? UserService => _userService?.Value;

    /// <summary>
    ///     Gets the <see cref="MemberService" />
    /// </summary>
    public IMemberService? MemberService => _memberService?.Value;

    /// <summary>
    ///     Gets the MemberTypeService
    /// </summary>
    public IMemberTypeService? MemberTypeService => _memberTypeService?.Value;

    /// <summary>
    ///     Gets the MemberGroupService
    /// </summary>
    public IMemberGroupService? MemberGroupService => _memberGroupService?.Value;

    /// <summary>
    ///     Gets the ExternalLoginService.
    /// </summary>
    public IExternalLoginService? ExternalLoginService => _externalLoginService?.Value;

    /// <summary>
    ///     Gets the RedirectUrlService.
    /// </summary>
    public IRedirectUrlService? RedirectUrlService => _redirectUrlService?.Value;

    /// <summary>
    ///     Gets the ConsentService.
    /// </summary>
    public IConsentService? ConsentService => _consentService?.Value;

    /// <summary>
    ///     Gets the KeyValueService.
    /// </summary>
    public IKeyValueService? KeyValueService => _keyValueService?.Value;

    /// <summary>
    ///     Gets the ContentTypeServiceBaseFactory.
    /// </summary>
    public IContentTypeBaseServiceProvider? ContentTypeBaseServices => _contentTypeBaseServiceProvider?.Value;

    /// <summary>
    ///     Creates a partial service context with only some services (for tests).
    /// </summary>
    /// <remarks>
    ///     <para>Using a true constructor for this confuses DI containers.</para>
    /// </remarks>
    public static ServiceContext CreatePartial(
        IContentService? contentService = null,
        IMediaService? mediaService = null,
        IContentTypeService? contentTypeService = null,
        IMediaTypeService? mediaTypeService = null,
        IDataTypeService? dataTypeService = null,
        IFileService? fileService = null,
        ILocalizationService? localizationService = null,
        IPackagingService? packagingService = null,
        IEntityService? entityService = null,
        IRelationService? relationService = null,
        IMemberGroupService? memberGroupService = null,
        IMemberTypeService? memberTypeService = null,
        IMemberService? memberService = null,
        IUserService? userService = null,
        ITagService? tagService = null,
        INotificationService? notificationService = null,
        ILocalizedTextService? localizedTextService = null,
        IAuditService? auditService = null,
        IDomainService? domainService = null,
        IMacroService? macroService = null,
        IPublicAccessService? publicAccessService = null,
        IExternalLoginService? externalLoginService = null,
        IServerRegistrationService? serverRegistrationService = null,
        IRedirectUrlService? redirectUrlService = null,
        IConsentService? consentService = null,
        IKeyValueService? keyValueService = null,
        IContentTypeBaseServiceProvider? contentTypeBaseServiceProvider = null)
    {
        Lazy<T>? Lazy<T>(T? service)
        {
            return service == null ? null : new Lazy<T>(() => service);
        }

        return new ServiceContext(
            Lazy(publicAccessService),
            Lazy(domainService),
            Lazy(auditService),
            Lazy(localizedTextService),
            Lazy(tagService),
            Lazy(contentService),
            Lazy(userService),
            Lazy(memberService),
            Lazy(mediaService),
            Lazy(contentTypeService),
            Lazy(mediaTypeService),
            Lazy(dataTypeService),
            Lazy(fileService),
            Lazy(localizationService),
            Lazy(packagingService),
            Lazy(serverRegistrationService),
            Lazy(entityService),
            Lazy(relationService),
            Lazy(macroService),
            Lazy(memberTypeService),
            Lazy(memberGroupService),
            Lazy(notificationService),
            Lazy(externalLoginService),
            Lazy(redirectUrlService),
            Lazy(consentService),
            Lazy(keyValueService),
            Lazy(contentTypeBaseServiceProvider));
    }
}
