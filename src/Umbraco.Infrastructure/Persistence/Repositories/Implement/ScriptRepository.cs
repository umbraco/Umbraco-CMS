using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents the Script Repository
/// </summary>
internal sealed class ScriptRepository : FileRepository<string, IScript>, IScriptRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptRepository"/> class, which manages script files in the specified file systems.
    /// </summary>
    /// <param name="fileSystems">An instance of <see cref="FileSystems"/> that provides access to the file systems used by the repository.</param>
    public ScriptRepository(FileSystems fileSystems)
        : base(fileSystems.ScriptsFileSystem)
    {
    }

    /// <summary>
    /// Retrieves a script from the file system by its identifier.
    /// </summary>
    /// <param name="id">The relative identifier or path of the script to retrieve. If <c>null</c>, no script is returned.</param>
    /// <returns>The <see cref="IScript"/> instance if found; otherwise, <c>null</c>.</returns>
    public override IScript? Get(string? id)
    {
        if (id is null || FileSystem is null)
        {
            return null;
        }

        // get the relative path within the filesystem
        // (though... id should be relative already)
        var path = FileSystem.GetRelativePath(id);

        if (FileSystem.FileExists(path) == false)
        {
            return null;
        }

        // content will be lazy-loaded when required
        DateTime created = FileSystem.GetCreated(path).UtcDateTime;
        DateTime updated = FileSystem.GetLastModified(path).UtcDateTime;

        var script = new Script(path, file => GetFileContent(file.OriginalPath))
        {
            // id can be the hash
            Id = path.GetHashCode(),
            Key = path.EncodeAsGuid(),

            // Content = content,
            CreateDate = created,
            UpdateDate = updated,
            VirtualPath = FileSystem.GetUrl(path),
        };

        // reset dirty initial properties (U4-1946)
        script.ResetDirtyProperties(false);

        return script;
    }

    /// <summary>
    /// Saves the specified <see cref="IScript"/> entity to the script repository.
    /// </summary>
    /// <param name="entity">The script entity to save.</param>
    public override void Save(IScript entity)
    {
        // TODO: Casting :/ Review GetFileContent and it's usages, need to look into it later
        var script = (Script)entity;

        base.Save(script);

        // ensure that from now on, content is lazy-loaded
        if (script.GetFileContent == null)
        {
            script.GetFileContent = file => GetFileContent(file.OriginalPath);
        }
    }

    /// <summary>
    /// Retrieves multiple scripts by their identifiers. If no identifiers are specified, returns all available scripts.
    /// </summary>
    /// <param name="ids">The identifiers of the scripts to retrieve. If null or empty, all scripts will be returned.</param>
    /// <returns>An enumerable collection of <see cref="IScript"/> objects matching the specified identifiers, or all scripts if no identifiers are provided.</returns>
    public override IEnumerable<IScript> GetMany(params string[]? ids)
    {
        // ensure they are de-duplicated, easy win if people don't do this as this can cause many excess queries
        ids = ids?.Distinct().ToArray();

        if (ids?.Any() ?? false)
        {
            foreach (var id in ids)
            {
                IScript? script = Get(id);
                if (script is not null)
                {
                    yield return script;
                }
            }
        }
        else
        {
            IEnumerable<string> files = FindAllFiles(string.Empty, "*.*");
            foreach (var file in files)
            {
                IScript? script = Get(file);
                if (script is not null)
                {
                    yield return script;
                }
            }
        }
    }
}
