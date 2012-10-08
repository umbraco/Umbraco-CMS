using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Serialization;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents the Macro Repository
    /// </summary>
    internal class MacroRepository : RepositoryBase<string, IMacro>, IMacroRepository
    {
        private IFileSystem _fileSystem;
        private SerializationService _serializationService;

        public MacroRepository(IUnitOfWork work) : base(work)
        {
            EnsureDependencies();
        }

        public MacroRepository(IUnitOfWork work, IRepositoryCacheProvider cache) : base(work, cache)
        {
            EnsureDependencies();
        }

        private void EnsureDependencies()
        {
            _fileSystem = FileSystemProviderManager.Current.GetFileSystemProvider("macro");
            var serviceStackSerializer = new ServiceStackJsonSerializer();
            _serializationService = new SerializationService(serviceStackSerializer);
        }

        #region Overrides of RepositoryBase<string,IMacro>

        protected override IMacro PerformGet(string id)
        {
            var name = string.Concat(id, ".macro");
            Stream file = _fileSystem.OpenFile(name);
            var o = _serializationService.FromStream(file, typeof(IMacro));
            return o as IMacro;
        }

        protected override IEnumerable<IMacro> PerformGetAll(params string[] ids)
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
                var files = _fileSystem.GetFiles("", "*.macro");
                foreach (var file in files)
                {
                    yield return Get(file.Replace(".macro", ""));
                }
            }
        }

        protected override IEnumerable<IMacro> PerformGetByQuery(IQuery<IMacro> query)
        {
            throw new NotImplementedException();
        }

        protected override bool PerformExists(string id)
        {
            var name = string.Concat(id, ".macro");
            return _fileSystem.FileExists(name);
        }

        protected override int PerformCount(IQuery<IMacro> query)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Overrides of IUnitOfWorkRepository

        protected override void PersistNewItem(IMacro entity)
        {
            ((Macro)entity).AddingEntity();

            var name = string.Concat(entity.Alias, ".macro");
            var json = _serializationService.ToStream(entity);
            _fileSystem.AddFile(name, json.ResultStream, true);
        }

        protected override void PersistUpdatedItem(IMacro entity)
        {
            ((Macro)entity).UpdatingEntity();

            var name = string.Concat(entity.Alias, ".macro");
            var json = _serializationService.ToStream(entity);
            _fileSystem.AddFile(name, json.ResultStream, true);
        }

        protected override void PersistDeletedItem(IMacro entity)
        {
            var name = string.Concat(entity.Alias, ".macro");
            if (_fileSystem.FileExists(name))
            {
                _fileSystem.DeleteFile(name);
            }
        }

        #endregion
    }
}