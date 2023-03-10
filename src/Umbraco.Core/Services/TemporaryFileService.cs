using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

internal sealed class TemporaryFileService : ITemporaryFileService
{
    private readonly ITemporaryFileRepository _temporaryFileRepository;
    private RuntimeSettings _runtimeSettings;
    private ContentSettings _contentSettings;

    public TemporaryFileService(
        ITemporaryFileRepository temporaryFileRepository,
        IOptionsMonitor<RuntimeSettings> runtimeOptionsMonitor,
        IOptionsMonitor<ContentSettings> contentOptionsMonitor
        )
    {
        _temporaryFileRepository = temporaryFileRepository;

        _runtimeSettings = runtimeOptionsMonitor.CurrentValue;
        _contentSettings = contentOptionsMonitor.CurrentValue;

        runtimeOptionsMonitor.OnChange(x => _runtimeSettings = x);
        contentOptionsMonitor.OnChange(x => _contentSettings = x);
    }

    public async Task<Attempt<TempFileModel, TemporaryFileStatus>> CreateAsync(TempFileModel tempFileModel)
    {
        Attempt<TempFileModel, TemporaryFileStatus> validationResult = await ValidateAsync(tempFileModel);
        if (validationResult.Success == false)
        {
            return validationResult;
        }

        tempFileModel.AvailableUntil = DateTime.Now.Add(_runtimeSettings.TemporaryFileLifeTime);

        TempFileModel? file = await _temporaryFileRepository.GetAsync(tempFileModel.Key);
        if (file is not null)
        {
            return Attempt<TempFileModel, TemporaryFileStatus>.Fail(TemporaryFileStatus.KeyAlreadyUsed, tempFileModel);
        }

        await _temporaryFileRepository.SaveAsync(tempFileModel);

        return Attempt<TempFileModel, TemporaryFileStatus>.Succeed(TemporaryFileStatus.Success, tempFileModel);
    }

    private async Task<Attempt<TempFileModel, TemporaryFileStatus>> ValidateAsync(TempFileModel tempFileModel)
    {
        if (IsAllowedFileExtension(tempFileModel) == false)
        {
            return Attempt.FailWithStatus<TempFileModel, TemporaryFileStatus>(TemporaryFileStatus.FileExtensionNotAllowed, tempFileModel);
        }

        return Attempt.SucceedWithStatus<TempFileModel, TemporaryFileStatus>(TemporaryFileStatus.Success, tempFileModel);
    }

    private bool IsAllowedFileExtension(TempFileModel tempFileModel)
    {
        var extension = Path.GetExtension(tempFileModel.FileName)[1..];

        return _contentSettings.IsFileAllowedForUpload(extension);
    }

    public async Task<Attempt<TempFileModel, TemporaryFileStatus>> DeleteAsync(Guid key)
    {
        TempFileModel? model = await _temporaryFileRepository.GetAsync(key);
        if (model is null)
        {
            return Attempt<TempFileModel, TemporaryFileStatus>.Fail(TemporaryFileStatus.NotFound, new TempFileModel());
        }

        await _temporaryFileRepository.DeleteAsync(key);

        return Attempt<TempFileModel, TemporaryFileStatus>.Succeed(TemporaryFileStatus.Success, model);
    }

    public async Task<TempFileModel?> GetAsync(Guid key) => await _temporaryFileRepository.GetAsync(key);
    public async Task<IEnumerable<Guid>> CleanUpOldTempFiles() => await _temporaryFileRepository.CleanUpOldTempFiles(DateTime.Now);
}
