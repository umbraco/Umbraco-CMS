using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents the Stylesheet Repository
    /// </summary>
    internal class StylesheetRepository : FileRepository<string, Stylesheet>, IStylesheetRepository
    {
        
        public StylesheetRepository(IUnitOfWork work, IFileSystem fileSystem)
            : base(work, fileSystem)
        {
        }

        #region Overrides of FileRepository<string,Stylesheet>

        public override Stylesheet Get(string id)
        {
            id = id.EnsureEndsWith(".css");

            if (FileSystem.FileExists(id) == false)
            {
                return null;
            }

            string content;

            using (var stream = FileSystem.OpenFile(id))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                content = reader.ReadToEnd();
            }

            var path = FileSystem.GetRelativePath(id);
            var created = FileSystem.GetCreated(path).UtcDateTime;
            var updated = FileSystem.GetLastModified(path).UtcDateTime;

            var stylesheet = new Stylesheet(path)
            {
                Content = content,
                Key = path.EncodeAsGuid(),
                CreateDate = created,
                UpdateDate = updated,
                Id = path.GetHashCode(),
                VirtualPath = FileSystem.GetUrl(id)
            };

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            stylesheet.ResetDirtyProperties(false);

            return stylesheet;

        }

        public override IEnumerable<Stylesheet> GetAll(params string[] ids)
        {
            //ensure they are de-duplicated, easy win if people don't do this as this can cause many excess queries
            ids = ids
                .Select(x => x.EnsureEndsWith(".css"))
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
            var dirs = SystemDirectories.Css;

            //Validate file
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
            var isValidPath = IOHelper.VerifyEditPath(fullPath, dirs.Split(','));

            //Validate extension
            var isValidExtension = IOHelper.VerifyFileExtension(stylesheet.Path, ValidExtensions);

            return isValidPath && isValidExtension;
        }

        #endregion
    }
}