using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services.OperationStatus;
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
        IOptionsMonitor<ContentSettings> contentOptionsMonitor)
    {
        _temporaryFileRepository = temporaryFileRepository;

        _runtimeSettings = runtimeOptionsMonitor.CurrentValue;
        _contentSettings = contentOptionsMonitor.CurrentValue;

        runtimeOptionsMonitor.OnChange(x => _runtimeSettings = x);
        contentOptionsMonitor.OnChange(x => _contentSettings = x);
    }

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

        temporaryFileModel = new TemporaryFileModel
        {
            Key = createModel.Key,
            FileName = createModel.FileName,
            OpenReadStream = createModel.OpenReadStream,
            AvailableUntil = DateTime.Now.Add(_runtimeSettings.TemporaryFileLifeTime)
        };

        await _temporaryFileRepository.SaveAsync(temporaryFileModel);

        return Attempt<TemporaryFileModel?, TemporaryFileOperationStatus>.Succeed(TemporaryFileOperationStatus.Success, temporaryFileModel);
    }

    private TemporaryFileOperationStatus Validate(TemporaryFileModelBase temporaryFileModel)
        => IsAllowedFileExtension(temporaryFileModel) == false
            ? TemporaryFileOperationStatus.FileExtensionNotAllowed
            : TemporaryFileOperationStatus.Success;

    private bool IsAllowedFileExtension(TemporaryFileModelBase temporaryFileModel)
    {
        var extension = Path.GetExtension(temporaryFileModel.FileName)[1..];

        return _contentSettings.IsFileAllowedForUpload(extension);
    }

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

    public async Task<TemporaryFileModel?> GetAsync(Guid key) => await _temporaryFileRepository.GetAsync(key);

    public async Task<IEnumerable<Guid>> CleanUpOldTempFiles() => await _temporaryFileRepository.CleanUpOldTempFiles(DateTime.Now);
}
