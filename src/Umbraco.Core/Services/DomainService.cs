using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    public class DomainService : ScopeRepositoryService, IDomainService
    {
        public DomainService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, repositoryFactory, logger, eventMessagesFactory)
        {
        }

        public bool Exists(string domainName)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repo = RepositoryFactory.CreateDomainRepository(uow);
                return repo.Exists(domainName);
            } 
        }

        public Attempt<OperationStatus> Delete(IDomain domain)
        {
            var evtMsgs = EventMessagesFactory.Get();

            using (var uow = UowProvider.GetUnitOfWork())
            {
                var deleteEventArgs = new DeleteEventArgs<IDomain>(domain, evtMsgs);
                if (uow.Events.DispatchCancelable(Deleting, this, deleteEventArgs))
                {
                    uow.Commit();
                    return OperationStatus.Cancelled(evtMsgs);
                }

                var repository = RepositoryFactory.CreateDomainRepository(uow);
                repository.Delete(domain);
                uow.Commit();

                deleteEventArgs.CanCancel = false;
                uow.Events.Dispatch(Deleted, this, deleteEventArgs);
                return OperationStatus.Success(evtMsgs);
            }

            
        }

        public IDomain GetByName(string name)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateDomainRepository(uow);
                return repository.GetByName(name);
            }
        }

        public IDomain GetById(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
	            var repo = RepositoryFactory.CreateDomainRepository(uow);
	            return repo.Get(id);
            }
        }

        public IEnumerable<IDomain> GetAll(bool includeWildcards)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repo = RepositoryFactory.CreateDomainRepository(uow);
                return repo.GetAll(includeWildcards);
            }
        }

        public IEnumerable<IDomain> GetAssignedDomains(int contentId, bool includeWildcards)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repo = RepositoryFactory.CreateDomainRepository(uow);
                return repo.GetAssignedDomains(contentId, includeWildcards);
            }
        }

        public Attempt<OperationStatus> Save(IDomain domainEntity)
        {
            var evtMsgs = EventMessagesFactory.Get();

            using (var uow = UowProvider.GetUnitOfWork())
            {
                var saveEventArgs = new SaveEventArgs<IDomain>(domainEntity, evtMsgs);
                if (uow.Events.DispatchCancelable(Saving, this, saveEventArgs))
                {
                    uow.Commit();
                    return OperationStatus.Cancelled(evtMsgs);
                }

                var repository = RepositoryFactory.CreateDomainRepository(uow);
                repository.AddOrUpdate(domainEntity);
                uow.Commit();
                saveEventArgs.CanCancel = false;
                uow.Events.Dispatch(Saved, this, saveEventArgs);
                return OperationStatus.Success(evtMsgs);
            }

            
        }

        #region Event Handlers
        /// <summary>
        /// Occurs before Delete
        /// </summary>		
        public static event TypedEventHandler<IDomainService, DeleteEventArgs<IDomain>> Deleting;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IDomainService, DeleteEventArgs<IDomain>> Deleted;
      
        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<IDomainService, SaveEventArgs<IDomain>> Saving;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IDomainService, SaveEventArgs<IDomain>> Saved;

      
        #endregion
    }
}