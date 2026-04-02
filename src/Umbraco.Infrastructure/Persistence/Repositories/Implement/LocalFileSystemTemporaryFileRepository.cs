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

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalFileSystemTemporaryFileRepository"/> class, which manages temporary files using the local file system.
    /// </summary>
    /// <param name="hostingEnvironment">The hosting environment providing information about the web application's environment and file system paths.</param>
    /// <param name="logger">The logger instance used for logging repository operations and errors.</param>
    /// <param name="jsonSerializer">The JSON serializer used for serializing and deserializing file metadata.</param>
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
        var path = _hostingEnvironment.TemporaryFileUploadPath;

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return new DirectoryInfo(path);
    }

    /// <summary>
    /// Gets the temporary file model asynchronously by the specified key.
    /// </summary>
    /// <param name="key">The unique identifier of the temporary file.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="TemporaryFileModel"/> if found; otherwise, null.</returns>
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

    /// <summary>
    /// Asynchronously saves the specified temporary file model to the local file system, overwriting any existing file with the same key.
    /// This operation ensures that the target directory is clean before saving the new file and its metadata.
    /// </summary>
    /// <param name="model">The temporary file model to save.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    public async Task SaveAsync(TemporaryFileModel model)
    {
        // Ensure folder does not exist
        await DeleteAsync(model.Key);

        DirectoryInfo rootDirectory = GetRootDirectory();

        DirectoryInfo fileDirectory = rootDirectory.CreateSubdirectory(model.Key.ToString());

        var fullFileName = Path.Combine(fileDirectory.FullName, model.FileName);
        var metadataFileName = Path.Combine(fileDirectory.FullName, MetaDataFileName);

        await CreateActualFile(model, fullFileName);
        await CreateMetadataFile(metadataFileName, new FileMetaData()
        {
            AvailableUntil = model.AvailableUntil
        });
    }

    /// <summary>
    /// Asynchronously deletes the temporary file directory associated with the specified key, if it exists.
    /// </summary>
    /// <param name="key">The unique identifier for the temporary file directory to delete.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
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

    /// <summary>
    /// Deletes temporary files whose expiration date has passed, as determined by the specified time.
    /// </summary>
    /// <param name="now">The current date and time used to determine which temporary files have expired.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of GUIDs representing the keys of the deleted temporary files.</returns>
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

        foreach (Guid keyToDelete in keysToDelete)
        {
            await DeleteAsync(keyToDelete);
        }

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

    private sealed class FileMetaData
    {
        /// <summary>
        /// Gets the date and time until which the temporary file remains available before it is subject to deletion.
        /// </summary>
        public DateTime AvailableUntil { get; init; }
    }

    private (IFileInfo actualFile, IFileInfo metadataFile) GetFilesByType(FileInfo[] files) =>
        files[0].Name == MetaDataFileName
            ? (new PhysicalFileInfo(files[1]), new PhysicalFileInfo(files[0]))
            : (new PhysicalFileInfo(files[0]), new PhysicalFileInfo(files[1]));
}
