using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface ITemporaryFileService
{
    Task<Attempt<TemporaryFileModel, TemporaryFileOperationStatus>> CreateAsync(TemporaryFileModel temporaryFileModel);

    Task<Attempt<TemporaryFileModel, TemporaryFileOperationStatus>> DeleteAsync(Guid key);

    Task<TemporaryFileModel?> GetAsync(Guid key);

    Task<IEnumerable<Guid>> CleanUpOldTempFiles();
}
