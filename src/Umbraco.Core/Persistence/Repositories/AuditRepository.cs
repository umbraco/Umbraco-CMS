using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal class AuditRepository : PetaPocoRepositoryBase<int, AuditItem>, IAuditRepository
    {
        public AuditRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax, IMappingResolver mappingResolver)
            : base(work, cache, logger, sqlSyntax, mappingResolver)
        {
        }

        protected override void PersistNewItem(AuditItem entity)
        {
            throw new NotImplementedException();
        }
        
        protected override void PersistUpdatedItem(AuditItem entity)
        {
            Database.Insert(new LogDto
            {
                Comment = entity.Comment,
                Datestamp = DateTime.Now,
                Header = entity.AuditType.ToString(),
                NodeId = entity.Id,
                UserId = entity.UserId
            });
        }

        protected override AuditItem PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });

            var dto = Database.First<LogDto>(sql);
            if (dto == null)
                return null;
            
            return new AuditItem(dto.NodeId, dto.Comment, Enum<AuditType>.Parse(dto.Header), dto.UserId);
        }

        protected override IEnumerable<AuditItem> PerformGetAll(params int[] ids)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<AuditItem> PerformGetByQuery(IQuery<AuditItem> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<AuditItem>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<LogDto>(sql);

            return dtos.Select(x => new AuditItem(x.NodeId, x.Comment, Enum<AuditType>.Parse(x.Header), x.UserId)).ToArray();
        }

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*")
                .From<LogDto>(SqlSyntax);
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            throw new NotImplementedException();
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }

        public void CleanLogs(int maximumAgeOfLogsInMinutes)
        {
            var oldestPermittedLogEntry = DateTime.Now.Subtract(new TimeSpan(0, maximumAgeOfLogsInMinutes, 0));

            Database.Execute(
                "delete from umbracoLog where datestamp < @oldestPermittedLogEntry and logHeader in ('open','system')",
                new {oldestPermittedLogEntry = oldestPermittedLogEntry});
        }
    }
}