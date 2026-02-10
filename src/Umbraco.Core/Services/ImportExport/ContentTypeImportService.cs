using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.ImportExport;

/// <summary>
///     Service implementation for importing content types (document types) from XML files.
/// </summary>
public class ContentTypeImportService : IContentTypeImportService
{
    private readonly IPackageDataInstallation _packageDataInstallation;
    private readonly IEntityService _entityService;
    private readonly ITemporaryFileToXmlImportService _temporaryFileToXmlImportService;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeImportService"/> class.
    /// </summary>
    /// <param name="packageDataInstallation">The package data installation service for importing document types.</param>
    /// <param name="entityService">The entity service for checking existence of entities.</param>
    /// <param name="temporaryFileToXmlImportService">The service for loading XML from temporary files.</param>
    /// <param name="coreScopeProvider">The core scope provider for creating database scopes.</param>
    /// <param name="userIdKeyResolver">The resolver for converting user keys to user IDs.</param>
    public ContentTypeImportService(
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

    /// <summary>
    ///     Imports a content type from a temporary file containing XML data.
    /// </summary>
    /// <param name="temporaryFileId">The unique identifier of the temporary file containing the content type XML definition.</param>
    /// <param name="userKey">The unique key of the user performing the import operation.</param>
    /// <param name="contentTypeId">
    ///     Optional. The unique identifier of an existing content type to overwrite.
    ///     When <c>null</c>, a new content type will be created.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an <see cref="Attempt{TResult, TStatus}"/>
    ///     with the imported <see cref="IContentType"/> on success, or <c>null</c> with an appropriate
    ///     <see cref="ContentTypeImportOperationStatus"/> on failure.
    /// </returns>
    public async Task<Attempt<IContentType?, ContentTypeImportOperationStatus>> Import(
        Guid temporaryFileId,
        Guid userKey,
        Guid? contentTypeId = null)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        _temporaryFileToXmlImportService.CleanupFileIfScopeCompletes(temporaryFileId);
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

        var entityExits = _entityService.Exists(packageEntityKey, UmbracoObjectTypes.DocumentType);
        if (entityExits && contentTypeId is null)
        {
            return Attempt.FailWithStatus<IContentType?, ContentTypeImportOperationStatus>(
                ContentTypeImportOperationStatus.DocumentTypeExists,
                null);
        }

        IReadOnlyList<IContentType> importResult =
            _packageDataInstallation.ImportDocumentType(loadXmlAttempt.Result!, await _userIdKeyResolver.GetAsync(userKey));

        scope.Complete();

        return Attempt.SucceedWithStatus<IContentType?, ContentTypeImportOperationStatus>(
            entityExits
                ? ContentTypeImportOperationStatus.SuccessUpdated
                : ContentTypeImportOperationStatus.SuccessCreated,
            importResult[0]);
    }
}
