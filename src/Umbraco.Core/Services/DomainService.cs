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
    public class DomainService : RepositoryService, IDomainService
    {
        public DomainService(IDatabaseUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, logger, eventMessagesFactory)
        {
        }

        public bool Exists(string domainName)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IDomainRepository>();
                var exists = repo.Exists(domainName);
                uow.Complete();
                return exists;
            }
        }

        public Attempt<OperationStatus> Delete(IDomain domain)
        {
            var evtMsgs = EventMessagesFactory.Get();
            if (Deleting.IsRaisedEventCancelled(
                   new DeleteEventArgs<IDomain>(domain, evtMsgs),
                   this))
            {
                return OperationStatus.Attempt.Cancel(evtMsgs);
            }

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IDomainRepository>();
                repository.Delete(domain);
                uow.Complete();
            }

            var args = new DeleteEventArgs<IDomain>(domain, false, evtMsgs);
            Deleted.RaiseEvent(args, this);
            return OperationStatus.Attempt.Succeed(evtMsgs);
        }

        public IDomain GetByName(string name)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IDomainRepository>();
                var domain = repository.GetByName(name);
                uow.Complete();
                return domain;
            }
        }

        public IDomain GetById(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IDomainRepository>();
                var domain = repo.Get(id);
                uow.Complete();
                return domain;
            }
        }

        public IEnumerable<IDomain> GetAll(bool includeWildcards)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IDomainRepository>();
                var domains = repo.GetAll(includeWildcards);
                uow.Complete();
                return domains;
            }
        }

        public IEnumerable<IDomain> GetAssignedDomains(int contentId, bool includeWildcards)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IDomainRepository>();
                var domains = repo.GetAssignedDomains(contentId, includeWildcards);
                uow.Complete();
                return domains;
            }
        }

        public Attempt<OperationStatus> Save(IDomain domainEntity)
        {
            var evtMsgs = EventMessagesFactory.Get();
            if (Saving.IsRaisedEventCancelled(
                    new SaveEventArgs<IDomain>(domainEntity, evtMsgs),
                    this))
            {
                return OperationStatus.Attempt.Cancel(evtMsgs);
            }

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IDomainRepository>();
                repository.AddOrUpdate(domainEntity);
                uow.Complete();
            }

            Saved.RaiseEvent(new SaveEventArgs<IDomain>(domainEntity, false, evtMsgs), this);
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