using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services.Implement
{
    public class DomainService : ScopeRepositoryService, IDomainService
    {
        private readonly IDomainRepository _domainRepository;

        public DomainService(IScopeProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory,
            IDomainRepository domainRepository)
            : base(provider, logger, eventMessagesFactory)
        {
            _domainRepository = domainRepository;
        }

        public bool Exists(string domainName)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _domainRepository.Exists(domainName);
            }
        }

        public Attempt<OperationResult> Delete(IDomain domain)
        {
            var evtMsgs = EventMessagesFactory.Get();

            using (var scope = ScopeProvider.CreateScope())
            {
                var deleteEventArgs = new DeleteEventArgs<IDomain>(domain, evtMsgs);
                if (scope.Events.DispatchCancelable(Deleting, this, deleteEventArgs))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Cancel(evtMsgs);
                }

                _domainRepository.Delete(domain);
                scope.Complete();

                deleteEventArgs.CanCancel = false;
                scope.Events.Dispatch(Deleted, this, deleteEventArgs);
            }

            return OperationResult.Attempt.Succeed(evtMsgs);
        }

        public IDomain GetByName(string name)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _domainRepository.GetByName(name);
            }
        }

        public IDomain GetById(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _domainRepository.Get(id);
            }
        }

        public IEnumerable<IDomain> GetAll(bool includeWildcards)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _domainRepository.GetAll(includeWildcards);
            }
        }

        public IEnumerable<IDomain> GetAssignedDomains(int contentId, bool includeWildcards)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _domainRepository.GetAssignedDomains(contentId, includeWildcards);
            }
        }

        public Attempt<OperationResult> Save(IDomain domainEntity)
        {
            var evtMsgs = EventMessagesFactory.Get();

            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<IDomain>(domainEntity, evtMsgs);
                if (scope.Events.DispatchCancelable(Saving, this, saveEventArgs))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Cancel(evtMsgs);
                }

                _domainRepository.Save(domainEntity);
                scope.Complete();
                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(Saved, this, saveEventArgs);
            }

            return OperationResult.Attempt.Succeed(evtMsgs);
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
