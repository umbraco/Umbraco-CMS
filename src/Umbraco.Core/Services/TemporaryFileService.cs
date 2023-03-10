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

    public async Task<Attempt<TemporaryFileModel, TemporaryFileStatus>> CreateAsync(TemporaryFileModel temporaryFileModel)
    {
        Attempt<TemporaryFileModel, TemporaryFileStatus> validationResult = await ValidateAsync(temporaryFileModel);
        if (validationResult.Success == false)
        {
            return validationResult;
        }

        temporaryFileModel.AvailableUntil = DateTime.Now.Add(_runtimeSettings.TemporaryFileLifeTime);

        TemporaryFileModel? file = await _temporaryFileRepository.GetAsync(temporaryFileModel.Key);
        if (file is not null)
        {
            return Attempt<TemporaryFileModel, TemporaryFileStatus>.Fail(TemporaryFileStatus.KeyAlreadyUsed, temporaryFileModel);
        }

        await _temporaryFileRepository.SaveAsync(temporaryFileModel);

        return Attempt<TemporaryFileModel, TemporaryFileStatus>.Succeed(TemporaryFileStatus.Success, temporaryFileModel);
    }

    private async Task<Attempt<TemporaryFileModel, TemporaryFileStatus>> ValidateAsync(TemporaryFileModel temporaryFileModel)
    {
        if (IsAllowedFileExtension(temporaryFileModel) == false)
        {
            return Attempt.FailWithStatus<TemporaryFileModel, TemporaryFileStatus>(TemporaryFileStatus.FileExtensionNotAllowed, temporaryFileModel);
        }

        return Attempt.SucceedWithStatus<TemporaryFileModel, TemporaryFileStatus>(TemporaryFileStatus.Success, temporaryFileModel);
    }

    private bool IsAllowedFileExtension(TemporaryFileModel temporaryFileModel)
    {
        var extension = Path.GetExtension(temporaryFileModel.FileName)[1..];

        return _contentSettings.IsFileAllowedForUpload(extension);
    }

    public async Task<Attempt<TemporaryFileModel, TemporaryFileStatus>> DeleteAsync(Guid key)
    {
        TemporaryFileModel? model = await _temporaryFileRepository.GetAsync(key);
        if (model is null)
        {
            return Attempt<TemporaryFileModel, TemporaryFileStatus>.Fail(TemporaryFileStatus.NotFound, new TemporaryFileModel());
        }

        await _temporaryFileRepository.DeleteAsync(key);

        return Attempt<TemporaryFileModel, TemporaryFileStatus>.Succeed(TemporaryFileStatus.Success, model);
    }

    public async Task<TemporaryFileModel?> GetAsync(Guid key) => await _temporaryFileRepository.GetAsync(key);
    public async Task<IEnumerable<Guid>> CleanUpOldTempFiles() => await _temporaryFileRepository.CleanUpOldTempFiles(DateTime.Now);
}
