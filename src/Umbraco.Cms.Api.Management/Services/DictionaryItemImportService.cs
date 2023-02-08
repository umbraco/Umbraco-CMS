using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Api.Management.Services.OperationStatus;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Packaging;
using Umbraco.Extensions;
using File = System.IO.File;

namespace Umbraco.Cms.Api.Management.Services;

internal sealed class DictionaryItemImportService : IDictionaryItemImportService
{
    private readonly IDictionaryItemService _dictionaryItemService;
    private readonly PackageDataInstallation _packageDataInstallation;
    private readonly ILogger<DictionaryItemImportService> _logger;
    private readonly ITemporaryFileService _temporaryFileService;

    public DictionaryItemImportService(
        IDictionaryItemService dictionaryItemService,
        PackageDataInstallation packageDataInstallation,
        ILogger<DictionaryItemImportService> logger,
        ITemporaryFileService temporaryFileService)
    {
        _dictionaryItemService = dictionaryItemService;
        _packageDataInstallation = packageDataInstallation;
        _logger = logger;
        _temporaryFileService = temporaryFileService;
    }

    public async Task<Attempt<IDictionaryItem?, DictionaryImportOperationStatus>> ImportDictionaryItemFromUdtFileAsync(string fileName, Guid? parentKey, int userId = Constants.Security.SuperUserId)
    {
        if (".udt".InvariantEquals(Path.GetExtension(fileName)) == false)
        {
            return Attempt.FailWithStatus<IDictionaryItem?, DictionaryImportOperationStatus>(DictionaryImportOperationStatus.InvalidFileType, null);
        }

        if (parentKey.HasValue && await _dictionaryItemService.GetAsync(parentKey.Value) == null)
        {
            return Attempt.FailWithStatus<IDictionaryItem?, DictionaryImportOperationStatus>(DictionaryImportOperationStatus.ParentNotFound, null);
        }

        // load the UDT file from disk
        (XDocument Document, DictionaryImportOperationStatus Status) loadResult = await LoadUdtFileAsync(fileName);
        if (loadResult.Status != DictionaryImportOperationStatus.Success)
        {
            return Attempt.FailWithStatus<IDictionaryItem?, DictionaryImportOperationStatus>(loadResult.Status, null);
        }

        // import the UDT file
        (IDictionaryItem? DictionaryItem, DictionaryImportOperationStatus Status) importResult = ImportUdtFile(loadResult.Document, userId, parentKey, fileName);

        // clean up the UDT file (we don't care about success or failure at this point, we'll let the temporary file service handle those)
        await _temporaryFileService.DeleteFileAsync(fileName);

        return importResult.Status == DictionaryImportOperationStatus.Success
            ? Attempt.SucceedWithStatus(DictionaryImportOperationStatus.Success, importResult.DictionaryItem)
            : Attempt.FailWithStatus<IDictionaryItem?, DictionaryImportOperationStatus>(importResult.Status, null);
    }

    private async Task<(XDocument Document, DictionaryImportOperationStatus Status)> LoadUdtFileAsync(string fileName)
    {
        try
        {
            var filePath = await _temporaryFileService.GetFilePathAsync(fileName);

            await using FileStream stream = File.OpenRead(filePath);
            XDocument document = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);

            return document.Root != null
                ? (document, DictionaryImportOperationStatus.Success)
                : (document, DictionaryImportOperationStatus.InvalidFileContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading UDT file: {FileName}", fileName);
            return (new XDocument(), DictionaryImportOperationStatus.InvalidFileContent);
        }
    }

    private (IDictionaryItem? DictionaryItem, DictionaryImportOperationStatus Status) ImportUdtFile(XDocument udtFileContent, int userId, Guid? parentKey, string fileName)
    {
        if (udtFileContent.Root == null)
        {
            return (null, DictionaryImportOperationStatus.InvalidFileContent);
        }

        try
        {
            IEnumerable<IDictionaryItem> dictionaryItems = _packageDataInstallation.ImportDictionaryItem(udtFileContent.Root, userId, parentKey);
            IDictionaryItem? importedDictionaryItem = dictionaryItems.FirstOrDefault();
            return importedDictionaryItem != null
                ? (importedDictionaryItem, DictionaryImportOperationStatus.Success)
                : (null, DictionaryImportOperationStatus.InvalidFileContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing UDT file: {FileName}", fileName);
            return (null, DictionaryImportOperationStatus.InvalidFileContent);
        }
    }
}
