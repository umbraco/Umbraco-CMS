using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents the Stylesheet Repository
/// </summary>
internal class StylesheetRepository : FileRepository<string, IStylesheet>, IStylesheetRepository
{
    public StylesheetRepository(FileSystems fileSystems)
        : base(fileSystems.StylesheetsFileSystem)
    {
    }

    #region Overrides of FileRepository<string,Stylesheet>

    public override IStylesheet? Get(string? id)
    {
        if (id is null || FileSystem is null)
        {
            return null;
        }

        // get the relative path within the filesystem
        // (though... id should be relative already)
        var path = FileSystem.GetRelativePath(id);

        path = path.EnsureEndsWith(".css");

        // if the css directory is changed, references to the old path can still exist (ie in RTE config)
        // these old references will throw an error, which breaks the RTE
        // try-catch here makes the request fail silently, and allows RTE to load correctly
        try
        {
            if (FileSystem.FileExists(path) == false)
            {
                return null;
            }
        }
        catch
        {
            return null;
        }

        // content will be lazy-loaded when required
        DateTime created = FileSystem.GetCreated(path).UtcDateTime;
        DateTime updated = FileSystem.GetLastModified(path).UtcDateTime;

        // var content = GetFileContent(path);
        var stylesheet = new Stylesheet(path, file => GetFileContent(file.OriginalPath))
        {
            // Content = content,
            Key = path.EncodeAsGuid(),
            CreateDate = created,
            UpdateDate = updated,
            Id = path.GetHashCode(),
            VirtualPath = FileSystem.GetUrl(path),
        };

        // reset dirty initial properties (U4-1946)
        stylesheet.ResetDirtyProperties(false);

        return stylesheet;
    }

    public override void Save(IStylesheet entity)
    {
        // TODO: Casting :/ Review GetFileContent and it's usages, need to look into it later
        var stylesheet = (Stylesheet)entity;

        base.Save(stylesheet);

        // ensure that from now on, content is lazy-loaded
        if (stylesheet.GetFileContent == null)
        {
            stylesheet.GetFileContent = file => GetFileContent(file.OriginalPath);
        }
    }

    public override IEnumerable<IStylesheet> GetMany(params string[]? ids)
    {
        // ensure they are de-duplicated, easy win if people don't do this as this can cause many excess queries
        ids = ids?
            .Select(x => x.EnsureEndsWith(".css"))
            .Distinct()
            .ToArray();

        if (ids?.Any() ?? false)
        {
            foreach (var id in ids)
            {
                IStylesheet? stylesheet = Get(id);
                if (stylesheet is not null)
                {
                    yield return stylesheet;
                }
            }
        }
        else
        {
            IEnumerable<string> files = FindAllFiles(string.Empty, "*.css");
            foreach (var file in files)
            {
                IStylesheet? stylesheet = Get(file);
                if (stylesheet is not null)
                {
                    yield return stylesheet;
                }
            }
        }
    }

    #endregion
}
