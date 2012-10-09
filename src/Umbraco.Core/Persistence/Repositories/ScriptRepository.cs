using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public ScriptRepository(IUnitOfWork work)
            : base(work, FileSystemProviderManager.Current.GetFileSystemProvider("scripts"))
        {
        }

        #region Implementation of IRepository<string,Script>

        public override Script Get(string id)
        {
            if(!FileSystem.FileExists(id))
            {
                throw new Exception(string.Format("The file {0} was not found", id));
            }

            var stream = FileSystem.OpenFile(id);
            byte[] bytes = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(bytes, 0, (int)stream.Length);
            var content = Encoding.UTF8.GetString(bytes);

            var path = FileSystem.GetRelativePath(id);

            var script = new Script(path) {Content = content};
            return script;
        }

        public override IEnumerable<Script> GetAll(params string[] ids)
        {
            if (ids.Any())
            {
                foreach (var id in ids)
                {
                    yield return Get(id);
                }
            }
            else
            {
                var files = FileSystem.GetFiles("", "*");
                foreach (var file in files)
                {
                    yield return Get(file);
                }
            }
        }

        #endregion
    }
}