using System.Collections.Generic;
using System.IO;
using System.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    /// <summary>
    /// Represents the Stylesheet Repository
    /// </summary>
    internal class StylesheetRepository : FileRepository<string, Stylesheet>, IStylesheetRepository
    {
        public StylesheetRepository(IFileSystems fileSystems)
            : base(fileSystems.StylesheetsFileSystem)
        { }

        #region Overrides of FileRepository<string,Stylesheet>

        public override Stylesheet Get(string id)
        {
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
                    return null;
            } catch
            {
                return null;
            }

            // content will be lazy-loaded when required
            var created = FileSystem.GetCreated(path).UtcDateTime;
            var updated = FileSystem.GetLastModified(path).UtcDateTime;
            //var content = GetFileContent(path);

            var stylesheet = new Stylesheet(path, file => GetFileContent(file.OriginalPath))
            {
                //Content = content,
                Key = path.EncodeAsGuid(),
                CreateDate = created,
                UpdateDate = updated,
                Id = path.GetHashCode(),
                VirtualPath = FileSystem.GetUrl(path)
            };

            // reset dirty initial properties (U4-1946)
            stylesheet.ResetDirtyProperties(false);

            return stylesheet;

        }

        public override void Save(Stylesheet entity)
        {
            base.Save(entity);

            // ensure that from now on, content is lazy-loaded
            if (entity.GetFileContent == null)
                entity.GetFileContent = file => GetFileContent(file.OriginalPath);
        }

        public override IEnumerable<Stylesheet> GetMany(params string[] ids)
        {
            //ensure they are de-duplicated, easy win if people don't do this as this can cause many excess queries
            ids = ids
                .Select(x => StringExtensions.EnsureEndsWith(x, ".css"))
                .Distinct()
                .ToArray();

            if (ids.Any())
            {
                foreach (var id in ids)
                {
                    yield return Get(id);
                }
            }
            else
            {
                var files = FindAllFiles("", "*.css");
                foreach (var file in files)
                {
                    yield return Get(file);
                }
            }
        }

        /// <summary>
        /// Gets a list of all <see cref="Stylesheet"/> that exist at the relative path specified.
        /// </summary>
        /// <param name="rootPath">
        /// If null or not specified, will return the stylesheets at the root path relative to the IFileSystem
        /// </param>
        /// <returns></returns>
        public IEnumerable<Stylesheet> GetStylesheetsAtPath(string rootPath = null)
        {
            return FileSystem.GetFiles(rootPath ?? string.Empty, "*.css").Select(Get);
        }

        private static readonly List<string> ValidExtensions = new List<string> { "css" };

        public bool ValidateStylesheet(Stylesheet stylesheet)
        {
            // get full path
            string fullPath;
            try
            {
                // may throw for security reasons
                fullPath = FileSystem.GetFullPath(stylesheet.Path);
            }
            catch
            {
                return false;
            }

            // validate path and extension
            var validDir = SystemDirectories.Css;
            var isValidPath = IOHelper.VerifyEditPath(fullPath, validDir);
            var isValidExtension = IOHelper.VerifyFileExtension(stylesheet.Path, ValidExtensions);
            return isValidPath && isValidExtension;
        }

        public Stream GetFileContentStream(string filepath)
        {
            if (FileSystem.FileExists(filepath) == false) return null;

            try
            {
                return FileSystem.OpenFile(filepath);
            }
            catch
            {
                return null; // deal with race conds
            }
        }

        public void SetFileContent(string filepath, Stream content)
        {
            FileSystem.AddFile(filepath, content, true);
        }

        public new long GetFileSize(string filepath)
        {
            if (FileSystem.FileExists(filepath) == false) return -1;

            try
            {
                return FileSystem.GetSize(filepath);
            }
            catch
            {
                return -1; // deal with race conds
            }
        }

        #endregion
    }
}
