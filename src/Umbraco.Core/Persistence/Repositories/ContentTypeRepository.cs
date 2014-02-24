using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="IContentType"/>
    /// </summary>
    internal class ContentTypeRepository : ContentTypeBaseRepository<int, IContentType>, IContentTypeRepository
    {
        private readonly ITemplateRepository _templateRepository;

		public ContentTypeRepository(IDatabaseUnitOfWork work, ITemplateRepository templateRepository)
            : base(work)
        {
            _templateRepository = templateRepository;
        }

		public ContentTypeRepository(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache, ITemplateRepository templateRepository)
            : base(work, cache)
        {
            _templateRepository = templateRepository;
        }

        #region Overrides of RepositoryBase<int,IContentType>

        protected override IContentType PerformGet(int id)
        {
            var contentTypeSql = GetBaseQuery(false);
            contentTypeSql.Where(GetBaseWhereClause(), new { Id = id });

            // The SQL will contain one record for each allowed template, so order to put the default one
            // at the top to populate the default template property correctly.
            contentTypeSql.OrderByDescending<DocumentTypeDto>(x => x.IsDefault);

            var dto = Database.Fetch<DocumentTypeDto, ContentTypeDto, NodeDto>(contentTypeSql).FirstOrDefault();

            if (dto == null)
                return null;

            var factory = new ContentTypeFactory(NodeObjectTypeId);
            var contentType = factory.BuildEntity(dto);

            contentType.AllowedContentTypes = GetAllowedContentTypeIds(id);
            contentType.PropertyGroups = GetPropertyGroupCollection(id, contentType.CreateDate, contentType.UpdateDate);
            ((ContentType)contentType).PropertyTypes = GetPropertyTypeCollection(id, contentType.CreateDate, contentType.UpdateDate);

            var templates = Database.Fetch<DocumentTypeDto>("WHERE contentTypeNodeId = @Id", new { Id = id });
            if(templates.Any())
            {
                contentType.AllowedTemplates = templates.Select(template => _templateRepository.Get(template.TemplateNodeId)).ToList();
            }

            var list = Database.Fetch<ContentType2ContentTypeDto>("WHERE childContentTypeId = @Id", new { Id = id});
            foreach (var contentTypeDto in list)
            {
                bool result = contentType.AddContentType(Get(contentTypeDto.ParentId));
                //Do something if adding fails? (Should hopefully not be possible unless someone created a circular reference)
            }

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            ((Entity)contentType).ResetDirtyProperties(false);
            return contentType;
        }

        protected override IEnumerable<IContentType> PerformGetAll(params int[] ids)
        {
            if (ids.Any())
            {
                foreach (var id in ids)
                {
                    yield return Get(id);
                }
            }
            else
            {
                var nodeDtos = Database.Fetch<NodeDto>("WHERE nodeObjectType = @NodeObjectType", new { NodeObjectType = NodeObjectTypeId });
                foreach (var nodeDto in nodeDtos)
                {
                    yield return Get(nodeDto.NodeId);
                }
            }
        }

        protected override IEnumerable<IContentType> PerformGetByQuery(IQuery<IContentType> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IContentType>(sqlClause, query);
            var sql = translator.Translate()
                .OrderBy<NodeDto>(x => x.Text);

            var dtos = Database.Fetch<DocumentTypeDto, ContentTypeDto, NodeDto>(sql);

            foreach (var dto in dtos.DistinctBy(x => x.ContentTypeDto.NodeId))
            {
                yield return Get(dto.ContentTypeDto.NodeId);
            }
        }

        #endregion

        public IEnumerable<IContentType> GetByQuery(IQuery<PropertyType> query)
        {
            var ints = PerformGetByQuery(query);
            foreach (var i in ints)
            {
                yield return Get(i);
            }
        }

        #region Overrides of PetaPocoRepositoryBase<int,IContentType>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*")
               .From<DocumentTypeDto>()
               .RightJoin<ContentTypeDto>()
               .On<ContentTypeDto, DocumentTypeDto>(left => left.NodeId, right => right.ContentTypeNodeId)
               .InnerJoin<NodeDto>()
               .On<ContentTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
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
                               "DELETE FROM cmsContentType WHERE NodeId = @Id",
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

                                            LogHelper.Error<ContentTypeRepository>(message, exception);
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
                Database.Insert(new DocumentTypeDto
                                    {ContentTypeNodeId = entity.Id, TemplateNodeId = template.Id, IsDefault = false});
            }

            ((ICanBeDirty)entity).ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IContentType entity)
        {
            ValidateAlias(entity);

            //Updates Modified date
            ((ContentType)entity).UpdatingEntity();

            //Look up parent to get and set the correct Path if ParentId has changed
            if (((ICanBeDirty)entity).IsPropertyDirty("ParentId"))
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
            Database.Delete<DocumentTypeDto>("WHERE contentTypeNodeId = @Id", new { Id = entity.Id});
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

            ((ICanBeDirty)entity).ResetDirtyProperties();
        }

        #endregion
    }
}