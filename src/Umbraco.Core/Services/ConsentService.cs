using System;
using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Implements <see cref="IContentService"/>.
    /// </summary>
    internal class ConsentService : ScopeRepositoryService, IConsentService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentService"/> class.
        /// </summary>
        public ConsentService(IScopeUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, repositoryFactory, logger, eventMessagesFactory)
        { }

        /// <inheritdoc />
        public IConsent Get(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateConsentRepository(uow);
                return repository.Get(id);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IConsent> GetBySource(Udi source, string actionType = null)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateConsentRepository(uow);
                var query = new Query<IConsent>().Where(x => x.Source == source);
                if (string.IsNullOrWhiteSpace(actionType) == false)
                    query = query.Where(x => x.ActionType == actionType);
                return repository.GetByQuery(query);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IConsent> GetByAction(Udi action)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateConsentRepository(uow);
                var query = new Query<IConsent>().Where(x => x.Action == action);
                return repository.GetByQuery(query);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IConsent> GetByActionType(string actionType)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateConsentRepository(uow);
                var query = new Query<IConsent>().Where(x => x.ActionType == actionType);
                return repository.GetByQuery(query);
            }
        }

        /// <inheritdoc />
        public void Save(IConsent consent)
        {
            try
            {
                using (var uow = UowProvider.GetUnitOfWork())
                {
                    var repository = RepositoryFactory.CreateConsentRepository(uow);
                    repository.AddOrUpdate(consent);
                    uow.Commit();
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to save consent (see inner exception).", e);
            }
        }

        public void Delete(IConsent consent)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreateConsentRepository(uow);
                repository.Delete(consent);
                uow.Commit();
            }
        }
    }
}
