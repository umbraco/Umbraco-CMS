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
    public class DomainService : RepositoryService, IDomainService
    {
        public DomainService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger)
            : base(provider, repositoryFactory, logger)
        {
        }

        public bool Exists(string domainName)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreateDomainRepository(uow))
            {
                return repo.Exists(domainName);
            } 
        }

        public void Delete(IDomain domain)
        {
            if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IDomain>(domain), this))
                return;

            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateDomainRepository(uow))
            {
                repository.Delete(domain);
                uow.Commit();               
            }

            var args = new DeleteEventArgs<IDomain>(domain, false);
            Deleted.RaiseEvent(args, this);
        }

        public IDomain GetByName(string name)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateDomainRepository(uow))
            {
                return repository.GetByName(name);
            }
        }

        public IDomain GetById(int id)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreateDomainRepository(uow))
            {
                return repo.Get(id);
            }
        }

        public IEnumerable<IDomain> GetAll(bool includeWildcards)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreateDomainRepository(uow))
            {
                return repo.GetAll(includeWildcards);
            }
        }

        public IEnumerable<IDomain> GetAssignedDomains(int contentId, bool includeWildcards)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreateDomainRepository(uow))
            {
                return repo.GetAssignedDomains(contentId, includeWildcards);
            }
        }

        public void Save(IDomain domainEntity, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IDomain>(domainEntity), this))
                    return;
            }

            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateDomainRepository(uow))
            {
                repository.AddOrUpdate(domainEntity);
                uow.Commit();
            }

            if (raiseEvents)
                Saved.RaiseEvent(new SaveEventArgs<IDomain>(domainEntity, false), this);
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