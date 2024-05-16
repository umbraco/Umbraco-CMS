using System.Xml.Linq;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ImportExport;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.ImportExport;

public interface ITemporaryFileToXmlImportService
{
    Task<Attempt<XElement?, TemporaryFileXmlImportOperationStatus>> LoadXElementFromTemporaryFileAsync(
        Guid temporaryFileId);

    Attempt<UmbracoEntityTypes> GetEntityType(XElement entityElement);

    Task<Attempt<EntityXmlAnalysis?, TemporaryFileXmlImportOperationStatus>> AnalyzeAsync(
        Guid temporaryFileId);

    void CleanupFileIfScopeCompletes(Guid temporaryFileId);
}
