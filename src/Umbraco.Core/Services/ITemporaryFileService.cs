using Umbraco.Cms.Core.Models.TemporaryFile;

namespace Umbraco.Cms.Core.Services;

public interface ITemporaryFileService
{
    Task<Attempt<TemporaryFileModel, TemporaryFileStatus>> CreateAsync(TemporaryFileModel temporaryFileModel);

    Task<Attempt<TemporaryFileModel, TemporaryFileStatus>> DeleteAsync(Guid key);

    Task<TemporaryFileModel?> GetAsync(Guid key);

    Task<IEnumerable<Guid>> CleanUpOldTempFiles();
}
