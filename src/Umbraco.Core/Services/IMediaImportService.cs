using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IMediaImportService
{
    public Task<IMedia> ImportAsync(string fileName, Stream fileStream, Guid? parentId, string? mediaTypeAlias, Guid userKey);
}
