using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.FileSystem;

internal abstract class FolderServiceOperationBase<TRepository, TFolderModel, TOperationStatus>
    where TRepository : IFileWithFoldersRepository
    where TFolderModel : FolderModelBase, new()
    where TOperationStatus : Enum
{
    private readonly TRepository _repository;

    private readonly ICoreScopeProvider _scopeProvider;

    protected FolderServiceOperationBase(TRepository repository, ICoreScopeProvider scopeProvider)
    {
        _repository = repository;
        _scopeProvider = scopeProvider;
    }

    protected abstract TOperationStatus Success { get; }

    protected abstract TOperationStatus NotFound { get; }

    protected abstract TOperationStatus NotEmpty { get; }

    protected abstract TOperationStatus AlreadyExists { get; }

    protected abstract TOperationStatus ParentNotFound { get; }

    protected abstract TOperationStatus InvalidName { get; }

    protected async Task<Attempt<TFolderModel?, TOperationStatus>> HandleCreateAsync(string name, string? parentPath)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        var path = GetFolderPath(name, parentPath);
        TOperationStatus validateResult = ValidateCreate(name, path, parentPath);
        if (Success.Equals(validateResult) is false)
        {
            return Attempt.FailWithStatus<TFolderModel?, TOperationStatus>(validateResult, null);
        }

        _repository.AddFolder(path);

        scope.Complete();

        var result = new TFolderModel
        {
            Name = name,
            Path = path,
            ParentPath = GetParentFolderPath(path)
        };
        return await Task.FromResult(Attempt.SucceedWithStatus<TFolderModel?, TOperationStatus>(Success, result));
    }

    protected async Task<TOperationStatus> HandleDeleteAsync(string path)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        TOperationStatus validateResult = ValidateDelete(path);
        if (Success.Equals(validateResult) is false)
        {
            return await Task.FromResult(validateResult);
        }

        _repository.DeleteFolder(path);

        scope.Complete();

        return Success;
    }

    protected async Task<TFolderModel?> HandleGetAsync(string path)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        // There's not much we can actually get when it's a folder, so it more a matter of ensuring the folder exists and returning a model.
        if (_repository.FolderExists(path) is false)
        {
            return await Task.FromResult<TFolderModel?>(null);
        }

        var result = new TFolderModel
        {
            Name = GetFolderName(path),
            Path = path,
            ParentPath = GetParentFolderPath(path)
        };

        scope.Complete();
        return result;
    }

    private TOperationStatus ValidateCreate(string name, string path, string? parentPath)
    {
        if (name.ContainsAny(Path.GetInvalidFileNameChars()))
        {
            return InvalidName;
        }

        if (_repository.FolderExists(path))
        {
            return AlreadyExists;
        }

        if (string.IsNullOrWhiteSpace(parentPath) is false && _repository.FolderExists(parentPath) is false)
        {
            return ParentNotFound;
        }

        return Success;
    }

    private TOperationStatus ValidateDelete(string path)
    {
        if (_repository.FolderExists(path) is false)
        {
            return NotFound;
        }

        if (_repository.FolderHasContent(path))
        {
            return NotEmpty;
        }

        return Success;
    }

    private string GetFolderPath(string folderName, string? parentPath)
        => Path.Join(parentPath, folderName);

    private string GetFolderName(string path)
        => Path.GetFileName(path);

    private string? GetParentFolderPath(string path)
        => Path.GetDirectoryName(path);
}
