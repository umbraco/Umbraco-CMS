using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents the Script Repository
/// </summary>
internal class ScriptRepository : FileRepository<string, IScript>, IScriptRepository
{
    public ScriptRepository(FileSystems fileSystems)
        : base(fileSystems.ScriptsFileSystem)
    {
    }

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
