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

        /// <summary>
        /// Returns paged items in the audit trail for a given entity
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="orderDirection">
        /// By default this will always be ordered descending (newest first)
        /// </param>
        /// <param name="auditTypeFilter">
        /// Since we currently do not have enum support with our expression parser, we cannot query on AuditType in the query or the custom filter
        /// so we need to do that here
        /// </param>
        /// <param name="customFilter">
        /// Optional filter to be applied
        /// </param>
        /// <returns></returns>
        public IEnumerable<IAuditItem> GetPagedItemsByEntity(int entityId, long pageIndex, int pageSize, out long totalRecords,
            Direction orderDirection = Direction.Descending,
            AuditType[] auditTypeFilter = null,
            IQuery<IAuditItem> customFilter = null)
        {
            Mandate.ParameterCondition(pageIndex >= 0, "pageIndex");
            Mandate.ParameterCondition(pageSize > 0, "pageSize");

            if (entityId == Constants.System.Root || entityId <= 0)
            {
                totalRecords = 0;
                return Enumerable.Empty<IAuditItem>();
            }

            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateAuditRepository(uow);

                var query = Query<IAuditItem>.Builder.Where(x => x.Id == entityId);

                return repository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalRecords, orderDirection, auditTypeFilter, customFilter);
            }
        }

        /// <summary>
        /// Returns paged items in the audit trail for a given user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="orderDirection">
        /// By default this will always be ordered descending (newest first)
        /// </param>
        /// <param name="auditTypeFilter">
        /// Since we currently do not have enum support with our expression parser, we cannot query on AuditType in the query or the custom filter
        /// so we need to do that here
        /// </param>
        /// <param name="customFilter">
        /// Optional filter to be applied
        /// </param>
        /// <returns></returns>
        public IEnumerable<IAuditItem> GetPagedItemsByUser(int userId, long pageIndex, int pageSize, out long totalRecords, Direction orderDirection = Direction.Descending, AuditType[] auditTypeFilter = null, IQuery<IAuditItem> customFilter = null)
        {
            Mandate.ParameterCondition(pageIndex >= 0, "pageIndex");
            Mandate.ParameterCondition(pageSize > 0, "pageSize");

            if (userId < 0)
            {
                totalRecords = 0;
                return Enumerable.Empty<IAuditItem>();
            }

            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateAuditRepository(uow);

                var query = Query<IAuditItem>.Builder.Where(x => x.UserId == userId);

                return repository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalRecords, orderDirection, auditTypeFilter, customFilter);
            }
        }

        /// <inheritdoc />
        public IAuditEntry Write(int performingUserId, string perfomingDetails, string performingIp, DateTime eventDate, int affectedUserId, string affectedDetails, string eventType, string eventDetails)
        {
            var entry = new AuditEntry
            {
                PerformingUserId = performingUserId,
                PerformingDetails = perfomingDetails,
                PerformingIp = performingIp,
                EventDate = eventDate,
                AffectedUserId = affectedUserId,
                AffectedDetails = affectedDetails,
                EventType = eventType,
                EventDetails = eventDetails
            };

            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreateAuditEntryRepository(uow);
                repository.AddOrUpdate(entry);
                uow.Commit();
            }

            return entry;
        }

        /// <inheritdoc />
        public IEnumerable<IAuditEntry> Get()
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateAuditEntryRepository(uow);
                return repository.GetAll();
            }
        }

        /// <inheritdoc />
        public IEnumerable<IAuditEntry> GetPage(long pageIndex, int pageCount, out long records)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateAuditEntryRepository(uow);
                return repository.GetPage(pageIndex, pageCount, out records);
            }
        }
    }
}
