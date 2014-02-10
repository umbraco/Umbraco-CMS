using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal class MemberGroupRepository : PetaPocoRepositoryBase<int, IMemberGroup>
    {
        public MemberGroupRepository(IDatabaseUnitOfWork work) : base(work)
        {
        }

        public MemberGroupRepository(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache) : base(work, cache)
        {
        }

        private readonly MemberGroupFactory _modelFactory = new MemberGroupFactory();

        protected override IMemberGroup PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });

            var dto = Database.Fetch<NodeDto>(sql).FirstOrDefault();

            return dto == null ? null : _modelFactory.BuildEntity(dto);
        }

        protected override IEnumerable<IMemberGroup> PerformGetAll(params int[] ids)
        {
            var memberType = new Guid(Constants.ObjectTypes.MemberGroup);
            if (ids.Any())
            {
                var sql = new Sql()
                    .From<NodeDto>()
                    .Where<NodeDto>(dto => dto.NodeObjectType == memberType)
                    .Where("Name in (@ids)", new {ids = ids});
                return Database.Fetch<NodeDto>(sql)
                    .Select(x => _modelFactory.BuildEntity(x));
            }
            else
            {
                var sql = new Sql()
                    .From<NodeDto>()
                    .Where<NodeDto>(dto => dto.NodeObjectType == memberType);
                return Database.Fetch<NodeDto>(sql)
                    .Select(x => _modelFactory.BuildEntity(x));
            }
        }

        protected override IEnumerable<IMemberGroup> PerformGetByQuery(IQuery<IMemberGroup> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IMemberGroup>(sqlClause, query);
            var sql = translator.Translate();

            return Database.Fetch<NodeDto>(sql)
                .Select(x => _modelFactory.BuildEntity(x));
        }

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*")
                .From<NodeDto>()                
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "umbracoNode.id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new[]
                           {
                               "DELETE FROM cmsMember2MemberGroup WHERE MemberGroup = @Id",
                               "DELETE FROM umbracoNode WHERE id = @Id"
                           };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { return new Guid(Constants.ObjectTypes.MemberGroup); }
        }

        protected override void PersistNewItem(IMemberGroup entity)
        {
            //Save to db
            var group = entity as MemberGroup;
            group.AddingEntity();
            var dto = _modelFactory.BuildDto(group);
            var o = Database.IsNew(dto) ? Convert.ToInt32(Database.Insert(dto)) : Database.Update(dto);

            //Update with new correct path
            dto.Path = string.Concat("-1,", dto.NodeId);
            Database.Update(dto);

            group.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IMemberGroup entity)
        {
            var dto = _modelFactory.BuildDto(entity);

            Database.Update(dto);

            ((ICanBeDirty)entity).ResetDirtyProperties();
        }
    }
}