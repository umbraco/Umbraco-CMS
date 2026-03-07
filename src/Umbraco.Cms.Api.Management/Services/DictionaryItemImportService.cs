using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Api.Management.Services.OperationStatus;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Packaging;
using Umbraco.Extensions;
using File = System.IO.File;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;

namespace Umbraco.Cms.Api.Management.Services;

internal sealed class DictionaryItemImportService : IDictionaryItemImportService
{
    private readonly IDictionaryItemService _dictionaryItemService;
    private readonly IPackageDataInstallation _packageDataInstallation;
    private readonly ILogger<DictionaryItemImportService> _logger;
    private readonly ITemporaryFileService _temporaryFileService;
    private readonly IUserService _userService;
    private readonly IScopeProvider _scopeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Services.DictionaryItemImportService"/> class,
    /// which is responsible for importing dictionary items into the Umbraco CMS.
    /// </summary>
    /// <param name="dictionaryItemService">Service for managing dictionary items.</param>
    /// <param name="packageDataInstallation">Service for handling package data installation.</param>
    /// <param name="logger">The logger used for logging import operations.</param>
    /// <param name="temporaryFileService">Service for handling temporary files during import.</param>
    /// <param name="userService">Service for managing user information and permissions.</param>
    /// <param name="scopeProvider">Provider for managing database transaction scopes.</param>
    public DictionaryItemImportService(
        IDictionaryItemService dictionaryItemService,
        IPackageDataInstallation packageDataInstallation,
        ILogger<DictionaryItemImportService> logger,
        ITemporaryFileService temporaryFileService,
        IUserService userService,
        IScopeProvider scopeProvider)
    {
        _dictionaryItemService = dictionaryItemService;
        _packageDataInstallation = packageDataInstallation;
        _logger = logger;
        _temporaryFileService = temporaryFileService;
        _userService = userService;
        _scopeProvider = scopeProvider;
    }

    /// <summary>
    /// Asynchronously imports a dictionary item from a temporary UDT (Umbraco Dictionary Translation) file.
    /// </summary>
    /// <param name="fileKey">The unique identifier of the temporary UDT file to import.</param>
    /// <param name="parentKey">The optional unique identifier of the parent dictionary item, if any.</param>
    /// <param name="userKey">The unique identifier of the user performing the import.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The result is an <see cref="Attempt{IDictionaryItem?, DictionaryImportOperationStatus}"/>,
    /// which contains the imported dictionary item (if successful) and the status of the import operation.
    /// </returns>

    public async Task<Attempt<IDictionaryItem?, DictionaryImportOperationStatus>> ImportDictionaryItemFromUdtFileAsync(Guid fileKey, Guid? parentKey, Guid userKey)
    {
        using var scope = _scopeProvider.CreateScope();
        _temporaryFileService.EnlistDeleteIfScopeCompletes(fileKey, _scopeProvider);

        TemporaryFileModel? temporaryFile = await _temporaryFileService.GetAsync(fileKey);

        if (temporaryFile is null)
        {
            return Attempt.FailWithStatus<IDictionaryItem?, DictionaryImportOperationStatus>(DictionaryImportOperationStatus.TemporaryFileNotFound, null);
        }



        if (".udt".InvariantEquals(Path.GetExtension(temporaryFile.FileName)) == false)
        {
            return Attempt.FailWithStatus<IDictionaryItem?, DictionaryImportOperationStatus>(DictionaryImportOperationStatus.InvalidFileType, null);
        }

        if (parentKey.HasValue && await _dictionaryItemService.GetAsync(parentKey.Value) == null)
        {
            return Attempt.FailWithStatus<IDictionaryItem?, DictionaryImportOperationStatus>(DictionaryImportOperationStatus.ParentNotFound, null);
        }

        // load the UDT file from disk
        (XDocument Document, DictionaryImportOperationStatus Status) loadResult = await LoadUdtFileAsync(temporaryFile);
        if (loadResult.Status != DictionaryImportOperationStatus.Success)
        {
            return Attempt.FailWithStatus<IDictionaryItem?, DictionaryImportOperationStatus>(loadResult.Status, null);
        }

        // import the UDT file
        (IDictionaryItem? DictionaryItem, DictionaryImportOperationStatus Status) importResult = await ImportUdtFile(loadResult.Document, userKey, parentKey, temporaryFile);

        scope.Complete();

        return importResult.Status == DictionaryImportOperationStatus.Success
            ? Attempt.SucceedWithStatus(DictionaryImportOperationStatus.Success, importResult.DictionaryItem)
            : Attempt.FailWithStatus<IDictionaryItem?, DictionaryImportOperationStatus>(importResult.Status, null);
    }

    private async Task<(XDocument Document, DictionaryImportOperationStatus Status)> LoadUdtFileAsync(TemporaryFileModel temporaryFileModel)
    {
        try
        {
            await using var dataStream = temporaryFileModel.OpenReadStream();
            XDocument document = await XDocument.LoadAsync(dataStream, LoadOptions.None, CancellationToken.None);

            return document.Root != null
                ? (document, DictionaryImportOperationStatus.Success)
                : (document, DictionaryImportOperationStatus.InvalidFileContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading UDT file: {FileName}", temporaryFileModel.FileName);
            return (new XDocument(), DictionaryImportOperationStatus.InvalidFileContent);
        }
    }

    private async Task<(IDictionaryItem? DictionaryItem, DictionaryImportOperationStatus Status)> ImportUdtFile(XDocument udtFileContent, Guid userKey, Guid? parentKey, TemporaryFileModel temporaryFileModel)
    {
        if (udtFileContent.Root == null)
        {
            return (null, DictionaryImportOperationStatus.InvalidFileContent);
        }

        try
        {
            var currentUserId = (await _userService.GetRequiredUserAsync(userKey)).Id;
            IEnumerable<IDictionaryItem> dictionaryItems = _packageDataInstallation.ImportDictionaryItem(udtFileContent.Root, currentUserId, parentKey);
            IDictionaryItem? importedDictionaryItem = dictionaryItems.FirstOrDefault();
            return importedDictionaryItem != null
                ? (importedDictionaryItem, DictionaryImportOperationStatus.Success)
                : (null, DictionaryImportOperationStatus.InvalidFileContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing UDT file: {FileName}", temporaryFileModel.FileName);
            return (null, DictionaryImportOperationStatus.InvalidFileContent);
        }
    }
}
