using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Models.Entities;
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
        public ContentTypeRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger logger, IContentTypeCommonRepository commonRepository, ILanguageRepository languageRepository)
            : base(scopeAccessor, cache, logger, commonRepository, languageRepository)
        { }

        protected override bool SupportsPublishing => ContentType.SupportsPublishingConst;

        protected override IRepositoryCachePolicy<IContentType, int> CreateCachePolicy()
        {
            return new FullDataSetRepositoryCachePolicy<IContentType, int>(GlobalIsolatedCache, ScopeAccessor, GetEntityId, /*expires:*/ true);
        }

        // every GetExists method goes cachePolicy.GetSomething which in turns goes PerformGetAll,
        // since this is a FullDataSet policy - and everything is cached
        // so here,
        // every PerformGet/Exists just GetMany() and then filters
        // except PerformGetAll which is the one really doing the job

        // TODO: the filtering is highly inefficient as we deep-clone everything
        // there should be a way to GetMany(predicate) right from the cache policy!
        // and ah, well, this all caching should be refactored + the cache refreshers
        // should to repository.Clear() not deal with magic caches by themselves

        protected override IContentType PerformGet(int id)
            => GetMany().FirstOrDefault(x => x.Id == id);

        protected override IContentType PerformGet(Guid id)
            => GetMany().FirstOrDefault(x => x.Key == id);

        protected override IContentType PerformGet(string alias)
            => GetMany().FirstOrDefault(x => x.Alias.InvariantEquals(alias));

        protected override bool PerformExists(Guid id)
        => GetMany().FirstOrDefault(x => x.Key == id) != null;

        protected override IEnumerable<IContentType> PerformGetAll(params int[] ids)
        {
            // the cache policy will always want everything
            // even GetMany(ids) gets everything and filters afterwards
            if (ids.Any()) throw new PanicException("There can be no ids specified");
            return CommonRepository.GetAllTypes().OfType<IContentType>();
        }

        protected override IEnumerable<IContentType> PerformGetAll(params Guid[] ids)
        {
            var all = GetMany();
            return ids.Any() ? all.Where(x => ids.Contains(x.Key)) : all;
        }

        protected override IEnumerable<IContentType> PerformGetByQuery(IQuery<IContentType> query)
        {
            var baseQuery = GetBaseQuery(false);
            var translator = new SqlTranslator<IContentType>(baseQuery, query);
            var sql = translator.Translate();
            var ids = Database.Fetch<int>(sql).Distinct().ToArray();

            return ids.Length > 0 ? GetMany(ids).OrderBy(x => x.Name) : Enumerable.Empty<IContentType>();
        }

        /// <inheritdoc />
        public IEnumerable<IContentType> GetByQuery(IQuery<PropertyType> query)
        {
            var ints = PerformGetByQuery(query).ToArray();
            return ints.Length > 0 ? GetMany(ints) : Enumerable.Empty<IContentType>();
        }

        protected IEnumerable<int> PerformGetByQuery(IQuery<PropertyType> query)
        {
            // used by DataTypeService to remove properties
            // from content types if they have a deleted data type - see
            // notes in DataTypeService.Delete as it's a bit weird

            var sqlClause = Sql()
                .SelectAll()
                .From<PropertyTypeGroupDto>()
                .RightJoin<PropertyTypeDto>()
                .On<PropertyTypeGroupDto, PropertyTypeDto>(left => left.Id, right => right.PropertyTypeGroupId)
                .InnerJoin<DataTypeDto>()
                .On<PropertyTypeDto, DataTypeDto>(left => left.DataTypeId, right => right.NodeId);

            var translator = new SqlTranslator<PropertyType>(sqlClause, query);
            var sql = translator.Translate()
                .OrderBy<PropertyTypeDto>(x => x.PropertyTypeGroupId);

            return Database
                .FetchOneToMany<PropertyTypeGroupDto>(x => x.PropertyTypeDtos, sql)
                .Select(x => x.ContentTypeNodeId).Distinct();
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
                sql = sql.WhereIn<NodeDto>(dto => dto.NodeObjectType, objectTypes);
            }

            return Database.Fetch<string>(sql);
        }

        public IEnumerable<int> GetAllContentTypeIds(string[] aliases)
        {
            if (aliases.Length == 0) return Enumerable.Empty<int>();

            var sql = Sql()
                .Select<ContentTypeDto>(x => x.NodeId)
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
                : sql.Select<ContentTypeDto>(x => x.NodeId);

            sql
                .From<ContentTypeDto>()
                .InnerJoin<NodeDto>().On<ContentTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .LeftJoin<ContentTypeTemplateDto>().On<ContentTypeTemplateDto, ContentTypeDto>(left => left.ContentTypeNodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);

            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return $"{Constants.DatabaseSchema.Tables.Node}.id = @id";
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
                Logger.Error<ContentTypeRepository,string>("ContentType '{EntityName}' cannot have an empty Alias. This is most likely due to invalid characters stripped from the Alias.", entity.Name);
                throw ex;
            }

            entity.AddingEntity();

            PersistNewBaseContentType(entity);
            PersistTemplates(entity, false);
            PersistHistoryCleanup(entity);

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
            entity.UpdatingEntity();

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
            PersistHistoryCleanup(entity);

            entity.ResetDirtyProperties();
        }

        private void PersistHistoryCleanup(IContentType entity)
        {
            // historyCleanup property is not mandatory for api endpoint, handle the case where it's not present.
            // DocumentTypeSave doesn't handle this for us like ContentType constructors do.
            ContentVersionCleanupPolicyDto dto = new ContentVersionCleanupPolicyDto()
            {
                ContentTypeId = entity.Id,
                Updated = DateTime.Now,
                PreventCleanup = entity.HistoryCleanup?.PreventCleanup ?? false,
                KeepAllVersionsNewerThanDays = entity.HistoryCleanup?.KeepAllVersionsNewerThanDays,
                KeepLatestVersionPerDayForDays = entity.HistoryCleanup?.KeepLatestVersionPerDayForDays,
            };

            Database.InsertOrUpdate(dto);
        }
    }
}
