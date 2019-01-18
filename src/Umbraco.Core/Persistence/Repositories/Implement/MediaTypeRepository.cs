using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="IMediaType"/>
    /// </summary>
    internal class MediaTypeRepository : ContentTypeRepositoryBase<IMediaType>, IMediaTypeRepository
    {
        public MediaTypeRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger logger)
            : base(scopeAccessor, cache, logger)
        { }

        protected override bool IsPublishing => MediaType.IsPublishingConst;

        protected override IRepositoryCachePolicy<IMediaType, int> CreateCachePolicy()
        {
            return new FullDataSetRepositoryCachePolicy<IMediaType, int>(GlobalIsolatedCache, ScopeAccessor, GetEntityId, /*expires:*/ true);
        }

        protected override IMediaType PerformGet(int id)
        {
            //use the underlying GetAll which will force cache all content types
            return GetMany().FirstOrDefault(x => x.Id == id);
        }

        protected override IMediaType PerformGet(Guid id)
        {
            //use the underlying GetAll which will force cache all content types
            return GetMany().FirstOrDefault(x => x.Key == id);
        }

        protected override bool PerformExists(Guid id)
        {
            return GetMany().FirstOrDefault(x => x.Key == id) != null;
        }

        protected override IMediaType PerformGet(string alias)
        {
            //use the underlying GetAll which will force cache all content types
            return GetMany().FirstOrDefault(x => x.Alias.InvariantEquals(alias));
        }

        protected override IEnumerable<IMediaType> PerformGetAll(params int[] ids)
        {
            if (ids.Any())
            {
                //NOTE: This logic should never be executed according to our cache policy
                return ContentTypeQueryMapper.GetMediaTypes(Database, SqlSyntax, IsPublishing, this)
                    .Where(x => ids.Contains(x.Id));
            }

            return ContentTypeQueryMapper.GetMediaTypes(Database, SqlSyntax, IsPublishing, this);
        }

        protected override IEnumerable<IMediaType> PerformGetAll(params Guid[] ids)
        {
            //use the underlying GetAll which will force cache all content types

            if (ids.Any())
            {
                return GetMany().Where(x => ids.Contains(x.Key));
            }
            else
            {
                return GetMany();
            }
        }

        protected override IEnumerable<IMediaType> PerformGetByQuery(IQuery<IMediaType> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IMediaType>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<ContentTypeDto>(sql);

            return
                //This returns a lookup from the GetAll cached looup
                (dtos.Any()
                    ? GetMany(dtos.DistinctBy(x => x.NodeId).Select(x => x.NodeId).ToArray())
                    : Enumerable.Empty<IMediaType>())
                    //order the result by name
                    .OrderBy(x => x.Name);
        }

        /// <summary>
        /// Gets all entities of the specified <see cref="PropertyType"/> query
        /// </summary>
        /// <param name="query"></param>
        /// <returns>An enumerable list of <see cref="IMediaType"/> objects</returns>
        public IEnumerable<IMediaType> GetByQuery(IQuery<PropertyType> query)
        {
            var ints = PerformGetByQuery(query).ToArray();
            return ints.Any()
                ? GetMany(ints)
                : Enumerable.Empty<IMediaType>();
        }

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            var sql = Sql();

            sql = isCount
                ? sql.SelectCount()
                : sql.Select<ContentTypeDto>(r => r.Select(x => x.NodeDto));

            sql
                .From<ContentTypeDto>()
                .InnerJoin<NodeDto>()
                .On<ContentTypeDto, NodeDto>( left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);

            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "umbracoNode.id = @id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var l = (List<string>) base.GetDeleteClauses(); // we know it's a list
            l.Add("DELETE FROM cmsContentType WHERE nodeId = @id");
            l.Add("DELETE FROM umbracoNode WHERE id = @id");
            return l;
        }

        protected override Guid NodeObjectTypeId => Constants.ObjectTypes.MediaType;

        protected override void PersistNewItem(IMediaType entity)
        {
            ((MediaType)entity).AddingEntity();

            PersistNewBaseContentType(entity);

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IMediaType entity)
        {
            ValidateAlias(entity);

            //Updates Modified date
            ((MediaType)entity).UpdatingEntity();

            //Look up parent to get and set the correct Path if ParentId has changed
            if (entity.IsPropertyDirty("ParentId"))
            {
                var parent = Database.First<NodeDto>("WHERE id = @ParentId", new { ParentId = entity.ParentId });
                entity.Path = string.Concat(parent.Path, ",", entity.Id);
                entity.Level = parent.Level + 1;
                var maxSortOrder =
                    Database.ExecuteScalar<int>(
                        "SELECT coalesce(max(sortOrder),0) FROM umbracoNode WHERE parentid = @ParentId AND nodeObjectType = @NodeObjectType",
                        new { ParentId = entity.ParentId, NodeObjectType = NodeObjectTypeId });
                entity.SortOrder = maxSortOrder + 1;
            }

            PersistUpdatedBaseContentType(entity);

            entity.ResetDirtyProperties();
        }
    }
}
