using Umbraco.Cms.Core.Models.TemporaryFile;

namespace Umbraco.Cms.Core.Services;

public interface ITemporaryFileService
{
    Task<Attempt<TempFileModel, TemporaryFileStatus>> CreateAsync(TempFileModel tempFileModel);

    Task<Attempt<TempFileModel, TemporaryFileStatus>> DeleteAsync(Guid key);

    Task<TempFileModel?> GetAsync(Guid key);

    Task<IEnumerable<Guid>> CleanUpOldTempFiles();
}
