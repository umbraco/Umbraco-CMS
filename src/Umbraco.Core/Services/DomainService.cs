using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    public class DomainService : ScopeRepositoryService, IDomainService
    {
        public DomainService(IScopeUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, logger, eventMessagesFactory)
        {
        }

        public bool Exists(string domainName)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repo = uow.CreateRepository<IDomainRepository>();
                return repo.Exists(domainName);
            }
        }

        public Attempt<OperationStatus> Delete(IDomain domain)
        {
            var evtMsgs = EventMessagesFactory.Get();

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                if (uow.Events.DispatchCancelable(Deleting, this, new DeleteEventArgs<IDomain>(domain, evtMsgs)))
                {
                    uow.Complete();
                    return OperationStatus.Attempt.Cancel(evtMsgs);
                }

                var repository = uow.CreateRepository<IDomainRepository>();
                repository.Delete(domain);
                uow.Complete();

                var args = new DeleteEventArgs<IDomain>(domain, false, evtMsgs);
                uow.Events.Dispatch(Deleted, this, args);
            }

            return OperationStatus.Attempt.Succeed(evtMsgs);
        }

        public IDomain GetByName(string name)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IDomainRepository>();
                return repository.GetByName(name);
            }
        }

        public IDomain GetById(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repo = uow.CreateRepository<IDomainRepository>();
                return repo.Get(id);
            }
        }

        public IEnumerable<IDomain> GetAll(bool includeWildcards)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repo = uow.CreateRepository<IDomainRepository>();
                return repo.GetAll(includeWildcards);
            }
        }

        public IEnumerable<IDomain> GetAssignedDomains(int contentId, bool includeWildcards)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repo = uow.CreateRepository<IDomainRepository>();
                return repo.GetAssignedDomains(contentId, includeWildcards);
            }
        }

        public Attempt<OperationStatus> Save(IDomain domainEntity)
        {
            var evtMsgs = EventMessagesFactory.Get();

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                if (uow.Events.DispatchCancelable(Saving, this, new SaveEventArgs<IDomain>(domainEntity, evtMsgs)))
                {
                    uow.Complete();
                    return OperationStatus.Attempt.Cancel(evtMsgs);
                }

                var repository = uow.CreateRepository<IDomainRepository>();
                repository.AddOrUpdate(domainEntity);
                uow.Complete();

                uow.Events.Dispatch(Saved, this, new SaveEventArgs<IDomain>(domainEntity, false, evtMsgs));
            }

            return OperationStatus.Attempt.Succeed(evtMsgs);
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
