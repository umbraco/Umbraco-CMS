using System.Xml;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ImportExport;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.ImportExport;

public class TemporaryFileToXmlImportService : ITemporaryFileToXmlImportService
{
    private readonly ITemporaryFileService _temporaryFileService;
    private readonly IPackageDataInstallation _packageDataInstallation;

    public TemporaryFileToXmlImportService(
        ITemporaryFileService temporaryFileService,
        IPackageDataInstallation packageDataInstallation)
    {
        _temporaryFileService = temporaryFileService;
        _packageDataInstallation = packageDataInstallation;
    }

    public async Task<Attempt<XElement?, TemporaryFileXmlImportOperationStatus>> LoadXElementFromTemporaryFileAsync(
        Guid temporaryFileId, bool cleanupFile = true)
    {
        TemporaryFileModel? documentTypeFile = await _temporaryFileService.GetAsync(temporaryFileId);
        if (documentTypeFile is null)
        {
            return Attempt.FailWithStatus<XElement?, TemporaryFileXmlImportOperationStatus>(
                TemporaryFileXmlImportOperationStatus.TemporaryFileNotFound, null);
        }

        XDocument document;
        await using (Stream fileStream = documentTypeFile.OpenReadStream())
        {
            document = await XDocument.LoadAsync(fileStream, LoadOptions.None, CancellationToken.None);
        }

        if (cleanupFile)
        {
            await _temporaryFileService.DeleteAsync(documentTypeFile.Key);
        }

        return Attempt.SucceedWithStatus<XElement?, TemporaryFileXmlImportOperationStatus>(
            TemporaryFileXmlImportOperationStatus.Success,
            document.Root);
    }

    public Attempt<UmbracoEntityTypes> GetEntityType(XElement entityElement)
    {
        var entityType = entityElement.Name.ToString();
        return entityType switch
        {
            IEntityXmlSerializer.DocumentTypeElementName
                => Attempt<UmbracoEntityTypes>.Succeed(UmbracoEntityTypes.DocumentType),
            IEntityXmlSerializer.MediaTypeElementName
                => Attempt<UmbracoEntityTypes>.Succeed(UmbracoEntityTypes.MediaType),
            _ => Attempt<UmbracoEntityTypes>.Fail()
        };
    }

    public async Task<Attempt<EntityXmlAnalysis?, TemporaryFileXmlImportOperationStatus>> AnalyzeAsync(
        Guid temporaryFileId)
    {
        Attempt<XElement?, TemporaryFileXmlImportOperationStatus> xmlElementAttempt =
            await LoadXElementFromTemporaryFileAsync(temporaryFileId, false);

        if (xmlElementAttempt.Success is false)
        {
            return Attempt<EntityXmlAnalysis, TemporaryFileXmlImportOperationStatus>.Fail(xmlElementAttempt.Status);
        }

        Attempt<UmbracoEntityTypes> entityTypeAttempt = GetEntityType(xmlElementAttempt.Result!);
        if (entityTypeAttempt.Success is false)
        {
            return Attempt<EntityXmlAnalysis, TemporaryFileXmlImportOperationStatus>.Fail(
                TemporaryFileXmlImportOperationStatus.UndeterminedEntityType);
        }

        Guid entityTypeKey = _packageDataInstallation.GetContentTypeKey(xmlElementAttempt.Result!);
        var entityTypeAlias = _packageDataInstallation.GetEntityTypeAlias(xmlElementAttempt.Result!);
        return Attempt<EntityXmlAnalysis?, TemporaryFileXmlImportOperationStatus>.Succeed(
            TemporaryFileXmlImportOperationStatus.Success,
            new EntityXmlAnalysis
            {
                EntityType = entityTypeAttempt.Result, Alias = entityTypeAlias, Key = entityTypeKey,
            });
    }
}
