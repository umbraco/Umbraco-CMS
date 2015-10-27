using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents the Script Repository
    /// </summary>
    internal class ScriptRepository : FileRepository<string, Script>, IScriptRepository
    {
        private readonly IContentSection _contentConfig;

        public ScriptRepository(IUnitOfWork work, IFileSystem fileSystem, IContentSection contentConfig)
			: base(work, fileSystem)
        {
            if (contentConfig == null) throw new ArgumentNullException("contentConfig");
            _contentConfig = contentConfig;
        }

        #region Implementation of IRepository<string,Script>

        public override Script Get(string id)
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

            var script = new Script(path, file => GetFileContent(file.OriginalPath))
            {
                //id can be the hash
                Id = path.GetHashCode(),
                Key = path.EncodeAsGuid(),
                //Content = content,
                CreateDate = created,
                UpdateDate = updated,
                VirtualPath = FileSystem.GetUrl(path)
            };

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            script.ResetDirtyProperties(false);

            return script;
        }

        public override void AddOrUpdate(Script entity)
        {
            base.AddOrUpdate(entity);

            // ensure that from now on, content is lazy-loaded
            if (entity.GetFileContent == null)
                entity.GetFileContent = file => GetFileContent(file.OriginalPath);
        }

        public override IEnumerable<Script> GetAll(params string[] ids)
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

        public bool ValidateScript(Script script)
        {
            // get full path
            string fullPath;
            try
            {
                // may throw for security reasons
                fullPath = FileSystem.GetFullPath(script.Path);
            }
            catch
            {
                return false;
            }

            // validate path & extension
            var validDir = SystemDirectories.Scripts;
            var isValidPath = IOHelper.VerifyEditPath(fullPath, validDir);
            var validExts = _contentConfig.ScriptFileTypes.ToList();
            var isValidExtension = IOHelper.VerifyFileExtension(script.Path, validExts);
            return isValidPath && isValidExtension;
        }

        #endregion
    }
}