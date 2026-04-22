using System.Reflection;
using System.Text;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal abstract class FileRepository<TId, TEntity> : IReadRepository<TId, TEntity>, IWriteRepository<TEntity>
    where TEntity : IFile
{
    protected FileRepository(IFileSystem? fileSystem) => FileSystem = fileSystem;

    protected IFileSystem? FileSystem { get; }

    /// <summary>
    /// Creates and persists a new folder at the specified path.
    /// </summary>
    /// <param name="folderPath">The path where the new folder will be created.</param>
    public virtual void AddFolder(string folderPath) => PersistNewItem(new Folder(folderPath));

    /// <summary>
    /// Deletes the folder located at the given path from the underlying storage.
    /// </summary>
    /// <param name="folderPath">The path of the folder to delete.</param>
    public virtual void DeleteFolder(string folderPath) => PersistDeletedItem(new Folder(folderPath));

    /// <summary>
    /// Determines whether the specified folder exists in the file system.
    /// </summary>
    /// <param name="folderPath">The path of the folder to check.</param>
    /// <returns><c>true</c> if the folder exists; otherwise, <c>false</c>.</returns>
    public virtual bool FolderExists(string folderPath) => FileSystem?.DirectoryExists(folderPath) is true;

    /// <summary>
    /// Determines whether the specified folder contains any files or directories.
    /// </summary>
    /// <param name="folderPath">The path of the folder to check for content.</param>
    /// <returns><c>true</c> if the folder contains any files or directories; otherwise, <c>false</c>.</returns>
    public virtual bool FolderHasContent(string folderPath) => FileSystem?.GetFiles(folderPath).Any() is true || FileSystem?.GetDirectories(folderPath).Any() is true;

    /// <summary>
    /// Returns a stream for reading the contents of the specified file.
    /// </summary>
    /// <param name="filepath">The path of the file to read.</param>
    /// <returns>
    /// A <see cref="Stream"/> for reading the file's contents, or <see cref="Stream.Null"/> if the file does not exist or cannot be opened.
    /// </returns>
    public Stream GetFileContentStream(string filepath)
    {
        if (FileSystem?.FileExists(filepath) == false)
        {
            return Stream.Null;
        }

        try
        {
            return FileSystem?.OpenFile(filepath) ?? Stream.Null;
        }
        catch
        {
            return Stream.Null; // deal with race conds
        }
    }

    internal virtual void PersistNewFolder(Folder entity) => FileSystem?.CreateFolder(entity.Path);

    internal virtual void PersistDeletedFolder(Folder entity) => FileSystem?.DeleteDirectory(entity.Path);

    /// <summary>
    ///     Gets a stream that is used to write to the file
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    protected virtual Stream GetContentStream(string content) => new MemoryStream(Encoding.UTF8.GetBytes(content));

    /// <summary>
    ///     Returns all files in the file system
    /// </summary>
    /// <param name="path"></param>
    /// <param name="filter"></param>
    /// <returns>
    ///     Returns a list of all files with their paths. For example:
    ///     \hello.txt
    ///     \folder1\test.txt
    ///     \folder1\blah.csv
    ///     \folder1\folder2\blahhhhh.svg
    /// </returns>
    protected IEnumerable<string> FindAllFiles(string path, string filter)
    {
        var list = new List<string>();
        IEnumerable<string>? collection = FileSystem?.GetFiles(path, filter);
        if (collection is not null)
        {
            list.AddRange(collection);
        }

        IEnumerable<string>? directories = FileSystem?.GetDirectories(path);
        if (directories is not null)
        {
            foreach (var directory in directories)
            {
                list.AddRange(FindAllFiles(directory, filter));
            }
        }

        return list;
    }

    protected string? GetFileContent(string? filename)
    {
        if (filename is null || FileSystem?.FileExists(filename) == false)
        {
            return null;
        }

        try
        {
            using Stream? stream = FileSystem?.OpenFile(filename);
            if (stream is not null)
            {
                using var reader = new StreamReader(stream, Encoding.UTF8, true);
                return reader.ReadToEnd();
            }
        }
        catch
        {
            return null; // deal with race conds
        }

        return null;
    }

    /// <summary>
    /// Replaces or creates the file at the specified <paramref name="filepath"/> with the provided content stream.
    /// </summary>
    /// <param name="filepath">The path of the file to write or overwrite.</param>
    /// <param name="content">A stream containing the new content for the file.</param>
    public void SetFileContent(string filepath, Stream content) => FileSystem?.AddFile(filepath, content, true);

    /// <summary>
    /// Returns the size of the specified file in bytes.
    /// </summary>
    /// <param name="filename">The name of the file whose size is to be retrieved.</param>
    /// <returns>
    /// The size of the file in bytes, or -1 if the file does not exist or an error occurs (such as a race condition).
    /// </returns>
    public long GetFileSize(string filename)
    {
        if (FileSystem?.FileExists(filename) == false)
        {
            return -1;
        }

        try
        {
            return FileSystem!.GetSize(filename);
        }
        catch
        {
            return -1; // deal with race conds
        }
    }

    #region Implementation of IRepository<TId,TEntity>

    /// <summary>
    /// Saves the specified entity to the file system. If the entity does not already exist, a new file is created; otherwise, the existing file is updated.
    /// </summary>
    /// <param name="entity">The entity to be saved to the file system.</param>
    public virtual void Save(TEntity entity)
    {
        if (FileSystem?.FileExists(entity.OriginalPath) == false)
        {
            PersistNewItem(entity);
        }
        else
        {
            PersistUpdatedItem(entity);
        }
    }

    /// <summary>
    /// Deletes the specified entity from the repository.
    /// This virtual method calls <see cref="PersistDeletedItem"/> to perform the deletion.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    public virtual void Delete(TEntity entity) => PersistDeletedItem(entity);

    /// <summary>
    /// Gets an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to get.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    public abstract TEntity? Get(TId? id);

    /// <summary>
    /// Retrieves the entities corresponding to the specified identifiers.
    /// </summary>
    /// <param name="ids">The identifiers of the entities to retrieve. If null or empty, no entities are returned.</param>
    /// <returns>An enumerable collection of entities matching the specified identifiers.</returns>
    public abstract IEnumerable<TEntity> GetMany(params TId[]? ids);

    /// <summary>
    /// Determines whether a file with the specified identifier exists.
    /// </summary>
    /// <param name="id">The identifier of the file to check for existence.</param>
    /// <returns>True if the file exists; otherwise, false.</returns>
    public virtual bool Exists(TId id) => FileSystem?.FileExists(id!.ToString()!) ?? false;

    #endregion

    #region Implementation of IUnitOfWorkRepository

    /// <summary>
    /// Persists a new entity item to the repository, adding it to the underlying storage.
    /// If the entity is a folder, it is persisted using folder-specific logic.
    /// </summary>
    /// <param name="entity">The entity to persist. If the entity is a folder, it will be handled accordingly.</param>
    public void PersistNewItem(IEntity entity)
    {
        // special case for folder
        if (entity is Folder folder)
        {
            PersistNewFolder(folder);
        }
        else
        {
            PersistNewItem((TEntity)entity);
        }
    }

    /// <summary>
    /// Persists changes to an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity with updated values to persist.</param>
    public void PersistUpdatedItem(IEntity entity) => PersistUpdatedItem((TEntity)entity);

    /// <summary>
    /// Persists the deletion of the specified entity. If the entity is a folder, it is handled by <see cref="PersistDeletedFolder"/>; otherwise, the entity is deleted using the generic deletion logic.
    /// </summary>
    /// <param name="entity">The entity to be deleted. If the entity is a folder, special deletion logic is applied.</param>
    public void PersistDeletedItem(IEntity entity)
    {
        // special case for folder
        if (entity is Folder folder)
        {
            PersistDeletedFolder(folder);
        }
        else
        {
            PersistDeletedItem((TEntity)entity);
        }
    }

    #endregion

    #region Abstract IUnitOfWorkRepository Methods

    protected virtual void PersistNewItem(TEntity entity)
    {
        if (entity.Content is null || FileSystem is null)
        {
            return;
        }

        using (Stream stream = GetContentStream(entity.Content))
        {
            FileSystem.AddFile(entity.Path, stream, true);
            entity.CreateDate = FileSystem.GetCreated(entity.Path).UtcDateTime;
            entity.UpdateDate = FileSystem.GetLastModified(entity.Path).UtcDateTime;

            // the id can be the hash
            entity.Id = entity.Path.GetHashCode();
            entity.Key = entity.Path.EncodeAsGuid();
            entity.VirtualPath = FileSystem?.GetUrl(entity.Path);
        }
    }

    protected virtual void PersistUpdatedItem(TEntity entity)
    {
        if (entity.Content is null || FileSystem is null)
        {
            return;
        }

        using (Stream stream = GetContentStream(entity.Content))
        {
            FileSystem.AddFile(entity.Path, stream, true);
            entity.CreateDate = FileSystem.GetCreated(entity.Path).UtcDateTime;
            entity.UpdateDate = FileSystem.GetLastModified(entity.Path).UtcDateTime;

            // the id can be the hash
            entity.Id = entity.Path.GetHashCode();
            entity.Key = entity.Path.EncodeAsGuid();
            entity.VirtualPath = FileSystem.GetUrl(entity.Path);
        }

        // now that the file has been written, we need to check if the path had been changed
        if (entity.Path.InvariantEquals(entity.OriginalPath) == false)
        {
            // delete the original file
            FileSystem?.DeleteFile(entity.OriginalPath);

            // reset the original path on the file
            entity.ResetOriginalPath();
        }
    }

    protected virtual void PersistDeletedItem(TEntity entity)
    {
        if (FileSystem?.FileExists(entity.Path) ?? false)
        {
            FileSystem.DeleteFile(entity.Path);
        }
    }

    #endregion
}
