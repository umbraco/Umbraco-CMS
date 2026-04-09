using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.ImportExport;

/// <summary>
///     Service implementation for importing media types from XML files.
/// </summary>
public class MediaTypeImportService : IMediaTypeImportService
{
    private readonly IPackageDataInstallation _packageDataInstallation;
    private readonly IEntityService _entityService;
    private readonly ITemporaryFileToXmlImportService _temporaryFileToXmlImportService;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaTypeImportService"/> class.
    /// </summary>
    /// <param name="packageDataInstallation">The package data installation service for importing media types.</param>
    /// <param name="entityService">The entity service for checking existence of entities.</param>
    /// <param name="temporaryFileToXmlImportService">The service for loading XML from temporary files.</param>
    /// <param name="coreScopeProvider">The core scope provider for creating database scopes.</param>
    /// <param name="userIdKeyResolver">The resolver for converting user keys to user IDs.</param>
    public MediaTypeImportService(
        IPackageDataInstallation packageDataInstallation,
        IEntityService entityService,
        ITemporaryFileToXmlImportService temporaryFileToXmlImportService,
        ICoreScopeProvider coreScopeProvider,
        IUserIdKeyResolver userIdKeyResolver)
    {
        _packageDataInstallation = packageDataInstallation;
        _entityService = entityService;
        _temporaryFileToXmlImportService = temporaryFileToXmlImportService;
        _coreScopeProvider = coreScopeProvider;
        _userIdKeyResolver = userIdKeyResolver;
    }

    /// <inheritdoc />
    public async Task<Attempt<IMediaType?, MediaTypeImportOperationStatus>> Import(
        Guid temporaryFileId,
        Guid userKey,
        Guid? mediaTypeId = null)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        _temporaryFileToXmlImportService.CleanupFileIfScopeCompletes(temporaryFileId);
        Attempt<XElement?, TemporaryFileXmlImportOperationStatus> loadXmlAttempt =
            await _temporaryFileToXmlImportService.LoadXElementFromTemporaryFileAsync(temporaryFileId);
        if (loadXmlAttempt.Success is false)
        {
            return Attempt.FailWithStatus<IMediaType?, MediaTypeImportOperationStatus>(
                loadXmlAttempt.Status is TemporaryFileXmlImportOperationStatus.TemporaryFileNotFound
                    ? MediaTypeImportOperationStatus.TemporaryFileNotFound
                    : MediaTypeImportOperationStatus.TemporaryFileConversionFailure,
                null);
        }

        Attempt<UmbracoEntityTypes> packageEntityTypeAttempt = _temporaryFileToXmlImportService.GetEntityType(loadXmlAttempt.Result!);
        if (packageEntityTypeAttempt.Success is false ||
            packageEntityTypeAttempt.Result is not UmbracoEntityTypes.MediaType)
        {
            return Attempt.FailWithStatus<IMediaType?, MediaTypeImportOperationStatus>(
                MediaTypeImportOperationStatus.TypeMismatch,
                null);
        }

        Guid packageEntityKey = _packageDataInstallation.GetContentTypeKey(loadXmlAttempt.Result!);
        if (mediaTypeId is not null && mediaTypeId.Equals(packageEntityKey) is false)
        {
            return Attempt.FailWithStatus<IMediaType?, MediaTypeImportOperationStatus>(
                MediaTypeImportOperationStatus.IdMismatch,
                null);
        }

        var entityExits = _entityService.Exists(
            _packageDataInstallation.GetContentTypeKey(loadXmlAttempt.Result!),
            UmbracoObjectTypes.MediaType);
        if (entityExits && mediaTypeId is null)
        {
            return Attempt.FailWithStatus<IMediaType?, MediaTypeImportOperationStatus>(
                MediaTypeImportOperationStatus.MediaTypeExists,
                null);
        }

        IReadOnlyList<IMediaType> importResult =
            _packageDataInstallation.ImportMediaTypes(new[] { loadXmlAttempt.Result! }, await _userIdKeyResolver.GetAsync(userKey));

        scope.Complete();

        return Attempt.SucceedWithStatus<IMediaType?, MediaTypeImportOperationStatus>(
            entityExits
                ? MediaTypeImportOperationStatus.SuccessUpdated
                : MediaTypeImportOperationStatus.SuccessCreated,
            importResult[0]);
    }
}
