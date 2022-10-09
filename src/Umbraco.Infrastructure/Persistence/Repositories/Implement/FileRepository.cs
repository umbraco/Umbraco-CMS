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

    public virtual void AddFolder(string folderPath) => PersistNewItem(new Folder(folderPath));

    public virtual void DeleteFolder(string folderPath) => PersistDeletedItem(new Folder(folderPath));

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

    public void SetFileContent(string filepath, Stream content) => FileSystem?.AddFile(filepath, content, true);

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

    public virtual void Delete(TEntity entity) => PersistDeletedItem(entity);

    public abstract TEntity? Get(TId? id);

    public abstract IEnumerable<TEntity> GetMany(params TId[]? ids);

    public virtual bool Exists(TId id) => FileSystem?.FileExists(id!.ToString()!) ?? false;

    #endregion

    #region Implementation of IUnitOfWorkRepository

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

    public void PersistUpdatedItem(IEntity entity) => PersistUpdatedItem((TEntity)entity);

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
