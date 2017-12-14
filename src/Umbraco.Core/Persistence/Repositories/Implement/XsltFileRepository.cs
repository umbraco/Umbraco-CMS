using System.Collections.Generic;
using System.IO;
using System.Linq;
using LightInject;
using Umbraco.Core.IO;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    /// <summary>
    /// Represents the XsltFile Repository
    /// </summary>
    internal class XsltFileRepository : FileRepository<string, XsltFile>, IXsltFileRepository
    {
        public XsltFileRepository([Inject("XsltFileSystem")] IFileSystem fileSystem)
            : base(fileSystem)
        { }

        public override XsltFile Get(string id)
        {
            var path = FileSystem.GetRelativePath(id);

            path = path.EnsureEndsWith(".xslt");

            if (FileSystem.FileExists(path) == false)
                return null;

            var created = FileSystem.GetCreated(path).UtcDateTime;
            var updated = FileSystem.GetLastModified(path).UtcDateTime;

            var xsltFile = new XsltFile(path, file => GetFileContent(file.OriginalPath))
            {
                Key = path.EncodeAsGuid(),
                CreateDate = created,
                UpdateDate = updated,
                Id = path.GetHashCode(),
                VirtualPath = FileSystem.GetUrl(path)
            };

            // reset dirty initial properties (U4-1946)
            xsltFile.ResetDirtyProperties(false);

            return xsltFile;
        }

        public override void Save(XsltFile entity)
        {
            base.Save(entity);

            // ensure that from now on, content is lazy-loaded
            if (entity.GetFileContent == null)
                entity.GetFileContent = file => GetFileContent(file.OriginalPath);
        }

        public override IEnumerable<XsltFile> GetMany(params string[] ids)
        {
            ids = ids
                .Select(x => StringExtensions.EnsureEndsWith(x, ".xslt"))
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
                var files = FindAllFiles("", "*.xslt");
                foreach (var file in files)
                {
                    yield return Get(file);
                }
            }
        }

        /// <summary>
        /// Gets a list of all <see cref="XsltFile"/> that exist at the relative path specified.
        /// </summary>
        /// <param name="rootPath">
        /// If null or not specified, will return the XSLT files at the root path relative to the IFileSystem
        /// </param>
        /// <returns></returns>
        public IEnumerable<XsltFile> GetXsltFilesAtPath(string rootPath = null)
        {
            return FileSystem.GetFiles(rootPath ?? string.Empty, "*.xslt").Select(Get);
        }

        private static readonly List<string> ValidExtensions = new List<string> { "xslt" };

        public bool ValidateXsltFile(XsltFile xsltFile)
        {
            // get full path
            string fullPath;
            try
            {
                // may throw for security reasons
                fullPath = FileSystem.GetFullPath(xsltFile.Path);
            }
            catch
            {
                return false;
            }

            // validate path and extension
            var validDir = SystemDirectories.Xslt;
            var isValidPath = IOHelper.VerifyEditPath(fullPath, validDir);
            var isValidExtension = IOHelper.VerifyFileExtension(xsltFile.Path, ValidExtensions);
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
    }
}
