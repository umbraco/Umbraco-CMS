using System.Xml;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ImportExport;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.ImportExport;

/// <summary>
///     Service implementation for loading and analyzing XML import data from temporary files.
/// </summary>
/// <remarks>
///     This service provides functionality to convert temporary files containing XML definitions
///     into <see cref="XElement"/> objects for further processing during content type imports.
/// </remarks>
public class TemporaryFileToXmlImportService : ITemporaryFileToXmlImportService
{
    private readonly ITemporaryFileService _temporaryFileService;
    private readonly IPackageDataInstallation _packageDataInstallation;
    private readonly ICoreScopeProvider _coreScopeProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TemporaryFileToXmlImportService"/> class.
    /// </summary>
    /// <param name="temporaryFileService">The service for managing temporary files.</param>
    /// <param name="packageDataInstallation">The package data installation service for extracting entity information from XML.</param>
    /// <param name="coreScopeProvider">The core scope provider for creating database scopes.</param>
    public TemporaryFileToXmlImportService(
        ITemporaryFileService temporaryFileService,
        IPackageDataInstallation packageDataInstallation,
        ICoreScopeProvider coreScopeProvider)
    {
        _temporaryFileService = temporaryFileService;
        _packageDataInstallation = packageDataInstallation;
        _coreScopeProvider = coreScopeProvider;
    }

    /// <summary>
    ///     Loads the XML content from a temporary file and returns it as an <see cref="XElement"/>.
    /// </summary>
    /// <param name="temporaryFileId">The unique identifier of the temporary file to load.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an <see cref="Attempt{TResult, TStatus}"/>
    ///     with the loaded <see cref="XElement"/> on success, or <c>null</c> with an appropriate
    ///     <see cref="TemporaryFileXmlImportOperationStatus"/> on failure.
    /// </returns>
    /// <remarks>
    ///     Only if this method is called within a scope, the temporary file will be cleaned up if that scope completes.
    /// </remarks>
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

    /// <inheritdoc />
    public void CleanupFileIfScopeCompletes(Guid temporaryFileId)
        => _temporaryFileService.EnlistDeleteIfScopeCompletes(temporaryFileId, _coreScopeProvider);

    /// <summary>
    ///     Determines the Umbraco entity type from an XML element.
    /// </summary>
    /// <param name="entityElement">The XML element to analyze.</param>
    /// <returns>
    ///     An <see cref="Attempt{TResult}"/> containing the <see cref="UmbracoEntityTypes"/> value
    ///     on success, or a failed attempt if the entity type could not be determined.
    /// </returns>
    public Attempt<UmbracoEntityTypes> GetEntityType(XElement entityElement)
    {
        var entityType = entityElement.Name.ToString();
        return entityType switch
        {
            IEntityXmlSerializer.DocumentTypeElementName
                => Attempt<UmbracoEntityTypes>.Succeed(UmbracoEntityTypes.DocumentType),
            IEntityXmlSerializer.MediaTypeElementName
                => Attempt<UmbracoEntityTypes>.Succeed(UmbracoEntityTypes.MediaType),
            IEntityXmlSerializer.MemberTypeElementName
                => Attempt<UmbracoEntityTypes>.Succeed(UmbracoEntityTypes.MemberType),
            _ => Attempt<UmbracoEntityTypes>.Fail()
        };
    }

    /// <summary>
    ///     Analyzes a temporary file containing XML import data and returns information about the entity that would be imported.
    /// </summary>
    /// <param name="temporaryFileId">The unique identifier of the temporary file to analyze.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an <see cref="Attempt{TResult, TStatus}"/>
    ///     with an <see cref="EntityXmlAnalysis"/> on success containing the entity type, alias, and key;
    ///     or <c>null</c> with an appropriate <see cref="TemporaryFileXmlImportOperationStatus"/> on failure.
    /// </returns>
    /// <remarks>
    ///     As this method does not persist anything, no scope is created and the temporary file is not cleaned up.
    ///     This method reads the file through the use of <see cref="LoadXElementFromTemporaryFileAsync"/> and returns
    ///     basic information regarding the entity that would be imported if this file was processed by
    ///     <see cref="IContentTypeImportService.Import(Guid, Guid, Guid?)"/> or
    ///     <see cref="IMediaTypeImportService.Import(Guid, Guid, Guid?)"/>.
    /// </remarks>
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
