using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.ImportExport;

public class MediaTypeImportService : TemporaryFileToXmlImportServiceBase, IMediaTypeImportService
{
    private readonly IPackageDataInstallation _packageDataInstallation;
    private readonly IEntityService _entityService;

    public MediaTypeImportService(
        ITemporaryFileService temporaryFileService,
        IPackageDataInstallation packageDataInstallation,
        IEntityService entityService) : base(temporaryFileService)
    {
        _packageDataInstallation = packageDataInstallation;
        _entityService = entityService;
    }

    public async Task<Attempt<IMediaType?, MediaTypeImportOperationStatus>> Import(Guid temporaryFileId, int userId, bool overwrite = false)
    {
        Attempt<XElement?, TemporaryFileOperationStatus> loadXmlAttempt =
            await LoadXElementFromTemporaryFileAsync(temporaryFileId);
        if (loadXmlAttempt.Success is false)
        {
            return Attempt.FailWithStatus<IMediaType?, MediaTypeImportOperationStatus>(
                loadXmlAttempt.Status is TemporaryFileOperationStatus.NotFound
                    ? MediaTypeImportOperationStatus.TemporaryFileNotFound
                    : MediaTypeImportOperationStatus.TemporaryFileConversionFailure,
                null);
        }

        var entityExits = _entityService.Exists(_packageDataInstallation.GetContentTypeKey(loadXmlAttempt.Result!),
            UmbracoObjectTypes.DocumentType);
        if (overwrite is false && entityExits)
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
