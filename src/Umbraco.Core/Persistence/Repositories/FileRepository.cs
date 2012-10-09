using System.Collections.Generic;
using System.IO;
using System.Text;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal abstract class FileRepository<TId, TEntity> : IRepository<TId, TEntity> 
        where TEntity : IFile
    {
        private IUnitOfWork _work;
        private readonly IFileSystem _fileSystem;

        protected FileRepository(IUnitOfWork work, IFileSystem fileSystem)
        {
            _work = work;
            _fileSystem = fileSystem;
        }

        /// <summary>
        /// Returns the Unit of Work added to the repository
        /// </summary>
        protected IUnitOfWork UnitOfWork
        {
            get { return _work; }
        }

        protected IFileSystem FileSystem
        {
            get { return _fileSystem; }
        }

        #region Implementation of IRepository<TId,TEntity>

        public virtual void AddOrUpdate(TEntity entity)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(entity.Content));
            FileSystem.AddFile(entity.Name, stream, true);
        }

        public virtual void Delete(TEntity entity)
        {
            if (_fileSystem.FileExists(entity.Name))
            {
                _fileSystem.DeleteFile(entity.Name);
            }
            else if(_fileSystem.FileExists(entity.Path))
            {
                _fileSystem.DeleteFile(entity.Path);
            }
        }

        public abstract TEntity Get(TId id);

        public abstract IEnumerable<TEntity> GetAll(params TId[] ids);

        public virtual bool Exists(TId id)
        {
            return _fileSystem.FileExists(id.ToString());
        }

        public void SetUnitOfWork(IUnitOfWork work)
        {
            _work = work;
        }

        #endregion
    }
}