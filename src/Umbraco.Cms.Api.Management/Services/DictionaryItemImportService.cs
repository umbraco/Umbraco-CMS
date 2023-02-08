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
        XDocument document;
        try
        {
            var filePath = await _temporaryFileService.GetFilePathAsync(fileName);
            await using FileStream stream = File.OpenRead(filePath);
            document = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading UDT file: {FileName}", fileName);
            return Attempt.FailWithStatus<IDictionaryItem?, DictionaryImportOperationStatus>(DictionaryImportOperationStatus.InvalidFileContent, null);
        }

        if (document.Root == null)
        {
            return Attempt.FailWithStatus<IDictionaryItem?, DictionaryImportOperationStatus>(DictionaryImportOperationStatus.InvalidFileContent, null);
        }

        // import the UDT file
        IDictionaryItem? importedDictionaryItem;
        try
        {
            IEnumerable<IDictionaryItem> dictionaryItems = _packageDataInstallation.ImportDictionaryItem(document.Root, userId, parentKey);
            importedDictionaryItem = dictionaryItems.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing UDT file: {FileName}", fileName);
            return Attempt.FailWithStatus<IDictionaryItem?, DictionaryImportOperationStatus>(DictionaryImportOperationStatus.InvalidFileContent, null);
        }

        if (importedDictionaryItem == null)
        {
            return Attempt.FailWithStatus<IDictionaryItem?, DictionaryImportOperationStatus>(DictionaryImportOperationStatus.InvalidFileContent, null);
        }

        // clean up the UDT file (we don't care about success or failure at this point, we'll let the temporary file service handle those)
        await _temporaryFileService.DeleteFileAsync(fileName);

        return Attempt.SucceedWithStatus<IDictionaryItem?, DictionaryImportOperationStatus>(DictionaryImportOperationStatus.Success, importedDictionaryItem);
    }
}
