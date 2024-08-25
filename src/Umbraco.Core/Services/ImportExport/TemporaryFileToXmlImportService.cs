using System.Xml;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ImportExport;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.ImportExport;

public class TemporaryFileToXmlImportService : ITemporaryFileToXmlImportService
{
    private readonly ITemporaryFileService _temporaryFileService;
    private readonly IPackageDataInstallation _packageDataInstallation;
    private readonly ICoreScopeProvider _coreScopeProvider;

    public TemporaryFileToXmlImportService(
        ITemporaryFileService temporaryFileService,
        IPackageDataInstallation packageDataInstallation,
        ICoreScopeProvider coreScopeProvider)
    {
        _temporaryFileService = temporaryFileService;
        _packageDataInstallation = packageDataInstallation;
        _coreScopeProvider = coreScopeProvider;
    }

    /// <remark>
    /// Only if this method is called within a scope, the temporary file will be cleaned up if that scope completes.
    /// </remark>
    public async Task<Attempt<XElement?, TemporaryFileXmlImportOperationStatus>> LoadXElementFromTemporaryFileAsync(
        Guid temporaryFileId)
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

        return Attempt.SucceedWithStatus<XElement?, TemporaryFileXmlImportOperationStatus>(
            TemporaryFileXmlImportOperationStatus.Success,
            document.Root);
    }

    public void CleanupFileIfScopeCompletes(Guid temporaryFileId)
        => _temporaryFileService.EnlistDeleteIfScopeCompletes(temporaryFileId, _coreScopeProvider);

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

    /// <summary>
    /// Reads the file trough the use of <see cref="LoadXElementFromTemporaryFileAsync"/> and returns basic information regarding the entity that would be imported if this file was processed by
    /// <see cref="IContentTypeImportService.Import(Guid,Guid,System.Nullable{System.Guid})"/> or <see cref="IMediaTypeImportService.Import(Guid,Guid,System.Nullable{System.Guid})"/>.
    /// </summary>
    /// <remarks>As this method does not persist anything, no scope is created and the temporary file is not cleaned up, see remark in <see cref="LoadXElementFromTemporaryFileAsync"/>.</remarks>
    public async Task<Attempt<EntityXmlAnalysis?, TemporaryFileXmlImportOperationStatus>> AnalyzeAsync(
        Guid temporaryFileId)
    {
        Attempt<XElement?, TemporaryFileXmlImportOperationStatus> xmlElementAttempt =
            await LoadXElementFromTemporaryFileAsync(temporaryFileId);

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
