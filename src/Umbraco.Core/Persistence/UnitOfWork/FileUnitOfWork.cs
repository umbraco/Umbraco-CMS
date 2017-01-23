using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Scoping;
using IsolationLevel = System.Data.IsolationLevel;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Represents the Unit of Work implementation for working with files
    /// </summary>
    internal class FileUnitOfWork : ScopeUnitOfWork
    {
        private Guid _key;

        public FileUnitOfWork(IScopeProvider scopeProvider, IsolationLevel isolationLevel = IsolationLevel.Unspecified) : base(scopeProvider, isolationLevel)
        {
            _key = Guid.NewGuid();
        }

        #region Implementation of IUnitOfWork        

        public override void Commit()
        {
            //NOTE: I'm leaving this in here for reference, but this is useless, transaction scope + Files doesn't do anything,
            // the closest you can get is transactional NTFS, but that requires distributed transaction coordinator and some other libs/wrappers,
            // plus MS has not deprecated it anyways. To do transactional IO we'd have to write this ourselves using temporary files and then 
            // on committing move them to their correct place.
            //using(var scope = new TransactionScope())
            //{
            //    // Commit the transaction
            //    scope.Complete();
            //}

            while (Operations.Count > 0)
            {
                var operation = Operations.Dequeue();
                switch (operation.Type)
                {
                    case TransactionType.Insert:
                        operation.Repository.PersistNewItem(operation.Entity);
                        break;
                    case TransactionType.Delete:
                        operation.Repository.PersistDeletedItem(operation.Entity);
                        break;
                    case TransactionType.Update:
                        operation.Repository.PersistUpdatedItem(operation.Entity);
                        break;
                }
            }

            // Clear everything
            Operations.Clear();
            _key = Guid.NewGuid();
        }

        public override object Key
        {
            get { return _key; }
        }

        #endregion
       
    }
}