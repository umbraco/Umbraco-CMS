using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Persistence.Repositories;

namespace Umbraco.Cms.Core.Services;

internal sealed class TemporaryFileService : ITemporaryFileService
{
    private readonly ITemporaryFileRepository _temporaryFileRepository;
    private RuntimeSettings _runtimeSettings;

    public TemporaryFileService(ITemporaryFileRepository temporaryFileRepository, IOptionsMonitor<RuntimeSettings> optionsMonitor)
    {
        _temporaryFileRepository = temporaryFileRepository;

        _runtimeSettings = optionsMonitor.CurrentValue;

        optionsMonitor.OnChange(x => _runtimeSettings = x);
    }

    public async Task<Attempt<TempFileModel, TemporaryFileStatus>> CreateAsync(TempFileModel tempFileModel)
    {
        tempFileModel.AvailableUntil = DateTime.Now.Add(_runtimeSettings.TemporaryFileLifeTime);

        TempFileModel? file = await _temporaryFileRepository.GetAsync(tempFileModel.Key);
        if (file is not null)
        {
            return Attempt<TempFileModel, TemporaryFileStatus>.Fail(TemporaryFileStatus.KeyAlreadyUsed, tempFileModel);
        }

        await _temporaryFileRepository.SaveAsync(tempFileModel);

        return Attempt<TempFileModel, TemporaryFileStatus>.Succeed(TemporaryFileStatus.Success, tempFileModel);
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
