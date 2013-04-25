using System.Collections.Generic;
using System.IO;
using System.Text;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal abstract class FileRepository<TId, TEntity> : DisposableObject, IUnitOfWorkRepository, IRepository<TId, TEntity> 
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
            _work.RegisterAdded(entity, this);
        }

        public virtual void Delete(TEntity entity)
        {
            if (_work != null)
            {
                _work.RegisterRemoved(entity, this);
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

        #region Implementation of IUnitOfWorkRepository

        public void PersistNewItem(IEntity entity)
        {
            PersistNewItem((TEntity)entity);
        }

        public void PersistUpdatedItem(IEntity entity)
        {
            PersistUpdatedItem((TEntity)entity);
        }

        public void PersistDeletedItem(IEntity entity)
        {
            PersistDeletedItem((TEntity)entity);
        }

        #endregion

        #region Abstract IUnitOfWorkRepository Methods

        protected virtual void PersistNewItem(TEntity entity)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(entity.Content));
            FileSystem.AddFile(entity.Name, stream, true);
        }

        protected virtual void PersistUpdatedItem(TEntity entity)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(entity.Content));
            FileSystem.AddFile(entity.Name, stream, true);
        }

        protected virtual void PersistDeletedItem(TEntity entity)
        {
            if (_fileSystem.FileExists(entity.Name))
            {
                _fileSystem.DeleteFile(entity.Name);
            }
            else if (_fileSystem.FileExists(entity.Path))
            {
                _fileSystem.DeleteFile(entity.Path);
            }
        }

        #endregion

        protected IEnumerable<string> FindAllFiles(string path)
        {
            var list = new List<string>();
            list.AddRange(FileSystem.GetFiles(path, "*"));

            var directories = FileSystem.GetDirectories(path);
            foreach (var directory in directories)
            {
                list.AddRange(FindAllFiles(directory));
            }

            return list;
        }

		/// <summary>
		/// Dispose any disposable properties
		/// </summary>
		/// <remarks>
		/// Dispose the unit of work
		/// </remarks>
		protected override void DisposeResources()
		{
			_work.DisposeIfDisposable();
		}
    }
}