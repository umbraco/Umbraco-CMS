using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    public sealed class AuditService : ScopeRepositoryService, IAuditService
    {
        public AuditService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, repositoryFactory, logger, eventMessagesFactory)
        { }

        public void Add(AuditType type, string comment, int userId, int objectId)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repo = RepositoryFactory.CreateAuditRepository(uow);
                repo.AddOrUpdate(new AuditItem(objectId, comment, type, userId));
                uow.Commit();
            }
        }

        public IEnumerable<IAuditItem> GetPagedItems(int id, long pageIndex, int pageSize, out long totalRecords, Direction orderDirection = Direction.Descending, IQuery<IAuditItem> filter = null)
        {
            Mandate.ParameterCondition(pageIndex >= 0, "pageIndex");
            Mandate.ParameterCondition(pageSize > 0, "pageSize");

            if (id == Constants.System.Root || id <= 0)
            {
                totalRecords = 0;
                return Enumerable.Empty<IAuditItem>();
            }

            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateAuditRepository(uow);

                var query = Query<IAuditItem>.Builder.Where(x => x.Id == id);

                return repository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalRecords, orderDirection, filter);
            }
        }
    }
}