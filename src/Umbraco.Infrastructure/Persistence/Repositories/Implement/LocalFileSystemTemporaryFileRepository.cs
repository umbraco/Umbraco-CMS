using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal sealed class LocalFileSystemTemporaryFileRepository : ITemporaryFileRepository
{
    private const string MetaDataFileName = ".metadata";
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ILogger<LocalFileSystemTemporaryFileRepository> _logger;
    private readonly IJsonSerializer _jsonSerializer;

    public LocalFileSystemTemporaryFileRepository(
        IHostingEnvironment hostingEnvironment,
        ILogger<LocalFileSystemTemporaryFileRepository> logger,
        IJsonSerializer jsonSerializer)
    {
        _hostingEnvironment = hostingEnvironment;
        _logger = logger;
        _jsonSerializer = jsonSerializer;
    }

    private DirectoryInfo GetRootDirectory()
    {
        var path = Path.Combine(_hostingEnvironment.LocalTempPath, "TemporaryFile");

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return new DirectoryInfo(path);
    }

    public async Task<TemporaryFileModel?> GetAsync(Guid key)
    {

        DirectoryInfo rootDirectory = GetRootDirectory();

        DirectoryInfo? fileDirectory = rootDirectory.GetDirectories(key.ToString()).FirstOrDefault();
        if (fileDirectory is null)
        {
            return null;
        }

        FileInfo[] files = fileDirectory.GetFiles();

        if (files.Length != 2)
        {
            _logger.LogError("Unexpected number of files in folder {FolderPath}",  fileDirectory.FullName);
            return null;
        }

        var (actualFile, metadataFile) = GetFilesByType(files);

        FileMetaData metaData = await GetMetaDataAsync(metadataFile);

        return new TemporaryFileModel()
        {
            FileName = actualFile.Name,
            Key = key,
            OpenReadStream = () => actualFile.CreateReadStream(),
            AvailableUntil = metaData.AvailableUntil
        };
    }

    public async Task SaveAsync(TemporaryFileModel model)
    {
        // Ensure folder does not exist
        await DeleteAsync(model.Key);

        DirectoryInfo rootDirectory = GetRootDirectory();

        DirectoryInfo fileDirectory = rootDirectory.CreateSubdirectory(model.Key.ToString());

        var fullFileName = Path.Combine(fileDirectory.FullName, model.FileName);
        var metadataFileName = Path.Combine(fileDirectory.FullName, MetaDataFileName);

        await Task.WhenAll(
            CreateActualFile(model, fullFileName),
            CreateMetadataFile(metadataFileName, new FileMetaData()
            {
                AvailableUntil = model.AvailableUntil
            }));
    }

    public Task DeleteAsync(Guid key)
    {
        DirectoryInfo rootDirectory = GetRootDirectory();

        DirectoryInfo? fileDirectory = rootDirectory.GetDirectories(key.ToString()).FirstOrDefault();

        if (fileDirectory is not null)
        {
            fileDirectory.Delete(true);
        }

        return Task.CompletedTask;
    }

    public async Task<IEnumerable<Guid>> CleanUpOldTempFiles(DateTime now)
    {
        DirectoryInfo rootDirectory = GetRootDirectory();

        var keysToDelete = new List<Guid>();

        foreach (DirectoryInfo fileDirectory in rootDirectory.EnumerateDirectories())
        {
            var metadataFileName = Path.Combine(fileDirectory.FullName, MetaDataFileName);
            FileMetaData metaData = await GetMetaDataAsync(new PhysicalFileInfo(new FileInfo(metadataFileName)));

            if (metaData.AvailableUntil < now)
            {
                keysToDelete.Add(Guid.Parse(fileDirectory.Name));
            }
        }

        await Task.WhenAll(keysToDelete.Select(DeleteAsync).ToArray());

        return keysToDelete;
    }

    private async Task CreateMetadataFile(string fullFilePath, FileMetaData metaData)
    {
        var metadataContent = _jsonSerializer.Serialize(metaData);

        await File.WriteAllTextAsync(fullFilePath, metadataContent);
    }

    private static async Task CreateActualFile(TemporaryFileModel model, string fullFilePath)
    {
        FileStream fileStream = File.Create(fullFilePath);
        await using var dataStream = model.OpenReadStream();
        dataStream.Seek(0, SeekOrigin.Begin);
        await dataStream.CopyToAsync(fileStream);
        fileStream.Close();
    }

    private async Task<FileMetaData> GetMetaDataAsync(IFileInfo metadataFile)
    {
        using var reader = new StreamReader(metadataFile.CreateReadStream());
        var fileContent = await reader.ReadToEndAsync();
        FileMetaData? result = _jsonSerializer.Deserialize<FileMetaData>(fileContent);

        if (result is not null)
        {
            return result;
        }

        _logger.LogError("Unexpected metadata {FilePath}\n{FileContent}", metadataFile.PhysicalPath, fileContent);
        throw new InvalidOperationException("Unexpected content");
    }

    private class FileMetaData
    {
        public DateTime AvailableUntil { get; init; }
    }

    private (IFileInfo actualFile, IFileInfo metadataFile) GetFilesByType(FileInfo[] files) =>
        files[0].Name == MetaDataFileName
            ? (new PhysicalFileInfo(files[1]), new PhysicalFileInfo(files[0]))
            : (new PhysicalFileInfo(files[0]), new PhysicalFileInfo(files[1]));
}
