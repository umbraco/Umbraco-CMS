using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Core.Services.Implement
{
    public sealed class AuditService : RepositoryService, IAuditService
    {
        private readonly Lazy<bool> _isAvailable;
        private readonly IAuditRepository _auditRepository;
        private readonly IAuditEntryRepository _auditEntryRepository;

        public AuditService(IScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory,
            IAuditRepository auditRepository, IAuditEntryRepository auditEntryRepository)
            : base(provider, loggerFactory, eventMessagesFactory)
        {
            _auditRepository = auditRepository;
            _auditEntryRepository = auditEntryRepository;
            _isAvailable = new Lazy<bool>(DetermineIsAvailable);
        }

        public void Add(AuditType type, int userId, int objectId, string entityType, string comment, string parameters = null)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _auditRepository.Save(new AuditItem(objectId, type, userId, entityType, comment, parameters));
                scope.Complete();
            }
        }

        public IEnumerable<IAuditItem> GetLogs(int objectId)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var result = _auditRepository.Get(Query<IAuditItem>().Where(x => x.Id == objectId));
                scope.Complete();
                return result;
            }
        }

        public IEnumerable<IAuditItem> GetUserLogs(int userId, AuditType type, DateTime? sinceDate = null)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var result = sinceDate.HasValue == false
                    ? _auditRepository.Get(type, Query<IAuditItem>().Where(x => x.UserId == userId))
                    : _auditRepository.Get(type, Query<IAuditItem>().Where(x => x.UserId == userId && x.CreateDate >= sinceDate.Value));
                scope.Complete();
                return result;
            }
        }

        public IEnumerable<IAuditItem> GetLogs(AuditType type, DateTime? sinceDate = null)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var result = sinceDate.HasValue == false
                    ? _auditRepository.Get(type, Query<IAuditItem>())
                    : _auditRepository.Get(type, Query<IAuditItem>().Where(x => x.CreateDate >= sinceDate.Value));
                scope.Complete();
                return result;
            }
        }

        public void CleanLogs(int maximumAgeOfLogsInMinutes)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _auditRepository.CleanLogs(maximumAgeOfLogsInMinutes);
                scope.Complete();
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
            if (pageIndex < 0) throw new ArgumentOutOfRangeException(nameof(pageIndex));
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

            if (entityId == Cms.Core.Constants.System.Root || entityId <= 0)
            {
                totalRecords = 0;
                return Enumerable.Empty<IAuditItem>();
            }

            using (ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IAuditItem>().Where(x => x.Id == entityId);

                return _auditRepository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalRecords, orderDirection, auditTypeFilter, customFilter);
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
            if (pageIndex < 0) throw new ArgumentOutOfRangeException(nameof(pageIndex));
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

            if (userId < Cms.Core.Constants.Security.SuperUserId)
            {
                totalRecords = 0;
                return Enumerable.Empty<IAuditItem>();
            }

            using (ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IAuditItem>().Where(x => x.UserId == userId);

                return _auditRepository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalRecords, orderDirection, auditTypeFilter, customFilter);
            }
        }

        /// <inheritdoc />
        public IAuditEntry Write(int performingUserId, string perfomingDetails, string performingIp, DateTime eventDateUtc, int affectedUserId, string affectedDetails, string eventType, string eventDetails)
        {
            if (performingUserId < 0 && performingUserId != Cms.Core.Constants.Security.SuperUserId) throw new ArgumentOutOfRangeException(nameof(performingUserId));
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

            using (var scope = ScopeProvider.CreateScope())
            {
                _auditEntryRepository.Save(entry);
                scope.Complete();
            }

            return entry;
        }

        // TODO: Currently used in testing only, not part of the interface, need to add queryable methods to the interface instead
        internal IEnumerable<IAuditEntry> GetAll()
        {
            if (_isAvailable.Value == false) return Enumerable.Empty<IAuditEntry>();

            using (ScopeProvider.CreateScope(autoComplete: true))
            {
                return _auditEntryRepository.GetMany();
            }
        }

        // TODO: Currently used in testing only, not part of the interface, need to add queryable methods to the interface instead
        internal IEnumerable<IAuditEntry> GetPage(long pageIndex, int pageCount, out long records)
        {
            if (_isAvailable.Value == false)
            {
                records = 0;
                return Enumerable.Empty<IAuditEntry>();
            }

            using (ScopeProvider.CreateScope(autoComplete: true))
            {
                return _auditEntryRepository.GetPage(pageIndex, pageCount, out records);
            }
        }

        /// <summary>
        /// Determines whether the repository is available.
        /// </summary>
        private bool DetermineIsAvailable()
        {
            using (ScopeProvider.CreateScope(autoComplete: true))
            {
                return _auditEntryRepository.IsAvailable();
            }
        }
    }
}
