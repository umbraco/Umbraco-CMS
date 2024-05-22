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
