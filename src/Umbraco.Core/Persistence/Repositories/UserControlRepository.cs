using System.Collections.Generic;
using System.IO;
using System.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents the UserControl Repository
    /// </summary>
    internal class UserControlRepository : FileRepository<string, UserControl>, IUserControlRepository
    {
        public UserControlRepository(IUnitOfWork work, IFileSystem fileSystem)
            : base(work, fileSystem)
        {
        }

        public override UserControl Get(string id)
        {
            var path = FileSystem.GetRelativePath(id);

            path = path.EnsureEndsWith(".ascx");

            if (FileSystem.FileExists(path) == false)
                return null;

            var created = FileSystem.GetCreated(path).UtcDateTime;
            var updated = FileSystem.GetLastModified(path).UtcDateTime;

            var userControl = new UserControl(path, file => GetFileContent(file.OriginalPath))
            {
                Key = path.EncodeAsGuid(),
                CreateDate = created,
                UpdateDate = updated,
                Id = path.GetHashCode(),
                VirtualPath = FileSystem.GetUrl(path)
            };

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            userControl.ResetDirtyProperties(false);

            return userControl;
        }

        public override void AddOrUpdate(UserControl entity)
        {
            base.AddOrUpdate(entity);

            // ensure that from now on, content is lazy-loaded
            if (entity.GetFileContent == null)
                entity.GetFileContent = file => GetFileContent(file.OriginalPath);
        }

        public override IEnumerable<UserControl> GetAll(params string[] ids)
        {
            ids = ids
                .Select(x => StringExtensions.EnsureEndsWith(x, ".ascx"))
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
                var files = FindAllFiles("", "*.ascx");
                foreach (var file in files)
                {
                    yield return Get(file);
                }
            }
        }

        /// <summary>
        /// Gets a list of all <see cref="UserControl"/> that exist at the relative path specified. 
        /// </summary>
        /// <param name="rootPath">
        /// If null or not specified, will return the UserControl files at the root path relative to the IFileSystem
        /// </param>
        /// <returns></returns>
        public IEnumerable<UserControl> GetUserControlsAtPath(string rootPath = null)
        {
            return FileSystem.GetFiles(rootPath ?? string.Empty, "*.ascx").Select(Get);
        }

        private static readonly List<string> ValidExtensions = new List<string> { "ascx" };

        public bool ValidateUserControl(UserControl userControl)
        {
            // get full path
            string fullPath;
            try
            {
                // may throw for security reasons
                fullPath = FileSystem.GetFullPath(userControl.Path);
            }
            catch
            {
                return false;
            }

            // validate path and extension
            var validDir = SystemDirectories.UserControls;
            var isValidPath = IOHelper.VerifyEditPath(fullPath, validDir);
            var isValidExtension = IOHelper.VerifyFileExtension(userControl.Path, ValidExtensions);
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