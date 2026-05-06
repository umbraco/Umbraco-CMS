using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.FileSystem;

/// <summary>
///     Provides a base implementation for folder service operations in the file system.
/// </summary>
/// <typeparam name="TRepository">The type of the repository that implements <see cref="IFileWithFoldersRepository"/>.</typeparam>
/// <typeparam name="TFolderModel">The type of the folder model that inherits from <see cref="FolderModelBase"/>.</typeparam>
/// <typeparam name="TOperationStatus">The type of the operation status enumeration.</typeparam>
/// <remarks>
///     This abstract base class provides common functionality for creating, deleting, and retrieving
///     folders in the file system. Derived classes must provide the specific operation status values
///     for their respective folder types.
/// </remarks>
internal abstract class FolderServiceOperationBase<TRepository, TFolderModel, TOperationStatus>
    where TRepository : IFileWithFoldersRepository
    where TFolderModel : FolderModelBase, new()
    where TOperationStatus : Enum
{
    private readonly TRepository _repository;

    private readonly ICoreScopeProvider _scopeProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FolderServiceOperationBase{TRepository, TFolderModel, TOperationStatus}"/> class.
    /// </summary>
    /// <param name="repository">The repository for file system operations.</param>
    /// <param name="scopeProvider">The scope provider for database transactions.</param>
    protected FolderServiceOperationBase(TRepository repository, ICoreScopeProvider scopeProvider)
    {
        _repository = repository;
        _scopeProvider = scopeProvider;
    }

    /// <summary>
    ///     Gets the operation status value indicating a successful operation.
    /// </summary>
    protected abstract TOperationStatus Success { get; }

    /// <summary>
    ///     Gets the operation status value indicating that the folder was not found.
    /// </summary>
    protected abstract TOperationStatus NotFound { get; }

    /// <summary>
    ///     Gets the operation status value indicating that the folder is not empty.
    /// </summary>
    protected abstract TOperationStatus NotEmpty { get; }

    /// <summary>
    ///     Gets the operation status value indicating that a folder already exists at the specified path.
    /// </summary>
    protected abstract TOperationStatus AlreadyExists { get; }

    /// <summary>
    ///     Gets the operation status value indicating that the parent folder was not found.
    /// </summary>
    protected abstract TOperationStatus ParentNotFound { get; }

    /// <summary>
    ///     Gets the operation status value indicating that the folder name is invalid.
    /// </summary>
    protected abstract TOperationStatus InvalidName { get; }

    /// <summary>
    ///     Handles the creation of a new folder.
    /// </summary>
    /// <param name="name">The name of the folder to create.</param>
    /// <param name="parentPath">The path of the parent folder, or <c>null</c> for root-level folders.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an
    ///     <see cref="Attempt{TResult, TStatus}"/> with the created folder model on success,
    ///     or the appropriate operation status on failure.
    /// </returns>
    protected Task<Attempt<TFolderModel?, TOperationStatus>> HandleCreateAsync(string name, string? parentPath)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        var path = GetFolderPath(name, parentPath);
        TOperationStatus validateResult = ValidateCreate(name, path, parentPath);
        if (Success.Equals(validateResult) is false)
        {
            return Task.FromResult(Attempt.FailWithStatus<TFolderModel?, TOperationStatus>(validateResult, null));
        }

        _repository.AddFolder(path);

        scope.Complete();

        var result = new TFolderModel
        {
            Name = name,
            Path = path,
            ParentPath = GetParentFolderPath(path)
        };
        return Task.FromResult(Attempt.SucceedWithStatus<TFolderModel?, TOperationStatus>(Success, result));
    }

    /// <summary>
    ///     Handles the deletion of a folder.
    /// </summary>
    /// <param name="path">The path of the folder to delete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains
    ///     the operation status indicating success or the reason for failure.
    /// </returns>
    protected Task<TOperationStatus> HandleDeleteAsync(string path)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        TOperationStatus validateResult = ValidateDelete(path);
        if (Success.Equals(validateResult) is false)
        {
            return Task.FromResult(validateResult);
        }

        _repository.DeleteFolder(path);

        scope.Complete();

        return Task.FromResult(Success);
    }

    /// <summary>
    ///     Handles retrieving a folder by its path.
    /// </summary>
    /// <param name="path">The path of the folder to retrieve.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains
    ///     the folder model if found; otherwise, <c>null</c>.
    /// </returns>
    protected Task<TFolderModel?> HandleGetAsync(string path)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        // There's not much we can actually get when it's a folder, so it more a matter of ensuring the folder exists and returning a model.
        if (_repository.FolderExists(path) is false)
        {
            return Task.FromResult<TFolderModel?>(null);
        }

        var result = new TFolderModel
        {
            Name = GetFolderName(path),
            Path = path,
            ParentPath = GetParentFolderPath(path)
        };

        scope.Complete();

        return Task.FromResult<TFolderModel?>(result);
    }

    /// <summary>
    ///     Validates that a folder can be created with the specified parameters.
    /// </summary>
    /// <param name="name">The name of the folder to create.</param>
    /// <param name="path">The full path of the folder to create.</param>
    /// <param name="parentPath">The path of the parent folder, or <c>null</c> for root-level folders.</param>
    /// <returns>
    ///     The <see cref="Success"/> status if validation passes; otherwise, the appropriate
    ///     error status indicating why the folder cannot be created.
    /// </returns>
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

    /// <summary>
    ///     Validates that a folder can be deleted at the specified path.
    /// </summary>
    /// <param name="path">The path of the folder to delete.</param>
    /// <returns>
    ///     The <see cref="Success"/> status if validation passes; otherwise, the appropriate
    ///     error status indicating why the folder cannot be deleted.
    /// </returns>
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

    /// <summary>
    ///     Constructs the full folder path by combining the parent path and folder name.
    /// </summary>
    /// <param name="folderName">The name of the folder.</param>
    /// <param name="parentPath">The path of the parent folder, or <c>null</c> for root-level folders.</param>
    /// <returns>The combined folder path.</returns>
    private string GetFolderPath(string folderName, string? parentPath)
        => Path.Join(parentPath, folderName);

    /// <summary>
    ///     Extracts the folder name from a full path.
    /// </summary>
    /// <param name="path">The full path to the folder.</param>
    /// <returns>The folder name.</returns>
    private string GetFolderName(string path)
        => Path.GetFileName(path);

    /// <summary>
    ///     Extracts the parent folder path from a full path.
    /// </summary>
    /// <param name="path">The full path to the folder.</param>
    /// <returns>The parent folder path, or <c>null</c> if the folder is at the root level.</returns>
    private string? GetParentFolderPath(string path)
        => Path.GetDirectoryName(path);
}
