using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;

using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Relators;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="IContentType"/>
    /// </summary>
    internal class ContentTypeRepository : ContentTypeBaseRepository<IContentType>, IContentTypeRepository
    {
        private readonly ITemplateRepository _templateRepository;

        public ContentTypeRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax, ITemplateRepository templateRepository)
            : base(work, cache, logger, sqlSyntax)
        {
            _templateRepository = templateRepository;
        }

        private FullDataSetRepositoryCachePolicyFactory<IContentType, int> _cachePolicyFactory;
        protected override IRepositoryCachePolicyFactory<IContentType, int> CachePolicyFactory
        {
            get
            {
                //Use a FullDataSet cache policy - this will cache the entire GetAll result in a single collection
                return _cachePolicyFactory ?? (_cachePolicyFactory = new FullDataSetRepositoryCachePolicyFactory<IContentType, int>(
                    RuntimeCache, GetEntityId, () => PerformGetAll(), 
                    //allow this cache to expire
                    expires:true));
            }
        }

        protected override IContentType PerformGet(int id)
        {
            //use the underlying GetAll which will force cache all content types
            return GetAll().FirstOrDefault(x => x.Id == id);
        }

        protected override IEnumerable<IContentType> PerformGetAll(params int[] ids)
        {
            if (ids.Any())
            {
                //NOTE: This logic should never be executed according to our cache policy
                return ContentTypeQueryMapper.GetContentTypes(Database, SqlSyntax, this, _templateRepository)
                    .Where(x => ids.Contains(x.Id));
            }

            return ContentTypeQueryMapper.GetContentTypes(Database, SqlSyntax, this, _templateRepository);
        }

        protected override IEnumerable<IContentType> PerformGetByQuery(IQuery<IContentType> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IContentType>(sqlClause, query);
            var sql = translator.Translate();                

            var dtos = Database.Fetch<ContentTypeTemplateDto, ContentTypeDto, NodeDto>(sql);

            return
                //This returns a lookup from the GetAll cached looup
                (dtos.Any()
                    ? GetAll(dtos.DistinctBy(x => x.ContentTypeDto.NodeId).Select(x => x.ContentTypeDto.NodeId).ToArray())
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
                ? GetAll(ints)
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
            var sql = new Sql().Select("cmsContentType.alias")
                .From<ContentTypeDto>(SqlSyntax)
                .InnerJoin<NodeDto>(SqlSyntax)
                .On<ContentTypeDto, NodeDto>(SqlSyntax, dto => dto.NodeId, dto => dto.NodeId);

            if (objectTypes.Any())
            {
                sql = sql.Where("umbracoNode.nodeObjectType IN (@objectTypes)", objectTypes);
            }

            return Database.Fetch<string>(sql);
        }

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();

            sql.Select(isCount ? "COUNT(*)" : "*")
               .From<ContentTypeDto>(SqlSyntax)
               .InnerJoin<NodeDto>(SqlSyntax)
               .On<ContentTypeDto, NodeDto>(SqlSyntax, left => left.NodeId, right => right.NodeId)
               .LeftJoin<ContentTypeTemplateDto>(SqlSyntax)
               .On<ContentTypeTemplateDto, ContentTypeDto>(SqlSyntax, left => left.ContentTypeNodeId, right => right.NodeId)
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
                               "DELETE FROM cmsDocumentType WHERE contentTypeNodeId = @Id",
                               "DELETE FROM cmsContentType WHERE nodeId = @Id",
                               "DELETE FROM umbracoNode WHERE id = @Id"
                           };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { return new Guid(Constants.ObjectTypes.DocumentType); }
        }
        
        /// <summary>
        /// Deletes a content type
        /// </summary>
        /// <param name="entity"></param>
        /// <remarks>
        /// First checks for children and removes those first
        /// </remarks>
        protected override void PersistDeletedItem(IContentType entity)
        {
            var query = Query<IContentType>.Builder.Where(x => x.ParentId == entity.Id);
            var children = GetByQuery(query);
            foreach (var child in children)
            {
                //NOTE: We must cast here so that it goes to the outter method to
                // ensure the cache is updated.
                PersistDeletedItem((IEntity)child);
            }

            //Before we call the base class methods to run all delete clauses, we need to first 
            // delete all of the property data associated with this document type. Normally this will
            // be done in the ContentTypeService by deleting all associated content first, but in some cases
            // like when we switch a document type, there is property data left over that is linked
            // to the previous document type. So we need to ensure it's removed.
            var sql = new Sql().Select("DISTINCT cmsPropertyData.propertytypeid")
                .From<PropertyDataDto>(SqlSyntax)
                .InnerJoin<PropertyTypeDto>(SqlSyntax)
                .On<PropertyDataDto, PropertyTypeDto>(SqlSyntax, dto => dto.PropertyTypeId, dto => dto.Id)
                .InnerJoin<ContentTypeDto>(SqlSyntax)
                .On<ContentTypeDto, PropertyTypeDto>(SqlSyntax, dto => dto.NodeId, dto => dto.ContentTypeId)
                .Where<ContentTypeDto>(dto => dto.NodeId == entity.Id);

            //Delete all cmsPropertyData where propertytypeid EXISTS in the subquery above
            Database.Execute(SqlSyntax.GetDeleteSubquery("cmsPropertyData", "propertytypeid", sql));

            base.PersistDeletedItem(entity);
        }

        protected override void PersistNewItem(IContentType entity)
        {
            Mandate.That<Exception>(string.IsNullOrEmpty(entity.Alias) == false,
                                    () =>
                                    {
                                        var message =
                                            string.Format(
                                                "ContentType '{0}' cannot have an empty Alias. This is most likely due to invalid characters stripped from the Alias.",
                                                entity.Name);
                                        var exception = new Exception(message);

                                        Logger.Error<ContentTypeRepository>(message, exception);
                                        throw exception;
                                    });

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
        
        protected override IContentType PerformGet(Guid id)
        {
            //use the underlying GetAll which will force cache all content types
            return GetAll().FirstOrDefault(x => x.Key == id);
        }

        protected override IContentType PerformGet(string alias)
        {
            //use the underlying GetAll which will force cache all content types
            return GetAll().FirstOrDefault(x => x.Alias.InvariantEquals(alias));
        }

        protected override IEnumerable<IContentType> PerformGetAll(params Guid[] ids)
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
    }
}