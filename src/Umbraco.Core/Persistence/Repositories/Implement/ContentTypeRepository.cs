using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="IContentType"/>
    /// </summary>
    internal class ContentTypeRepository : ContentTypeRepositoryBase<IContentType>, IContentTypeRepository
    {
        private readonly ITemplateRepository _templateRepository;

        public ContentTypeRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger logger, ITemplateRepository templateRepository)
            : base(scopeAccessor, cache, logger)
        {
            _templateRepository = templateRepository;
        }

        protected override bool IsPublishing => ContentType.IsPublishingConst;

        protected override IRepositoryCachePolicy<IContentType, int> CreateCachePolicy()
        {
            return new FullDataSetRepositoryCachePolicy<IContentType, int>(GlobalIsolatedCache, ScopeAccessor, GetEntityId, /*expires:*/ true);
        }

        protected override IContentType PerformGet(int id)
        {
            //use the underlying GetAll which will force cache all content types
            return GetMany().FirstOrDefault(x => x.Id == id);
        }

        protected override IContentType PerformGet(Guid id)
        {
            //use the underlying GetAll which will force cache all content types
            return GetMany().FirstOrDefault(x => x.Key == id);
        }

        protected override IContentType PerformGet(string alias)
        {
            //use the underlying GetAll which will force cache all content types
            return GetMany().FirstOrDefault(x => x.Alias.InvariantEquals(alias));
        }

        protected override bool PerformExists(Guid id)
        {
            return GetMany().FirstOrDefault(x => x.Key == id) != null;
        }

        protected override IEnumerable<IContentType> PerformGetAll(params int[] ids)
        {
            if (ids.Any())
            {
                //NOTE: This logic should never be executed according to our cache policy
                return ContentTypeQueryMapper.GetContentTypes(Database, SqlSyntax, IsPublishing, this, _templateRepository)
                    .Where(x => ids.Contains(x.Id));
            }

            return ContentTypeQueryMapper.GetContentTypes(Database, SqlSyntax, IsPublishing, this, _templateRepository);
        }

        protected override IEnumerable<IContentType> PerformGetAll(params Guid[] ids)
        {
            // use the underlying GetAll which will force cache all content types
            return ids.Any() ? GetMany().Where(x => ids.Contains(x.Key)) : GetMany();
        }

        protected override IEnumerable<IContentType> PerformGetByQuery(IQuery<IContentType> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IContentType>(sqlClause, query);
            var sql = translator.Translate();

            // fixme - insane! GetBaseQuery does not even return a proper??? oh well...
            var dtos = Database.Fetch<ContentTypeTemplateDto>(sql);

            return
                //This returns a lookup from the GetAll cached looup
                (dtos.Any()
                    ? GetMany(dtos.DistinctBy(x => x.ContentTypeDto.NodeId).Select(x => x.ContentTypeDto.NodeId).ToArray())
                    : Enumerable.Empty<IContentType>())
                    //order the result by name
                    .OrderBy(x => x.Name);
        }

        /// <summary>
        /// Gets all entities of the specified <see cref="PropertyType"/> query
        /// </summary>
        /// <param name="query"></param>
        /// <returns>An enumerable list of <see cref="IContentType"/> objects</returns>
        public IEnumerable<IContentType> GetByQuery(IQuery<PropertyType> query)
        {
            var ints = PerformGetByQuery(query).ToArray();
            return ints.Any()
                ? GetMany(ints)
                : Enumerable.Empty<IContentType>();
        }

        /// <summary>
        /// Gets all property type aliases.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllPropertyTypeAliases()
        {
            return Database.Fetch<string>("SELECT DISTINCT Alias FROM cmsPropertyType ORDER BY Alias");
        }

        /// <summary>
        /// Gets all content type aliases
        /// </summary>
        /// <param name="objectTypes">
        /// If this list is empty, it will return all content type aliases for media, members and content, otherwise
        /// it will only return content type aliases for the object types specified
        /// </param>
        /// <returns></returns>
        public IEnumerable<string> GetAllContentTypeAliases(params Guid[] objectTypes)
        {
            var sql = Sql()
                .Select("cmsContentType.alias")
                .From<ContentTypeDto>()
                .InnerJoin<NodeDto>()
                .On<ContentTypeDto, NodeDto>(dto => dto.NodeId, dto => dto.NodeId);

            if (objectTypes.Any())
            {
                sql = sql.Where("umbracoNode.nodeObjectType IN (@objectTypes)", objectTypes);
            }

            return Database.Fetch<string>(sql);
        }

        public IEnumerable<int> GetAllContentTypeIds(string[] aliases)
        {
            if (aliases.Length == 0) return Enumerable.Empty<int>();

            var sql = Sql()
                .Select("cmsContentType.nodeId")
                .From<ContentTypeDto>()
                .InnerJoin<NodeDto>()
                .On<ContentTypeDto, NodeDto>(dto => dto.NodeId, dto => dto.NodeId)
                .Where<ContentTypeDto>(dto => aliases.Contains(dto.Alias));

            return Database.Fetch<int>(sql);
        }

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            var sql = Sql();

            sql = isCount
                ? sql.SelectCount()
                : sql.Select<ContentTypeTemplateDto>(r => r.Select(x => x.ContentTypeDto, r1 => r1.Select(x => x.NodeDto)));

            sql
                .From<ContentTypeDto>()
                .InnerJoin<NodeDto>()
                .On<ContentTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .LeftJoin<ContentTypeTemplateDto>()
                .On<ContentTypeTemplateDto, ContentTypeDto>(left => left.ContentTypeNodeId, right => right.NodeId)
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
            l.Add("DELETE FROM cmsDocumentType WHERE contentTypeNodeId = @id");
            l.Add("DELETE FROM cmsContentType WHERE nodeId = @id");
            l.Add("DELETE FROM umbracoNode WHERE id = @id");
            return l;
        }

        protected override Guid NodeObjectTypeId => Constants.ObjectTypes.DocumentType;

        /// <summary>
        /// Deletes a content type
        /// </summary>
        /// <param name="entity"></param>
        /// <remarks>
        /// First checks for children and removes those first
        /// </remarks>
        protected override void PersistDeletedItem(IContentType entity)
        {
            var query = Query<IContentType>().Where(x => x.ParentId == entity.Id);
            var children = Get(query);
            foreach (var child in children)
            {
                PersistDeletedItem(child);
            }

            //Before we call the base class methods to run all delete clauses, we need to first
            // delete all of the property data associated with this document type. Normally this will
            // be done in the ContentTypeService by deleting all associated content first, but in some cases
            // like when we switch a document type, there is property data left over that is linked
            // to the previous document type. So we need to ensure it's removed.
            var sql = Sql()
                .Select("DISTINCT " + Constants.DatabaseSchema.Tables.PropertyData + ".propertytypeid")
                .From<PropertyDataDto>()
                .InnerJoin<PropertyTypeDto>()
                .On<PropertyDataDto, PropertyTypeDto>(dto => dto.PropertyTypeId, dto => dto.Id)
                .InnerJoin<ContentTypeDto>()
                .On<ContentTypeDto, PropertyTypeDto>(dto => dto.NodeId, dto => dto.ContentTypeId)
                .Where<ContentTypeDto>(dto => dto.NodeId == entity.Id);

            //Delete all PropertyData where propertytypeid EXISTS in the subquery above
            Database.Execute(SqlSyntax.GetDeleteSubquery(Constants.DatabaseSchema.Tables.PropertyData, "propertytypeid", sql));

            base.PersistDeletedItem(entity);
        }

        protected override void PersistNewItem(IContentType entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Alias))
            {
                var ex = new Exception($"ContentType '{entity.Name}' cannot have an empty Alias. This is most likely due to invalid characters stripped from the Alias.");
                Logger.Error<ContentTypeRepository>("ContentType '{EntityName}' cannot have an empty Alias. This is most likely due to invalid characters stripped from the Alias.", entity.Name);
                throw ex;
            }

            ((ContentType)entity).AddingEntity();

            PersistNewBaseContentType(entity);
            PersistTemplates(entity, false);

            entity.ResetDirtyProperties();
        }

        protected void PersistTemplates(IContentType entity, bool clearAll)
        {
            // remove and insert, if required
            Database.Delete<ContentTypeTemplateDto>("WHERE contentTypeNodeId = @Id", new { Id = entity.Id });

            // we could do it all in foreach if we assume that the default template is an allowed template??
            var defaultTemplateId = ((ContentType) entity).DefaultTemplateId;
            if (defaultTemplateId > 0)
            {
                Database.Insert(new ContentTypeTemplateDto
                {
                    ContentTypeNodeId = entity.Id,
                    TemplateNodeId = defaultTemplateId,
                    IsDefault = true
                });
            }
            foreach (var template in entity.AllowedTemplates.Where(x => x != null && x.Id != defaultTemplateId))
            {
                Database.Insert(new ContentTypeTemplateDto
                {
                    ContentTypeNodeId = entity.Id,
                    TemplateNodeId = template.Id,
                    IsDefault = false
                });
            }
        }

        protected override void PersistUpdatedItem(IContentType entity)
        {
            ValidateAlias(entity);

            //Updates Modified date
            ((ContentType)entity).UpdatingEntity();

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
            PersistTemplates(entity, true);

            entity.ResetDirtyProperties();
        }
    }
}
