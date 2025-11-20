using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.ImportExport;

public interface IMemberTypeImportService
{
    Task<Attempt<IMemberType?, MemberTypeImportOperationStatus>> Import(
        Guid temporaryFileId,
        Guid userKey,
        Guid? mediaTypeId = null);
}
