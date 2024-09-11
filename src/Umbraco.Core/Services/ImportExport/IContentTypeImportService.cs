using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.ImportExport;

public interface IContentTypeImportService
{
    Task<Attempt<IContentType?, ContentTypeImportOperationStatus>> Import(Guid temporaryFileId, Guid userKey, Guid? contentTypeId = null);
}
