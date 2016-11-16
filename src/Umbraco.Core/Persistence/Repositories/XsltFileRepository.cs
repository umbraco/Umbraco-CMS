using System.Collections.Generic;
using System.IO;
using System.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents the XsltFile Repository
    /// </summary>
    internal class XsltFileRepository : FileRepository<string, XsltFile>, IXsltFileRepository
    {
        public XsltFileRepository(IUnitOfWork work, IFileSystem fileSystem)
            : base(work, fileSystem)
        {
        }
        
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

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            xsltFile.ResetDirtyProperties(false);

            return xsltFile;
        }

        public override void AddOrUpdate(XsltFile entity)
        {
            base.AddOrUpdate(entity);

            // ensure that from now on, content is lazy-loaded
            if (entity.GetFileContent == null)
                entity.GetFileContent = file => GetFileContent(file.OriginalPath);
        }

        public override IEnumerable<XsltFile> GetAll(params string[] ids)
        {
            ids = ids
                .Select(x => x.EnsureEndsWith(".xslt"))
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