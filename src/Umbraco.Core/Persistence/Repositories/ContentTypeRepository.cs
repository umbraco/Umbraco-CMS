using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;

using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Relators;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

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

        #region Overrides of RepositoryBase<int,IContentType>
        
        protected override IContentType PerformGet(int id)
        {
            var contentTypes = ContentTypeQueryMapper.GetContentTypes(
                new[] {id}, Database, SqlSyntax, this, _templateRepository);
            
            var contentType = contentTypes.SingleOrDefault();
            return contentType;
        }

        protected override IEnumerable<IContentType> PerformGetAll(params int[] ids)
        {
            if (ids.Any())
            {
                return ContentTypeQueryMapper.GetContentTypes(ids, Database, SqlSyntax, this, _templateRepository);
            }
            else
            {
                var sql = new Sql().Select("id").From<NodeDto>().Where<NodeDto>(dto => dto.NodeObjectType == NodeObjectTypeId);
                var allIds = Database.Fetch<int>(sql).ToArray();
                return ContentTypeQueryMapper.GetContentTypes(allIds, Database, SqlSyntax, this, _templateRepository);
            }
        }

        protected override IEnumerable<IContentType> PerformGetByQuery(IQuery<IContentType> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IContentType>(sqlClause, query);
            var sql = translator.Translate()
                .OrderBy<NodeDto>(x => x.Text);

            var dtos = Database.Fetch<DocumentTypeDto, ContentTypeDto, NodeDto>(sql);
            return dtos.Any()
                ? GetAll(dtos.DistinctBy(x => x.ContentTypeDto.NodeId).Select(x => x.ContentTypeDto.NodeId).ToArray())
                : Enumerable.Empty<IContentType>();
        }

        #endregion

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

        #region Overrides of PetaPocoRepositoryBase<int,IContentType>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();

            sql.Select(isCount ? "COUNT(*)" : "*")
               .From<ContentTypeDto>()
               .InnerJoin<NodeDto>()
               .On<ContentTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
               .LeftJoin<DocumentTypeDto>()
               .On<DocumentTypeDto, ContentTypeDto>(left => left.ContentTypeNodeId, right => right.NodeId)
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

        #endregion

        #region Unit of Work Implementation

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

            var factory = new ContentTypeFactory(NodeObjectTypeId);
            var dto = factory.BuildDto(entity);

            PersistNewBaseContentType(dto.ContentTypeDto, entity);
            //Inserts data into the cmsDocumentType table if a template exists
            if (dto.TemplateNodeId > 0)
            {
                dto.ContentTypeNodeId = entity.Id;
                Database.Insert(dto);
            }

            //Insert allowed Templates not including the default one, as that has already been inserted
            foreach (var template in entity.AllowedTemplates.Where(x => x != null && x.Id != dto.TemplateNodeId))
            {
                Database.Insert(new DocumentTypeDto { ContentTypeNodeId = entity.Id, TemplateNodeId = template.Id, IsDefault = false });
            }

            entity.ResetDirtyProperties();
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

            var factory = new ContentTypeFactory(NodeObjectTypeId);
            var dto = factory.BuildDto(entity);

            PersistUpdatedBaseContentType(dto.ContentTypeDto, entity);

            //Look up DocumentType entries for updating - this could possibly be a "remove all, insert all"-approach
            Database.Delete<DocumentTypeDto>("WHERE contentTypeNodeId = @Id", new { Id = entity.Id });
            //Insert the updated DocumentTypeDto if a template exists
            if (dto.TemplateNodeId > 0)
            {
                Database.Insert(dto);
            }

            //Insert allowed Templates not including the default one, as that has already been inserted
            foreach (var template in entity.AllowedTemplates.Where(x => x != null && x.Id != dto.TemplateNodeId))
            {
                Database.Insert(new DocumentTypeDto { ContentTypeNodeId = entity.Id, TemplateNodeId = template.Id, IsDefault = false });
            }

            entity.ResetDirtyProperties();
        }

        #endregion

        
    }
}