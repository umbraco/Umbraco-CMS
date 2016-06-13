using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="ISchemaType"/>
    /// </summary>
    internal class SchemaTypeRepository : ContentTypeBaseRepository<ISchemaType>, ISchemaTypeRepository
    {

        public SchemaTypeRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
            : base(work, cache, logger, sqlSyntax)
        {
        }

        private FullDataSetRepositoryCachePolicyFactory<ISchemaType, int> _cachePolicyFactory;
        protected override IRepositoryCachePolicyFactory<ISchemaType, int> CachePolicyFactory
        {
            get
            {
                //Use a FullDataSet cache policy - this will cache the entire GetAll result in a single collection
                return _cachePolicyFactory ?? (_cachePolicyFactory = new FullDataSetRepositoryCachePolicyFactory<ISchemaType, int>(
                    RuntimeCache, GetEntityId, () => PerformGetAll(),
                    //allow this cache to expire
                    expires: true));
            }
        }

        protected override ISchemaType PerformGet(int id)
        {
            //use the underlying GetAll which will force cache all content types
            return GetAll().FirstOrDefault(x => x.Id == id);
        }

        protected override IEnumerable<ISchemaType> PerformGetAll(params int[] ids)
        {
            if (ids.Any())
            {
                //NOTE: This logic should never be executed according to our cache policy
                return ContentTypeQueryMapper.GetSchemaTypes(Database, SqlSyntax, this)
                    .Where(x => ids.Contains(x.Id));
            }

            return ContentTypeQueryMapper.GetSchemaTypes(Database, SqlSyntax, this);
        }

        protected override IEnumerable<ISchemaType> PerformGetByQuery(IQuery<ISchemaType> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<ISchemaType>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<ContentTypeDto, NodeDto>(sql);

            return
                //This returns a lookup from the GetAll cached looup
                (dtos.Any()
                    ? GetAll(dtos.DistinctBy(x => x.NodeId).Select(x => x.NodeId).ToArray())
                    : Enumerable.Empty<ISchemaType>())
                    //order the result by name
                    .OrderBy(x => x.Name);
        }
        
        /// <summary>
        /// Gets all entities of the specified <see cref="PropertyType"/> query
        /// </summary>
        /// <param name="query"></param>
        /// <returns>An enumerable list of <see cref="IContentType"/> objects</returns>
        public IEnumerable<ISchemaType> GetByQuery(IQuery<PropertyType> query)
        {
            var ints = PerformGetByQuery(query).ToArray();
            return ints.Any()
                ? GetAll(ints)
                : Enumerable.Empty<ISchemaType>();
        }       
        
        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*")
                .From<ContentTypeDto>(SqlSyntax)
                .InnerJoin<NodeDto>(SqlSyntax)
                .On<ContentTypeDto, NodeDto>(SqlSyntax, left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "umbracoNode.id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
            {
                "DELETE FROM umbracoUser2NodeNotify WHERE nodeId = @Id",
                "DELETE FROM umbracoUser2NodePermission WHERE nodeId = @Id",
                "DELETE FROM cmsTagRelationship WHERE nodeId = @Id",
                "DELETE FROM cmsContentTypeAllowedContentType WHERE Id = @Id",
                "DELETE FROM cmsContentTypeAllowedContentType WHERE AllowedId = @Id",
                "DELETE FROM cmsContentType2ContentType WHERE parentContentTypeId = @Id",
                "DELETE FROM cmsContentType2ContentType WHERE childContentTypeId = @Id",
                "DELETE FROM cmsPropertyType WHERE contentTypeId = @Id",
                "DELETE FROM cmsPropertyTypeGroup WHERE contenttypeNodeId = @Id",
                "DELETE FROM cmsContentType WHERE nodeId = @Id",
                "DELETE FROM umbracoNode WHERE id = @Id"
            };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { return new Guid(Constants.ObjectTypes.SchemaType); }
        }
        
        protected override void PersistNewItem(ISchemaType entity)
        {
            ((SchemaType)entity).AddingEntity();

            PersistNewBaseContentType(entity);

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(ISchemaType entity)
        {
            ValidateAlias(entity);

            //Updates Modified date
            ((SchemaType)entity).UpdatingEntity();

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

        protected override ISchemaType PerformGet(Guid id)
        {
            //use the underlying GetAll which will force cache all content types
            return GetAll().FirstOrDefault(x => x.Key == id);
        }

        protected override IEnumerable<ISchemaType> PerformGetAll(params Guid[] ids)
        {
            //use the underlying GetAll which will force cache all content types

            if (ids.Any())
            {
                return GetAll().Where(x => ids.Contains(x.Key));
            }
            else
            {
                return GetAll();
            }
        }

        protected override bool PerformExists(Guid id)
        {
            return GetAll().FirstOrDefault(x => x.Key == id) != null;
        }

        protected override ISchemaType PerformGet(string alias)
        {
            //use the underlying GetAll which will force cache all content types
            return GetAll().FirstOrDefault(x => x.Alias.InvariantEquals(alias));
        }
    }
}