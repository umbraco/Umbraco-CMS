using System.Xml;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.ImportExport;

public class TemporaryFileToXmlImportServiceBase
{
    private readonly ITemporaryFileService _temporaryFileService;

    public TemporaryFileToXmlImportServiceBase(ITemporaryFileService temporaryFileService)
    {
        _temporaryFileService = temporaryFileService;
    }

    internal async Task<Attempt<XElement?, TemporaryFileOperationStatus>> LoadXElementFromTemporaryFileAsync(
        Guid temporaryFileId)
    {
        TemporaryFileModel? documentTypeFile = await _temporaryFileService.GetAsync(temporaryFileId);
        if (documentTypeFile is null)
        {
            // todo
            return Attempt.FailWithStatus<XElement?, TemporaryFileOperationStatus>(
                TemporaryFileOperationStatus.NotFound, null);
        }

        var xmlDocument = new XmlDocument { XmlResolver = null };
        await using (Stream fileStream = documentTypeFile.OpenReadStream())
        {
            xmlDocument.Load(fileStream);
        }

        var element = XElement.Parse(xmlDocument.InnerXml);

        await _temporaryFileService.DeleteAsync(documentTypeFile.Key);
        return Attempt.SucceedWithStatus<XElement?, TemporaryFileOperationStatus>(TemporaryFileOperationStatus.Success,
            element);
    }
}
