using System;
using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services
{
    public sealed class AuditService : ScopeRepositoryService, IAuditService
    {
        private readonly IAuditRepository _auditRepository;

        public AuditService(IScopeProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory,
            IAuditRepository auditRepository)
            : base(provider, logger, eventMessagesFactory)
        {
            _auditRepository = auditRepository;
        }

        public void Add(AuditType type, string comment, int userId, int objectId)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _auditRepository.Save(new AuditItem(objectId, comment, type, userId));
                scope.Complete();
            }
        }

        public IEnumerable<AuditItem> GetLogs(int objectId)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var result = _auditRepository.Get(Query<AuditItem>().Where(x => x.Id == objectId));
                scope.Complete();
                return result;
            }
        }

        public IEnumerable<AuditItem> GetUserLogs(int userId, AuditType type, DateTime? sinceDate = null)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var result = sinceDate.HasValue == false
                    ? _auditRepository.Get(Query<AuditItem>().Where(x => x.UserId == userId && x.AuditType == type))
                    : _auditRepository.Get(Query<AuditItem>().Where(x => x.UserId == userId && x.AuditType == type && x.CreateDate >= sinceDate.Value));
                scope.Complete();
                return result;
            }
        }

        public IEnumerable<AuditItem> GetLogs(AuditType type, DateTime? sinceDate = null)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var result = sinceDate.HasValue == false
                    ? _auditRepository.Get(Query<AuditItem>().Where(x => x.AuditType == type))
                    : _auditRepository.Get(Query<AuditItem>().Where(x => x.AuditType == type && x.CreateDate >= sinceDate.Value));
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
    }
}
