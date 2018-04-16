using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LightInject;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    /// <summary>
    /// Represents the Script Repository
    /// </summary>
    internal class ScriptRepository : FileRepository<string, Script>, IScriptRepository
    {
        private readonly IContentSection _contentConfig;

        public ScriptRepository([Inject("ScriptFileSystem")] IFileSystem fileSystem, IContentSection contentConfig)
            : base(fileSystem)
        {
            _contentConfig = contentConfig ?? throw new ArgumentNullException(nameof(contentConfig));
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

            // reset dirty initial properties (U4-1946)
            script.ResetDirtyProperties(false);

            return script;
        }

        public override void Save(Script entity)
        {
            base.Save(entity);

            // ensure that from now on, content is lazy-loaded
            if (entity.GetFileContent == null)
                entity.GetFileContent = file => GetFileContent(file.OriginalPath);
        }

        public override IEnumerable<Script> GetMany(params string[] ids)
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

        #endregion
    }
}
