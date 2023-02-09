using Microsoft.AspNetCore.Http;

namespace Umbraco.Cms.Api.Management.Services;

public interface ITemporaryFileService
{
    Task<string> GetFilePathAsync(string fileName);

    Task<string> SaveFileAsync(IFormFile file);

    Task<bool> DeleteFileAsync(string fileName);
}
