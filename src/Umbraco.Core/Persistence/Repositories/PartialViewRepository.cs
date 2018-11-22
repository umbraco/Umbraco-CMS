using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal class PartialViewRepository : FileRepository<string, IPartialView>, IPartialViewRepository
    {
        public PartialViewRepository(IUnitOfWork work, IFileSystem fileSystem) 
            : base(work, fileSystem)
        { }

        protected virtual PartialViewType ViewType { get { return PartialViewType.PartialView; } }

        public override IPartialView Get(string id)
        {
            // get the relative path within the filesystem
            // (though... id should be relative already)
            var path = FileSystem.GetRelativePath(id);

            if (FileSystem.FileExists(path) == false)
                return null;

            // content will be lazy-loaded when required
            var created = FileSystem.GetCreated(path).UtcDateTime;
            var updated = FileSystem.GetLastModified(path).UtcDateTime;
            //var content = GetFileContent(path);

            var view = new PartialView(ViewType, path, file => GetFileContent(file.OriginalPath))
            {
                //id can be the hash
                Id = path.GetHashCode(),
                Key = path.EncodeAsGuid(),
                //Content = content,
                CreateDate = created,
                UpdateDate = updated,
                VirtualPath = FileSystem.GetUrl(id)
            };

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            view.ResetDirtyProperties(false);

            return view;
        }

        public override void AddOrUpdate(IPartialView entity)
        {
            var partialView = entity as PartialView;
            if (partialView != null)
                partialView.ViewType = ViewType;

            base.AddOrUpdate(entity);

            // ensure that from now on, content is lazy-loaded
            if (partialView != null && partialView.GetFileContent == null)
                partialView.GetFileContent = file => GetFileContent(file.OriginalPath);
        }

        public override IEnumerable<IPartialView> GetAll(params string[] ids)
        {
            //ensure they are de-duplicated, easy win if people don't do this as this can cause many excess queries
            ids = ids.Distinct().ToArray();

            if (ids.Any())
            {
                foreach (var id in ids)
                {
                    yield return Get(id);
                }
            }
            else
            {
                var files = FindAllFiles("", "*.*");
                foreach (var file in files)
                {
                    yield return Get(file);
                }
            }
        }

        private static readonly List<string> ValidExtensions = new List<string> { "cshtml" };

        public virtual bool ValidatePartialView(IPartialView partialView)
        {
            // get full path
            string fullPath;
            try
            {
                // may throw for security reasons
                fullPath = FileSystem.GetFullPath(partialView.Path);
            }
            catch
            {
                return false;
            }

            // validate path & extension
            var validDir = SystemDirectories.MvcViews;
            var isValidPath = IOHelper.VerifyEditPath(fullPath, validDir);
            var isValidExtension = IOHelper.VerifyFileExtension(fullPath, ValidExtensions);
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

        /// <summary>
        /// Gets a stream that is used to write to the file
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        /// <remarks>
        /// This ensures the stream includes a utf8 BOM
        /// </remarks>
        protected override Stream GetContentStream(string content)
        {
            var data = Encoding.UTF8.GetBytes(content);
            var withBom = Encoding.UTF8.GetPreamble().Concat(data).ToArray();
            return new MemoryStream(withBom);
        }
    }
}