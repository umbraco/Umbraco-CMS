using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.ImportExport;

public class ContentTypeImportService : TemporaryFileToXmlImportServiceBase, IContentTypeImportService
{
    private readonly IPackageDataInstallation _packageDataInstallation;
    private readonly IEntityService _entityService;

    public ContentTypeImportService(
        ITemporaryFileService temporaryFileService,
        IPackageDataInstallation packageDataInstallation,
        IEntityService entityService) : base(temporaryFileService)
    {
        _packageDataInstallation = packageDataInstallation;
        _entityService = entityService;
    }

    public async Task<Attempt<IContentType?, ContentTypeImportOperationStatus>> Import(Guid temporaryFileId, int userId,
        bool overwrite = false)
    {
        Attempt<XElement?, TemporaryFileOperationStatus> loadXmlAttempt =
            await LoadXElementFromTemporaryFileAsync(temporaryFileId);
        if (loadXmlAttempt.Success is false)
        {
            return Attempt.FailWithStatus<IContentType?, ContentTypeImportOperationStatus>(
                loadXmlAttempt.Status is TemporaryFileOperationStatus.NotFound
                    ? ContentTypeImportOperationStatus.TemporaryFileNotFound
                    : ContentTypeImportOperationStatus.TemporaryFileConversionFailure,
                null);
        }

        var entityExits = _entityService.Exists(_packageDataInstallation.GetContentTypeKey(loadXmlAttempt.Result!),
            UmbracoObjectTypes.DocumentType);
        if (overwrite is false && entityExits)
        {
            return Attempt.FailWithStatus<IContentType?, ContentTypeImportOperationStatus>(
                ContentTypeImportOperationStatus.DocumentTypeExists,
                null);
        }

        IReadOnlyList<IContentType> importResult =
            _packageDataInstallation.ImportDocumentType(loadXmlAttempt.Result!, userId);

        return Attempt.SucceedWithStatus<IContentType?, ContentTypeImportOperationStatus>(
            entityExits
                ? ContentTypeImportOperationStatus.SuccessUpdated
                : ContentTypeImportOperationStatus.SuccessCreated,
            importResult[0]);
    }
}
