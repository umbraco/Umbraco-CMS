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
    public sealed class AuditService : RepositoryService, IAuditService
    {
        public AuditService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, repositoryFactory, logger, eventMessagesFactory)
        {
        }

        public void Add(AuditType type, string comment, int userId, int objectId)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreateAuditRepository(uow))
            {
                repo.AddOrUpdate(new AuditItem(objectId, comment, type, userId));
                uow.Commit();
            }
        }

        public IEnumerable<AuditItem> GetLogs(int objectId)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreateAuditRepository(uow))
            {
                var result = repo.GetByQuery(repo.Query.Where(x => x.Id == objectId));
                return result;
            }
        }

        public IEnumerable<AuditItem> GetUserLogs(int userId, AuditType type, DateTime? sinceDate = null)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreateAuditRepository(uow))
            {
                var result = sinceDate.HasValue == false
                    ? repo.GetByQuery(repo.Query.Where(x => x.UserId == userId && x.AuditType == type))
                    : repo.GetByQuery(repo.Query.Where(x => x.UserId == userId && x.AuditType == type && x.CreateDate >= sinceDate.Value));
                return result;
            }
        }

        public IEnumerable<AuditItem> GetLogs(AuditType type, DateTime? sinceDate = null)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreateAuditRepository(uow))
            {
                var result = sinceDate.HasValue == false
                    ? repo.GetByQuery(repo.Query.Where(x => x.AuditType == type))
                    : repo.GetByQuery(repo.Query.Where(x => x.AuditType == type && x.CreateDate >= sinceDate.Value));
                return result;
            }
        }

        public void CleanLogs(int maximumAgeOfLogsInMinutes)
        {
            using (var repo = RepositoryFactory.CreateAuditRepository(UowProvider.GetUnitOfWork()))
            {
                repo.CleanLogs(maximumAgeOfLogsInMinutes);
            }
        }
    }
}