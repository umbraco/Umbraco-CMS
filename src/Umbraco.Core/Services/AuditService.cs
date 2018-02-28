using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    public sealed class AuditService : ScopeRepositoryService, IAuditService
    {
        private readonly Lazy<bool> _isAvailable;

        public AuditService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, repositoryFactory, logger, eventMessagesFactory)
        {
            _isAvailable = new Lazy<bool>(DetermineIsAvailable);
        }

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
        public IAuditEntry Write(int performingUserId, string perfomingDetails, string performingIp, DateTime eventDateUtc, int affectedUserId, string affectedDetails, string eventType, string eventDetails)
        {
            if (performingUserId < 0) throw new ArgumentOutOfRangeException(nameof(performingUserId));
            if (string.IsNullOrWhiteSpace(perfomingDetails)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(perfomingDetails));
            if (string.IsNullOrWhiteSpace(eventType)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(eventType));
            if (string.IsNullOrWhiteSpace(eventDetails)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(eventDetails));

            //we need to truncate the data else we'll get SQL errors
            affectedDetails = affectedDetails?.Substring(0, Math.Min(affectedDetails.Length, AuditEntryDto.DetailsLength));
            eventDetails = eventDetails.Substring(0, Math.Min(eventDetails.Length, AuditEntryDto.DetailsLength));

            //validate the eventType - must contain a forward slash, no spaces, no special chars
            var eventTypeParts = eventType.ToCharArray();
            if (eventTypeParts.Contains('/') == false || eventTypeParts.All(c => char.IsLetterOrDigit(c) || c == '/' || c == '-') == false)
                throw new ArgumentException(nameof(eventType) + " must contain only alphanumeric characters, hyphens and at least one '/' defining a category");
            if (eventType.Length > AuditEntryDto.EventTypeLength)
                throw new ArgumentException($"Must be max {AuditEntryDto.EventTypeLength} chars.", nameof(eventType));
            if (performingIp != null && performingIp.Length > AuditEntryDto.IpLength)
                throw new ArgumentException($"Must be max {AuditEntryDto.EventTypeLength} chars.", nameof(performingIp));

            var entry = new AuditEntry
            {
                PerformingUserId = performingUserId,
                PerformingDetails = perfomingDetails,
                PerformingIp = performingIp,
                EventDateUtc = eventDateUtc,
                AffectedUserId = affectedUserId,
                AffectedDetails = affectedDetails,
                EventType = eventType,
                EventDetails = eventDetails
            };

            if (_isAvailable.Value == false) return entry;

            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreateAuditEntryRepository(uow);
                repository.AddOrUpdate(entry);
                uow.Commit();
            }

            return entry;
        }

        //TODO: Currently used in testing only, not part of the interface, need to add queryable methods to the interface instead
        internal IEnumerable<IAuditEntry> GetAll()
        {
            if (_isAvailable.Value == false) return Enumerable.Empty<IAuditEntry>();

            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateAuditEntryRepository(uow);
                return repository.GetAll();
            }
        }

        //TODO: Currently used in testing only, not part of the interface, need to add queryable methods to the interface instead
        internal IEnumerable<IAuditEntry> GetPage(long pageIndex, int pageCount, out long records)
        {
            if (_isAvailable.Value == false)
            {
                records = 0;
                return Enumerable.Empty<IAuditEntry>();
            }

            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateAuditEntryRepository(uow);
                return repository.GetPage(pageIndex, pageCount, out records);
            }
        }

        /// <summary>
        /// Determines whether the repository is available.
        /// </summary>
        private bool DetermineIsAvailable()
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateAuditEntryRepository(uow);
                return repository.IsAvailable();
            }
        }
    }
}
