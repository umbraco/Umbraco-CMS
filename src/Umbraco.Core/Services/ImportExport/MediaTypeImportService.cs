using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.ImportExport;

public class MediaTypeImportService : IMediaTypeImportService
{
    private readonly IPackageDataInstallation _packageDataInstallation;
    private readonly IEntityService _entityService;
    private readonly ITemporaryFileToXmlImportService _temporaryFileToXmlImportService;
    private readonly ICoreScopeProvider _coreScopeProvider;

    public MediaTypeImportService(
        IPackageDataInstallation packageDataInstallation,
        IEntityService entityService,
        ITemporaryFileToXmlImportService temporaryFileToXmlImportService,
        ICoreScopeProvider coreScopeProvider)
    {
        _packageDataInstallation = packageDataInstallation;
        _entityService = entityService;
        _temporaryFileToXmlImportService = temporaryFileToXmlImportService;
        _coreScopeProvider = coreScopeProvider;
    }

    public async Task<Attempt<IMediaType?, MediaTypeImportOperationStatus>> Import(
        Guid temporaryFileId,
        int userId,
        Guid? mediaTypeId = null)
    {
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
                MediaTypeImportOperationStatus.TypeMisMatch,
                null);
        }

        using ICoreScope scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);

        Guid packageEntityKey = _packageDataInstallation.GetContentTypeKey(loadXmlAttempt.Result!);
        if (mediaTypeId is not null && mediaTypeId.Equals(packageEntityKey) is false)
        {
            return Attempt.FailWithStatus<IMediaType?, MediaTypeImportOperationStatus>(
                MediaTypeImportOperationStatus.IdMismatch,
                null);
        }

        var entityExits = _entityService.Exists(
            _packageDataInstallation.GetContentTypeKey(loadXmlAttempt.Result!),
            UmbracoObjectTypes.DocumentType);
        if (entityExits && mediaTypeId is null)
        {
            return Attempt.FailWithStatus<IMediaType?, MediaTypeImportOperationStatus>(
                MediaTypeImportOperationStatus.MediaTypeExists,
                null);
        }

        IReadOnlyList<IMediaType> importResult =
            _packageDataInstallation.ImportMediaTypes(new[] { loadXmlAttempt.Result! }, userId);

        return Attempt.SucceedWithStatus<IMediaType?, MediaTypeImportOperationStatus>(
            entityExits
                ? MediaTypeImportOperationStatus.SuccessUpdated
                : MediaTypeImportOperationStatus.SuccessCreated,
            importResult[0]);
    }
}
