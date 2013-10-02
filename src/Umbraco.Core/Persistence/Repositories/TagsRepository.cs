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
    internal class TagsRepository : PetaPocoRepositoryBase<int, ITag>
    {
        protected TagsRepository(IDatabaseUnitOfWork work)
            : this(work, RuntimeCacheProvider.Current)
        {
        }

        internal TagsRepository(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache)
            : base(work, cache)
        {     
        }

        protected override ITag PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });

            var tagDto = Database.Fetch<TagDto>(sql).FirstOrDefault();
            if (tagDto == null)
                return null;

            var factory = new TagFactory();
            var entity = factory.BuildEntity(tagDto);

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            ((TracksChangesEntityBase)entity).ResetDirtyProperties(false);

            return entity;
        }

        protected override IEnumerable<ITag> PerformGetAll(params int[] ids)
        {
            if (ids.Any())
            {
                return PerformGetAllOnIds(ids);
            }

            var sql = GetBaseQuery(false);

            return ConvertFromDtos(Database.Fetch<TagDto>(sql))
                .ToArray();// we don't want to re-iterate again!
        }

        private IEnumerable<ITag> PerformGetAllOnIds(params int[] ids)
        {
            if (ids.Any() == false) yield break;
            foreach (var id in ids)
            {
                yield return Get(id);
            }
        }

        private IEnumerable<ITag> ConvertFromDtos(IEnumerable<TagDto> dtos)
        {
            var factory = new TagFactory();
            foreach (var entity in dtos.Select(factory.BuildEntity))
            {
                //on initial construction we don't want to have dirty properties tracked
                // http://issues.umbraco.org/issue/U4-1946
                ((TracksChangesEntityBase)entity).ResetDirtyProperties(false);
                yield return entity;
            }
        }

        protected override IEnumerable<ITag> PerformGetByQuery(IQuery<ITag> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<ITag>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<TagDto>(sql);

            foreach (var dto in dtos)
            {
                yield return Get(dto.Id);
            }
        }

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            if (isCount)
            {
                sql.Select("COUNT(*)").From<TagDto>();
            }
            else
            {
                return GetBaseQuery();
            }
            return sql;
        }

        private static Sql GetBaseQuery()
        {
            var sql = new Sql();
            sql.Select("*").From<TagDto>();
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                {
                    "DELETE FROM cmsTagRelationship WHERE tagId = @Id",
                    "DELETE FROM cmsTags WHERE id = @Id"
                };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }

        protected override void PersistNewItem(ITag entity)
        {
            ((Entity)entity).AddingEntity();

            var factory = new TagFactory();
            var dto = factory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;

            ((ICanBeDirty)entity).ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(ITag entity)
        {
            ((Entity)entity).UpdatingEntity();

            var factory = new TagFactory();
            var dto = factory.BuildDto(entity);

            Database.Update(dto);
            
            ((ICanBeDirty)entity).ResetDirtyProperties();
        }
    }
}