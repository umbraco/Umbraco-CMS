using System;
using System.Collections.Generic;
using System.IO;
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
    internal class ScriptRepository : IScriptRepository
    {
        private IUnitOfWork _work;
        private readonly IFileSystem _fileSystem;

        public ScriptRepository(IUnitOfWork work)
        {
            _work = work;
            _fileSystem = FileSystemProviderManager.Current.GetFileSystemProvider("script");
        }

        /// <summary>
        /// Returns the Unit of Work added to the repository
        /// </summary>
        protected IUnitOfWork UnitOfWork
        {
            get { return _work; }
        }

        #region Implementation of IRepository<string,Script>

        public void AddOrUpdate(Script entity)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(entity.Content));
            _fileSystem.AddFile(entity.Name, stream, true);
        }

        public void Delete(Script entity)
        {
            if (_fileSystem.FileExists(entity.Name))
            {
                _fileSystem.DeleteFile(entity.Name);
            }
        }

        public Script Get(string id)
        {
            if(!_fileSystem.FileExists(id))
            {
                throw new Exception(string.Format("The file {0} was not found", id));
            }

            var stream = _fileSystem.OpenFile(id);
            byte[] bytes = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(bytes, 0, (int)stream.Length);
            var content = Encoding.UTF8.GetString(bytes);

            var path = _fileSystem.GetRelativePath(id);

            var script = new Script(path) {Content = content};
            return script;
        }

        public IEnumerable<Script> GetAll(params string[] ids)
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
                var files = _fileSystem.GetFiles("", "*");
                foreach (var file in files)
                {
                    yield return Get(file);
                }
            }
        }

        public bool Exists(string id)
        {
            return _fileSystem.FileExists(id);
        }

        public void SetUnitOfWork(IUnitOfWork work)
        {
            _work = work;
        }

        #endregion
    }
}