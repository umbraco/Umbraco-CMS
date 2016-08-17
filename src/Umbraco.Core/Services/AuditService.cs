using System;
using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    public sealed class AuditService : RepositoryService, IAuditService
    {
        public AuditService(IDatabaseUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, logger, eventMessagesFactory)
        {
        }

        public void Add(AuditType type, string comment, int userId, int objectId)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IAuditRepository>();
                repo.AddOrUpdate(new AuditItem(objectId, comment, type, userId));
                uow.Complete();
            }
        }

        public IEnumerable<AuditItem> GetLogs(int objectId)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IAuditRepository>();
                var result = repo.GetByQuery(repo.Query.Where(x => x.Id == objectId));
                uow.Complete();
                return result;
            }
        }

        public IEnumerable<AuditItem> GetUserLogs(int userId, AuditType type, DateTime? sinceDate = null)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IAuditRepository>();
                var result = sinceDate.HasValue == false
                    ? repo.GetByQuery(repo.Query.Where(x => x.UserId == userId && x.AuditType == type))
                    : repo.GetByQuery(repo.Query.Where(x => x.UserId == userId && x.AuditType == type && x.CreateDate >= sinceDate.Value));
                uow.Complete();
                return result;
            }
        }

        public IEnumerable<AuditItem> GetLogs(AuditType type, DateTime? sinceDate = null)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IAuditRepository>();
                var result = sinceDate.HasValue == false
                    ? repo.GetByQuery(repo.Query.Where(x => x.AuditType == type))
                    : repo.GetByQuery(repo.Query.Where(x => x.AuditType == type && x.CreateDate >= sinceDate.Value));
                uow.Complete();
                return result;
            }
        }

        public void CleanLogs(int maximumAgeOfLogsInMinutes)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IAuditRepository>();
                repo.CleanLogs(maximumAgeOfLogsInMinutes);
                uow.Complete();
            }
        }
    }
}