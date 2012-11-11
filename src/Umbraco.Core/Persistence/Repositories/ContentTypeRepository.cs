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
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="IContentType"/>
    /// </summary>
    internal class ContentTypeRepository : ContentTypeBaseRepository<int, IContentType>, IContentTypeRepository
    {
        private readonly ITemplateRepository _templateRepository;

        public ContentTypeRepository(IUnitOfWork work, ITemplateRepository templateRepository)
            : base(work)
        {
            _templateRepository = templateRepository;
        }

        public ContentTypeRepository(IUnitOfWork work, IRepositoryCacheProvider cache, ITemplateRepository templateRepository)
            : base(work, cache)
        {
            _templateRepository = templateRepository;
        }

        #region Overrides of RepositoryBase<int,IContentType>

        protected override IContentType PerformGet(int id)
        {
            var contentTypeSql = GetBaseQuery(false);
            contentTypeSql.Where(GetBaseWhereClause(), new { Id = id });

            var dto = Database.Query<DocumentTypeDto, ContentTypeDto, NodeDto>(contentTypeSql).FirstOrDefault();

            if (dto == null)
                return null;

            var factory = new ContentTypeFactory(NodeObjectTypeId);
            var contentType = factory.BuildEntity(dto);

            contentType.AllowedContentTypes = GetAllowedContentTypeIds(id);
            contentType.PropertyGroups = GetPropertyGroupCollection(id);

            var templates = Database.Fetch<DocumentTypeDto>("WHERE contentTypeNodeId = @Id", new { Id = id });
            if(templates.Any())
            {
                contentType.AllowedTemplates =
                    templates.Select(template => _templateRepository.Get(template.TemplateNodeId));
            }

            var list = Database.Fetch<ContentType2ContentTypeDto>("WHERE childContentTypeId = @Id", new { Id = id});
            foreach (var contentTypeDto in list)
            {
                bool result = contentType.AddContentType(Get(contentTypeDto.ParentId));
                //Do something if adding fails? (Should hopefully not be possible unless someone created a circular reference)
            }

            ((ICanBeDirty)contentType).ResetDirtyProperties();
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
            var sql = translator.Translate();

            var documentTypeDtos = Database.Fetch<DocumentTypeDto, ContentTypeDto, NodeDto>(sql);

            foreach (var dto in documentTypeDtos)
            {
                yield return Get(dto.ContentTypeNodeId);
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
            //NOTE: If IsDefault=true we won't get ContentTypes like File, Folder etc. but only DocumentTypes.
            //Which is why "AND cmsDocumentType.IsDefault = @IsDefault" has been removed from sql below.
            //But might need to add it if we create a MediaTypeRepository
            //NOTE: Think the above is incorrect as ContentType and MediaType have different NodeObjectTypes.
            //UPDATE The above is no longer relevant as the ContentType/DocumentType and MediaType repos are not the same.
            sql.Select(isCount ? "COUNT(*)" : "*");
            sql.From("cmsDocumentType");
            sql.RightJoin("cmsContentType ON ([cmsContentType].[nodeId] = [cmsDocumentType].[contentTypeNodeId])");
            sql.InnerJoin("umbracoNode ON ([cmsContentType].[nodeId] = [umbracoNode].[id])");
            sql.Where("[umbracoNode].[nodeObjectType] = @NodeObjectType", new { NodeObjectType = NodeObjectTypeId });
            sql.Where("[cmsDocumentType].[IsDefault] = @IsDefault", new { IsDefault = true });
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "[umbracoNode].[id] = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                           {
                               string.Format("DELETE FROM umbracoUser2NodeNotify WHERE nodeId = @Id"),
                               string.Format("DELETE FROM umbracoUser2NodePermission WHERE nodeId = @Id"),
                               string.Format("DELETE FROM cmsTagRelationship WHERE nodeId = @Id"),
                               string.Format("DELETE FROM cmsContentTypeAllowedContentType WHERE Id = @Id"),
                               string.Format("DELETE FROM cmsContentType2ContentType WHERE parentContentTypeId = @Id"),
                               string.Format("DELETE FROM cmsContentType2ContentType WHERE childContentTypeId = @Id"),
                               string.Format("DELETE FROM cmsPropertyType WHERE contentTypeId = @Id"),
                               string.Format("DELETE FROM cmsPropertyTypeGroup WHERE contenttypeNodeId = @Id"),
                               string.Format("DELETE FROM cmsDocumentType WHERE contentTypeNodeId = @Id"),
                               string.Format("DELETE FROM cmsContentType WHERE NodeId = @Id"),
                               string.Format("DELETE FROM umbracoNode WHERE id = @Id")
                           };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { return new Guid("A2CB7800-F571-4787-9638-BC48539A0EFB"); }
        }

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(IContentType entity)
        {
            ((ContentType)entity).AddingEntity();

            var factory = new ContentTypeFactory(NodeObjectTypeId);
            var dto = factory.BuildDto(entity);

            PersistNewBaseContentType(dto.ContentTypeDto, entity);
            //Inserts data into the cmsDocumentType table
            dto.ContentTypeNodeId = entity.Id;
            Database.Insert(dto);

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
            //Updates Modified date
            ((ContentType)entity).UpdatingEntity();

            var factory = new ContentTypeFactory(NodeObjectTypeId);
            var dto = factory.BuildDto(entity);

            PersistUpdatedBaseContentType(dto.ContentTypeDto, entity);

            //Look up DocumentType entries for updating - this could possibly be a "remove all, insert all"-approach
            Database.Delete<DocumentTypeDto>("WHERE contentTypeNodeId = @Id", new { Id = entity.Id});

            Database.Insert(dto);

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