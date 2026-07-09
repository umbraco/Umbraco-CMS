using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides functionality for managing temporary files used during uploads and other operations.
/// </summary>
/// <remarks>
///     Temporary files are stored for a limited time based on <see cref="RuntimeSettings.TemporaryFileLifeTime" />
///     and are automatically cleaned up when they expire.
/// </remarks>
internal sealed class TemporaryFileService : ITemporaryFileService
{
    private readonly ITemporaryFileRepository _temporaryFileRepository;
    private readonly IFileStreamSecurityValidator _fileStreamSecurityValidator;
    private RuntimeSettings _runtimeSettings;
    private ContentSettings _contentSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TemporaryFileService" /> class.
    /// </summary>
    /// <param name="temporaryFileRepository">The repository for temporary file data access.</param>
    /// <param name="runtimeOptionsMonitor">The options monitor for runtime settings.</param>
    /// <param name="contentOptionsMonitor">The options monitor for content settings.</param>
    /// <param name="fileStreamSecurityValidator">The validator for checking file stream security.</param>
    public TemporaryFileService(
        ITemporaryFileRepository temporaryFileRepository,
        IOptionsMonitor<RuntimeSettings> runtimeOptionsMonitor,
        IOptionsMonitor<ContentSettings> contentOptionsMonitor,
        IFileStreamSecurityValidator fileStreamSecurityValidator)
    {
        _temporaryFileRepository = temporaryFileRepository;
        _fileStreamSecurityValidator = fileStreamSecurityValidator;

        _runtimeSettings = runtimeOptionsMonitor.CurrentValue;
        _contentSettings = contentOptionsMonitor.CurrentValue;

        runtimeOptionsMonitor.OnChange(x => _runtimeSettings = x);
        contentOptionsMonitor.OnChange(x => _contentSettings = x);
    }

    /// <inheritdoc />
    public async Task<Attempt<TemporaryFileModel?, TemporaryFileOperationStatus>> CreateAsync(CreateTemporaryFileModel createModel)
    {
        TemporaryFileOperationStatus validationResult = Validate(createModel);
        if (validationResult != TemporaryFileOperationStatus.Success)
        {
            return Attempt.FailWithStatus<TemporaryFileModel?, TemporaryFileOperationStatus>(validationResult, null);
        }

        TemporaryFileModel? temporaryFileModel = await _temporaryFileRepository.GetAsync(createModel.Key);
        if (temporaryFileModel is not null)
        {
            return Attempt.FailWithStatus<TemporaryFileModel?, TemporaryFileOperationStatus>(TemporaryFileOperationStatus.KeyAlreadyUsed, null);
        }

        await using Stream dataStream = createModel.OpenReadStream();
        dataStream.Seek(0, SeekOrigin.Begin);
        if (_fileStreamSecurityValidator.IsConsideredSafe(dataStream) is false)
        {
            return Attempt.FailWithStatus<TemporaryFileModel?, TemporaryFileOperationStatus>(TemporaryFileOperationStatus.UploadBlocked, null);
        }

        temporaryFileModel = new TemporaryFileModel
        {
            Key = createModel.Key,
            FileName = createModel.FileName,
            OpenReadStream = createModel.OpenReadStream,
            AvailableUntil = DateTime.Now.Add(_runtimeSettings.TemporaryFileLifeTime),
        };

        await _temporaryFileRepository.SaveAsync(temporaryFileModel);

        return Attempt<TemporaryFileModel?, TemporaryFileOperationStatus>.Succeed(TemporaryFileOperationStatus.Success, temporaryFileModel);
    }

    /// <summary>
    ///     Validates a temporary file model for allowed file extensions and valid file name.
    /// </summary>
    /// <param name="temporaryFileModel">The temporary file model to validate.</param>
    /// <returns>The operation status indicating success or the type of validation failure.</returns>
    private TemporaryFileOperationStatus Validate(TemporaryFileModelBase temporaryFileModel)
    {
        if (IsAllowedFileExtension(temporaryFileModel.FileName) == false)
        {
            return TemporaryFileOperationStatus.FileExtensionNotAllowed;
        }

        if (IsValidFileName(temporaryFileModel.FileName) == false)
        {
            return TemporaryFileOperationStatus.InvalidFileName;
        }

        return TemporaryFileOperationStatus.Success;
    }

    /// <summary>
    ///     Determines whether the file extension is allowed for upload.
    /// </summary>
    /// <param name="fileName">The file name to check.</param>
    /// <returns><c>true</c> if the file extension is allowed; otherwise, <c>false</c>.</returns>
    private bool IsAllowedFileExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName)[1..];
        return _contentSettings.IsFileAllowedForUpload(extension);
    }

    /// <summary>
    ///     Determines whether the file name is valid (not empty and contains no invalid characters).
    /// </summary>
    /// <param name="fileName">The file name to validate.</param>
    /// <returns><c>true</c> if the file name is valid; otherwise, <c>false</c>.</returns>
    private static bool IsValidFileName(string fileName) =>
        !string.IsNullOrEmpty(fileName) && fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;

    /// <inheritdoc />
    public async Task<Attempt<TemporaryFileModel?, TemporaryFileOperationStatus>> DeleteAsync(Guid key)
    {
        TemporaryFileModel? model = await _temporaryFileRepository.GetAsync(key);
        if (model is null)
        {
            return Attempt.FailWithStatus<TemporaryFileModel?, TemporaryFileOperationStatus>(TemporaryFileOperationStatus.NotFound, null);
        }

        await _temporaryFileRepository.DeleteAsync(key);

        return Attempt<TemporaryFileModel?, TemporaryFileOperationStatus>.Succeed(TemporaryFileOperationStatus.Success, model);
    }

    /// <inheritdoc />
    public async Task<TemporaryFileModel?> GetAsync(Guid key) => await _temporaryFileRepository.GetAsync(key);

    /// <inheritdoc />
    public async Task<IEnumerable<Guid>> CleanUpOldTempFiles() => await _temporaryFileRepository.CleanUpOldTempFiles(DateTime.Now);
}
