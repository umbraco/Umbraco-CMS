using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    /// <summary>
    /// Represents the Script Repository
    /// </summary>
    internal class ScriptRepository : FileRepository<string, IScript>, IScriptRepository
    {
        private readonly IIOHelper _ioHelper;
        private readonly GlobalSettings _globalSettings;

        public ScriptRepository(IFileSystems fileSystems, IIOHelper ioHelper, IOptions<GlobalSettings> globalSettings)
            : base(fileSystems.ScriptsFileSystem)
        {
            _ioHelper = ioHelper ?? throw new ArgumentNullException(nameof(ioHelper));
            _globalSettings = globalSettings.Value;
        }

        #region Implementation of IRepository<string,Script>

        public override IScript Get(string id)
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

        public override void Save(IScript entity)
        {
            // TODO: Casting :/ Review GetFileContent and it's usages, need to look into it later
            var script = (Script) entity;

            base.Save(script);

            // ensure that from now on, content is lazy-loaded
            if (script.GetFileContent == null)
                script.GetFileContent = file => GetFileContent(file.OriginalPath);
        }

        public override IEnumerable<IScript> GetMany(params string[] ids)
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

        public bool ValidateScript(IScript script)
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
            var validDir = _globalSettings.UmbracoScriptsPath;
            var isValidPath = _ioHelper.VerifyEditPath(fullPath, validDir);
            var validExts = new[] {"js"};
            var isValidExtension = _ioHelper.VerifyFileExtension(script.Path, validExts);
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
