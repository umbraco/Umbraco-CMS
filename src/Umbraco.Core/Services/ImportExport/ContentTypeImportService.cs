using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.ImportExport;

public class ContentTypeImportService : IContentTypeImportService
{
    private readonly IPackageDataInstallation _packageDataInstallation;
    private readonly IEntityService _entityService;
    private readonly ITemporaryFileToXmlImportService _temporaryFileToXmlImportService;
    private readonly ICoreScopeProvider _coreScopeProvider;

    public ContentTypeImportService(
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

    /// <summary>
    /// Imports the contentType
    /// </summary>
    /// <param name="temporaryFileId"></param>
    /// <param name="userId"></param>
    /// <param name="contentTypeId">the id of the contentType to overwrite, null if a new contentType should be created</param>
    /// <returns></returns>
    public async Task<Attempt<IContentType?, ContentTypeImportOperationStatus>> Import(
        Guid temporaryFileId,
        int userId,
        Guid? contentTypeId = null)
    {
        Attempt<XElement?, TemporaryFileXmlImportOperationStatus> loadXmlAttempt =
            await _temporaryFileToXmlImportService.LoadXElementFromTemporaryFileAsync(temporaryFileId);
        if (loadXmlAttempt.Success is false)
        {
            return Attempt.FailWithStatus<IContentType?, ContentTypeImportOperationStatus>(
                loadXmlAttempt.Status is TemporaryFileXmlImportOperationStatus.TemporaryFileNotFound
                    ? ContentTypeImportOperationStatus.TemporaryFileNotFound
                    : ContentTypeImportOperationStatus.TemporaryFileConversionFailure,
                null);
        }

        Attempt<UmbracoEntityTypes> packageEntityTypeAttempt = _temporaryFileToXmlImportService.GetEntityType(loadXmlAttempt.Result!);
        if (packageEntityTypeAttempt.Success is false ||
            packageEntityTypeAttempt.Result is not UmbracoEntityTypes.DocumentType)
        {
            return Attempt.FailWithStatus<IContentType?, ContentTypeImportOperationStatus>(
                ContentTypeImportOperationStatus.TypeMismatch,
                null);
        }

        Guid packageEntityKey = _packageDataInstallation.GetContentTypeKey(loadXmlAttempt.Result!);
        if (contentTypeId is not null && contentTypeId.Equals(packageEntityKey) is false)
        {
            return Attempt.FailWithStatus<IContentType?, ContentTypeImportOperationStatus>(
                ContentTypeImportOperationStatus.IdMismatch,
                null);
        }

        using ICoreScope scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);

        var entityExits = _entityService.Exists(packageEntityKey, UmbracoObjectTypes.DocumentType);
        if (entityExits && contentTypeId is null)
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
