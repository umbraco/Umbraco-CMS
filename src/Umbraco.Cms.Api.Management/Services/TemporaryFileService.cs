using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;

namespace Umbraco.Cms.Api.Management.Services;

internal sealed class TemporaryFileService : ITemporaryFileService
{
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<TemporaryFileService> _logger;

    public TemporaryFileService(IHostEnvironment hostEnvironment, ILogger<TemporaryFileService> logger)
    {
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    public async Task<string> GetFilePathAsync(string fileName)
    {
        var root = _hostEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempFileUploads);
        fileName = fileName.Trim(Constants.CharArrays.DoubleQuote);

        var filePath = Path.Combine(root, fileName);
        return await Task.FromResult(filePath);
    }

    public async Task<string> SaveFileAsync(IFormFile file)
    {
        var filePath = await GetFilePathAsync(file.FileName);

        await using FileStream fileStream = File.Create(filePath);
        await file.CopyToAsync(fileStream);

        return filePath;
    }

    public async Task<bool> DeleteFileAsync(string fileName)
    {
        var filePath = await GetFilePathAsync(fileName);
        try
        {
            File.Delete(filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting temporary file: {FilePath}", filePath);
            return await Task.FromResult(false);
        }
    }
}
