using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core;
using Umbraco.Cms.Api.Management.Models;
using Umbraco.Cms.Api.Management.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Services;

internal sealed class UploadFileService : IUploadFileService
{
    private readonly ITemporaryFileService _temporaryFileService;

    public UploadFileService(ITemporaryFileService temporaryFileService) => _temporaryFileService = temporaryFileService;

    public async Task<Attempt<UdtFileUpload, UdtFileUploadOperationStatus>> UploadUdtFileAsync(IFormFile file)
    {
        UdtFileUpload DefaultModel() => new() { FileName = file.FileName, Content = new XDocument() };

        if (".udt".InvariantEquals(Path.GetExtension(file.FileName)) == false)
        {
            return Attempt.FailWithStatus(UdtFileUploadOperationStatus.InvalidFileType, DefaultModel());
        }

        var filePath = await _temporaryFileService.SaveFileAsync(file);

        XDocument content;
        await using (FileStream fileStream = File.OpenRead(filePath))
        {
            content = await XDocument.LoadAsync(fileStream, LoadOptions.None, CancellationToken.None);
        }

        if (content.Root == null)
        {
            return Attempt.FailWithStatus(UdtFileUploadOperationStatus.InvalidFileContent, DefaultModel());
        }

        // grab the file name from the file path in case the temporary file name is different from the uploaded one
        var model = new UdtFileUpload { FileName = Path.GetFileName(filePath), Content = content };
        return Attempt.SucceedWithStatus(UdtFileUploadOperationStatus.Success, model);
    }
}
