using System.Text;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
/// Provides methods to manage and retrieve partial view items from a file system.
/// </summary>
internal sealed class PartialViewRepository : FileRepository<string, IPartialView>, IPartialViewRepository
{
    private readonly IOptionsMonitor<RuntimeSettings> _runtimeSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartialViewRepository"/> class.
    /// </summary>
    /// <param name="fileSystems">An instance of <see cref="FileSystems"/> that provides access to the file systems used by the repository.</param>

    /// <summary>
    /// Initializes a new instance of the <see cref="PartialViewRepository"/> class.
    /// </summary>
    public PartialViewRepository(FileSystems fileSystems, IOptionsMonitor<RuntimeSettings> runtimeSettings)
        : base(fileSystems.PartialViewsFileSystem) => _runtimeSettings = runtimeSettings;

    /// <summary>
    /// Retrieves a partial view from the file system by its relative identifier.
    /// </summary>
    /// <param name="id">The relative identifier or path of the partial view to retrieve. If <c>null</c>, no view is returned.</param>
    /// <returns>The <see cref="IPartialView"/> instance if a matching partial view exists; otherwise, <c>null</c>.</returns>
    public override IPartialView? Get(string? id)
    {
        if (FileSystem is null)
        {
            return null;
        }

        // get the relative path within the filesystem
        // (though... id should be relative already)
        var path = FileSystem.GetRelativePath(id!);

        if (FileSystem.FileExists(path) == false)
        {
            return null;
        }

        // content will be lazy-loaded when required
        DateTime created = FileSystem.GetCreated(path).UtcDateTime;
        DateTime updated = FileSystem.GetLastModified(path).UtcDateTime;

        // var content = GetFileContent(path);
        var view = new PartialView(path, file => GetFileContent(file.OriginalPath))
        {
            // id can be the hash
            Id = path.GetHashCode(),
            Key = path.EncodeAsGuid(),

            // Content = content,
            CreateDate = created,
            UpdateDate = updated,
            VirtualPath = FileSystem.GetUrl(id),
        };

        // reset dirty initial properties (U4-1946)
        view.ResetDirtyProperties(false);

        return view;
    }

    /// <summary>
    /// Saves the specified <see cref="IPartialView"/> entity to the data store.
    /// Ensures that the partial view's content is set up for lazy loading after saving.
    /// </summary>
    /// <param name="entity">The partial view entity to save.</param>
    public override void Save(IPartialView entity)
    {
        base.Save(entity);

        // ensure that from now on, content is lazy-loaded
        if (entity is PartialView partialView && partialView.GetFileContent == null)
        {
            partialView.GetFileContent = file => GetFileContent(file.OriginalPath);
        }
    }

    /// <summary>
    /// Retrieves multiple partial views by their unique identifiers.
    /// If no IDs are specified, all available partial views are returned.
    /// Duplicate IDs are ignored.
    /// </summary>
    /// <param name="ids">An optional array of partial view IDs to retrieve. If null or empty, all partial views are returned.</param>
    /// <returns>An enumerable collection of <see cref="Umbraco.Cms.Core.Models.IPartialView"/> instances corresponding to the specified IDs, or all partial views if no IDs are provided.</returns>
    public override IEnumerable<IPartialView> GetMany(params string[]? ids)
    {
        // ensure they are de-duplicated, easy win if people don't do this as this can cause many excess queries
        ids = ids?.Distinct().ToArray();

        if (ids?.Any() ?? false)
        {
            foreach (var id in ids)
            {
                IPartialView? partialView = Get(id);
                if (partialView is not null)
                {
                    yield return partialView;
                }
            }
        }
        else
        {
            IEnumerable<string> files = FindAllFiles(string.Empty, "*.*");
            foreach (var file in files)
            {
                IPartialView? partialView = Get(file);
                if (partialView is not null)
                {
                    yield return partialView;
                }
            }
        }
    }

    /// <summary>
    /// Returns a stream for reading the content of the file at the specified <paramref name="filepath"/>.
    /// </summary>
    /// <param name="filepath">The path of the file to read from the file system.</param>
    /// <returns>A <see cref="Stream"/> containing the file's content, or <see cref="Stream.Null"/> if the file does not exist or cannot be opened.</returns>
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

    /// <summary>
    /// Sets the content of the specified file by writing the provided stream to the given file path, overwriting any existing content.
    /// </summary>
    /// <param name="filepath">The path of the file to which the content will be written.</param>
    /// <param name="content">A stream containing the content to write to the file.</param>
    public void SetFileContent(string filepath, Stream content) => FileSystem?.AddFile(filepath, content, true);

    /// <summary>
    ///     Persists a new partial view item, but only when not in production runtime mode.
    /// </summary>
    /// <param name="entity">The partial view entity to persist.</param>
    protected override void PersistNewItem(IPartialView entity)
    {
        // Only save file when not in production runtime mode.
        if (_runtimeSettings.CurrentValue.Mode == RuntimeMode.Production)
        {
            return;
        }

        base.PersistNewItem(entity);
    }

    /// <summary>
    ///     Persists an updated partial view item, but only when not in production runtime mode.
    /// </summary>
    /// <param name="entity">The partial view entity to persist.</param>
    protected override void PersistUpdatedItem(IPartialView entity)
    {
        // Only save file when not in production runtime mode.
        if (_runtimeSettings.CurrentValue.Mode == RuntimeMode.Production)
        {
            return;
        }

        base.PersistUpdatedItem(entity);
    }

    /// <summary>
    ///     Gets a stream that is used to write to the file.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    /// <remarks>
    ///     This ensures the stream includes a utf8 BOM.
    /// </remarks>
    protected override Stream GetContentStream(string content)
    {
        var data = Encoding.UTF8.GetBytes(content);
        var withBom = Encoding.UTF8.GetPreamble().Concat(data).ToArray();
        return new MemoryStream(withBom);
    }
}
