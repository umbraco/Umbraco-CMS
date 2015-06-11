﻿using System.Collections.Generic;
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

        public virtual void AddFolder(string folderPath)
        {
            _work.RegisterAdded(new Folder(folderPath), this);
        }

        public virtual void DeleteFolder(string folderPath)
        {
            _work.RegisterRemoved(new Folder(folderPath), this);
        }

        #region Implementation of IRepository<TId,TEntity>

        public virtual void AddOrUpdate(TEntity entity)
        {
            if (FileSystem.FileExists(entity.Path) == false)
            {
                _work.RegisterAdded(entity, this);
            }
            else
            {
                _work.RegisterChanged(entity, this);
            }
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
            //special case for folder
            var folder = entity as Folder;
            if (folder != null)
            {
                PersistNewFolder(folder);
            }
            else
            {
                PersistNewItem((TEntity)entity);
            }
        }

        public void PersistUpdatedItem(IEntity entity)
        {
            PersistUpdatedItem((TEntity)entity);
        }

        public void PersistDeletedItem(IEntity entity)
        {
            //special case for folder
            var folder = entity as Folder;
            if (folder != null)
            {
                PersistDeletedFolder(folder);
            }
            else
            {
                PersistDeletedItem((TEntity)entity);
            }
        }

        #endregion

        internal virtual void PersistNewFolder(Folder entity)
        {
            _fileSystem.CreateFolder(entity.Path);
        }

        internal virtual void PersistDeletedFolder(Folder entity)
        {
            _fileSystem.DeleteDirectory(entity.Path);
        }

        #region Abstract IUnitOfWorkRepository Methods
        
        protected virtual void PersistNewItem(TEntity entity)
        {
            using (var stream = GetContentStream(entity.Content))
            {
                FileSystem.AddFile(entity.Path, stream, true);
                entity.CreateDate = FileSystem.GetCreated(entity.Path).UtcDateTime;
                entity.UpdateDate = FileSystem.GetLastModified(entity.Path).UtcDateTime;
                //the id can be the hash
                entity.Id = entity.Path.GetHashCode();
                entity.Key = entity.Path.EncodeAsGuid();
                entity.VirtualPath = FileSystem.GetUrl(entity.Path);
            }
        }

        protected virtual void PersistUpdatedItem(TEntity entity)
        {
            using (var stream = GetContentStream(entity.Content))
            {
                FileSystem.AddFile(entity.Path, stream, true);
                entity.CreateDate = FileSystem.GetCreated(entity.Path).UtcDateTime;
                entity.UpdateDate = FileSystem.GetLastModified(entity.Path).UtcDateTime;
                //the id can be the hash
                entity.Id = entity.Path.GetHashCode();
                entity.Key = entity.Path.EncodeAsGuid();
                entity.VirtualPath = FileSystem.GetUrl(entity.Path);
            }
        }

        protected virtual void PersistDeletedItem(TEntity entity)
        {
            if (_fileSystem.FileExists(entity.Path))
            {
                _fileSystem.DeleteFile(entity.Path);
            }
        }

        #endregion

        /// <summary>
        /// Gets a stream that is used to write to the file
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        protected virtual Stream GetContentStream(string content)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(content));
        }

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