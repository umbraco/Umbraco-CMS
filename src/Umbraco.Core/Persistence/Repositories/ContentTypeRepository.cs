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
using Umbraco.Core.Persistence.Relators;
using Umbraco.Core.Persistence.SqlSyntax;
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
            var contentTypes = ContentTypeQueryMapper.GetContentTypes(
                new[] {id}, Database, this, _templateRepository);
            
            var contentType = contentTypes.SingleOrDefault();
            return contentType;
        }

        protected override IEnumerable<IContentType> PerformGetAll(params int[] ids)
        {
            if (ids.Any())
            {
                return ContentTypeQueryMapper.GetContentTypes(ids, Database, this, _templateRepository);
            }
            else
            {
                var sql = new Sql().Select("id").From<NodeDto>().Where<NodeDto>(dto => dto.NodeObjectType == NodeObjectTypeId);
                var allIds = Database.Fetch<int>(sql).ToArray();
                return ContentTypeQueryMapper.GetContentTypes(allIds, Database, this, _templateRepository);
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
               .Where<NodeDto>(x => x.NodeObjectType == new Guid(Constants.ObjectTypes.DocumentType));

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
                Database.Insert(new DocumentTypeDto { ContentTypeNodeId = entity.Id, TemplateNodeId = template.Id, IsDefault = false });
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

            ((ICanBeDirty)entity).ResetDirtyProperties();
        }

        #endregion

        internal static class ContentTypeQueryMapper
        {

            public class AssociatedTemplate
            {
                public AssociatedTemplate(int templateId, string @alias, string templateName)
                {
                    TemplateId = templateId;
                    Alias = alias;
                    TemplateName = templateName;
                }

                public int TemplateId { get; set; }
                public string Alias { get; set; }
                public string TemplateName { get; set; }

                protected bool Equals(AssociatedTemplate other)
                {
                    return TemplateId == other.TemplateId;
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj)) return false;
                    if (ReferenceEquals(this, obj)) return true;
                    if (obj.GetType() != this.GetType()) return false;
                    return Equals((AssociatedTemplate) obj);
                }

                public override int GetHashCode()
                {
                    return TemplateId;
                }
            }

            public static IEnumerable<IContentType> GetContentTypes(
                int[] contentTypeIds, Database db,
                IContentTypeRepository contentTypeRepository,
                ITemplateRepository templateRepository)
            {
                IDictionary<int, IEnumerable<AssociatedTemplate>> allAssociatedTemplates;
                IDictionary<int, IEnumerable<int>> allParentContentTypeIds;
                var contentTypes = MapContentTypes(contentTypeIds, db, out allAssociatedTemplates, out allParentContentTypeIds)
                    .ToArray();

                foreach (var contentType in contentTypes)
                {
                    var associatedTemplates = allAssociatedTemplates[contentType.Id];
                    var parentContentTypeIds = allParentContentTypeIds[contentType.Id];

                    MapContentTypeTemplatesAndChildren(
                        contentType, db, contentTypeRepository, templateRepository, associatedTemplates, parentContentTypeIds);

                    //on initial construction we don't want to have dirty properties tracked
                    // http://issues.umbraco.org/issue/U4-1946
                    ((Entity)contentType).ResetDirtyProperties(false);    
                }
                
                return contentTypes;
            }

            internal static void MapContentTypeTemplatesAndChildren(IContentType contentType,
                Database db,
                IContentTypeRepository contentTypeRepository,
                ITemplateRepository templateRepository,
                IEnumerable<AssociatedTemplate> associatedTemplates, 
                IEnumerable<int> parentContentTypeIds)
            {
                //NOTE: SQL call #2

                PropertyGroupCollection propGroups;
                PropertyTypeCollection propTypes;
                MapGroupsAndProperties(contentType.Id, db, out propTypes, out propGroups);
                contentType.PropertyGroups = propGroups;
                ((ContentType)contentType).PropertyTypes = propTypes;

                //NOTE: SQL call #3++
                //SEE: http://issues.umbraco.org/issue/U4-5174 to fix this

                var templateIds = associatedTemplates.Select(x => x.TemplateId).ToArray();
                var templates = templateIds.Any()
                    ? templateRepository.GetAll()
                    : Enumerable.Empty<ITemplate>();
                contentType.AllowedTemplates = templates.ToArray();

                //NOTE: SQL call #4++

                var parentIdsAsArray = parentContentTypeIds.ToArray();
                if (parentIdsAsArray.Any())
                {
                    var parentContentTypes = contentTypeRepository.GetAll(parentIdsAsArray);
                    foreach (var parentContentType in parentContentTypes)
                    {
                        var result = contentType.AddContentType(parentContentType);
                        //Do something if adding fails? (Should hopefully not be possible unless someone created a circular reference)    
                    }
                }

                //on initial construction we don't want to have dirty properties tracked
                // http://issues.umbraco.org/issue/U4-1946
                ((Entity)contentType).ResetDirtyProperties(false);
            }

            internal static IEnumerable<IContentType> MapContentTypes(int[] contentTypeIds, Database db,
                out IDictionary<int, IEnumerable<AssociatedTemplate>> associatedTemplates,
                out IDictionary<int, IEnumerable<int>> parentContentTypeIds)
            {
                Mandate.That(contentTypeIds.Any(), () => new InvalidOperationException("must be at least one content type id specified"));
                Mandate.ParameterNotNull(db, "db");
                
                //ensure they are unique
                contentTypeIds = contentTypeIds.Distinct().ToArray();

                var sql = @"SELECT cmsDocumentType.IsDefault as dtIsDefault, cmsDocumentType.templateNodeId as dtTemplateId,
		                        cmsContentType.pk as ctPk, cmsContentType.alias as ctAlias, cmsContentType.allowAtRoot as ctAllowAtRoot, cmsContentType.description as ctDesc,
		                        cmsContentType.icon as ctIcon, cmsContentType.isContainer as ctIsContainer, cmsContentType.nodeId as ctId, cmsContentType.thumbnail as ctThumb,
                                AllowedTypes.allowedId as ctaAllowedId, AllowedTypes.SortOrder as ctaSortOrder, AllowedTypes.alias as ctaAlias,		                        
                                ParentTypes.parentContentTypeId as chtParentId,
                                umbracoNode.createDate as nCreateDate, umbracoNode.[level] as nLevel, umbracoNode.nodeObjectType as nObjectType, umbracoNode.nodeUser as nUser,
		                        umbracoNode.parentID as nParentId, umbracoNode.[path] as nPath, umbracoNode.sortOrder as nSortOrder, umbracoNode.[text] as nName, umbracoNode.trashed as nTrashed,
		                        umbracoNode.uniqueID as nUniqueId,                                
		                        Template.alias as tAlias, Template.nodeId as tId,Template.text as tText
                        FROM cmsContentType
                        INNER JOIN umbracoNode
                        ON cmsContentType.nodeId = umbracoNode.id
                        LEFT JOIN cmsDocumentType
                        ON cmsDocumentType.contentTypeNodeId = cmsContentType.nodeId
                        LEFT JOIN (
	                        SELECT cmsContentTypeAllowedContentType.Id, cmsContentTypeAllowedContentType.AllowedId, cmsContentType.alias, cmsContentTypeAllowedContentType.SortOrder 
	                        FROM cmsContentTypeAllowedContentType	
	                        INNER JOIN cmsContentType
	                        ON cmsContentTypeAllowedContentType.AllowedId = cmsContentType.nodeId
                        ) AllowedTypes
                        ON AllowedTypes.Id = cmsContentType.nodeId
                        LEFT JOIN (
	                        SELECT * FROM cmsTemplate
	                        INNER JOIN umbracoNode
	                        ON cmsTemplate.nodeId = umbracoNode.id
                        ) as Template
                        ON Template.nodeId = cmsDocumentType.templateNodeId
                        LEFT JOIN cmsContentType2ContentType as ParentTypes
                        ON ParentTypes.childContentTypeId = cmsContentType.nodeId	
                        WHERE (umbracoNode.nodeObjectType = @nodeObjectType)
                        AND (umbracoNode.id IN (@contentTypeIds))";
                
                //NOTE: we are going to assume there's not going to be more than 2100 content type ids since that is the max SQL param count!
                // since there will be 2 params per single row it will be half that amount
                if ((contentTypeIds.Length - 1) > 2000)
                    throw new InvalidOperationException("Cannot perform this lookup, too many sql parameters");
                
                var result = db.Fetch<dynamic>(sql, new { nodeObjectType = new Guid(Constants.ObjectTypes.DocumentType), contentTypeIds = contentTypeIds });

                if (result.Any() == false)
                {
                    parentContentTypeIds = null;
                    associatedTemplates = null;
                    return Enumerable.Empty<IContentType>();
                }

                parentContentTypeIds = new Dictionary<int, IEnumerable<int>>();
                associatedTemplates = new Dictionary<int, IEnumerable<AssociatedTemplate>>();
                var mappedContentTypes = new List<IContentType>();

                foreach (var contentTypeId in contentTypeIds)
                {
                    //the current content type id that we're working with

                    var currentCtId = contentTypeId;

                    //first we want to get the main content type data this is 1 : 1 with umbraco node data

                    var ct = result
                        .Where(x => x.ctId == currentCtId)
                        .Select(x => new { x.ctPk, x.ctId, x.ctAlias, x.ctAllowAtRoot, x.ctDesc, x.ctIcon, x.ctIsContainer, x.ctThumb, x.nName, x.nCreateDate, x.nLevel, x.nObjectType, x.nUser, x.nParentId, x.nPath, x.nSortOrder, x.nTrashed, x.nUniqueId })
                        .DistinctBy(x => (int)x.ctId)
                        .FirstOrDefault();

                    if (ct == null)
                    {
                        continue;
                    }

                    //get the unique list of associated templates
                    var defaultTemplates = result
                        .Where(x => x.ctId == currentCtId)
                        //use a tuple so that distinct checks both values
                        .Select(x => new Tuple<bool?, int?>(x.dtIsDefault, x.dtTemplateId))
                        .Where(x => x.Item1.HasValue && x.Item2.HasValue)
                        .Distinct()
                        .OrderByDescending(x => x.Item1.Value)
                        .ToArray();
                    //if there isn't one set to default explicitly, we'll pick the first one
                    var defaultTemplate = defaultTemplates.FirstOrDefault(x => x.Item1.Value)
                        ?? defaultTemplates.FirstOrDefault();

                    var dtDto = new DocumentTypeDto
                    {
                        //create the content type dto
                        ContentTypeDto = new ContentTypeDto
                        {
                            Alias = ct.ctAlias,
                            AllowAtRoot = ct.ctAllowAtRoot,
                            Description = ct.ctDesc,
                            Icon = ct.ctIcon,
                            IsContainer = ct.ctIsContainer,
                            NodeId = ct.ctId,
                            PrimaryKey = ct.ctPk,
                            Thumbnail = ct.ctThumb,
                            //map the underlying node dto
                            NodeDto = new NodeDto
                            {
                                CreateDate = ct.nCreateDate,
                                Level = (short)ct.nLevel,
                                NodeId = ct.ctId,
                                NodeObjectType = ct.nObjectType,
                                ParentId = ct.nParentId,
                                Path = ct.nPath,
                                SortOrder = ct.nSortOrder,
                                Text = ct.nName,
                                Trashed = ct.nTrashed,
                                UniqueId = ct.nUniqueId,
                                UserId = ct.nUser
                            }
                        },
                        ContentTypeNodeId = ct.ctId,
                        IsDefault = defaultTemplate != null,
                        TemplateNodeId = defaultTemplate != null ? defaultTemplate.Item2.Value : 0,
                    };

                    // We will map a subset of the associated template - alias, id, name

                    associatedTemplates.Add(currentCtId, result
                        .Where(x => x.ctId == currentCtId)
                        .Where(x => x.tId != null)
                        .Select(x => new AssociatedTemplate(x.tId, x.tAlias, x.tText))
                        .Distinct()
                        .ToArray());

                    //now create the content type object

                    var factory = new ContentTypeFactory(new Guid(Constants.ObjectTypes.DocumentType));
                    var contentType = factory.BuildEntity(dtDto);

                    //map the allowed content types
                    contentType.AllowedContentTypes = result
                        .Where(x => x.ctId == currentCtId)
                        //use tuple so we can use distinct on all vals
                        .Select(x => new Tuple<int?, int?, string>(x.ctaAllowedId, x.ctaSortOrder, x.ctaAlias))
                        .Where(x => x.Item1.HasValue && x.Item2.HasValue && x.Item3 != null)
                        .Distinct()
                        .Select(x => new ContentTypeSort(new Lazy<int>(() => x.Item1.Value), x.Item2.Value, x.Item3))
                        .ToList();

                    mappedContentTypes.Add(contentType);

                    //map the child content type ids
                    parentContentTypeIds.Add(currentCtId, result
                        .Where(x => x.ctId == currentCtId)
                        .Select(x => (int?) x.chtParentId)
                        .Where(x => x.HasValue)
                        .Distinct()
                        .Select(x => x.Value).ToList());
                }

                return mappedContentTypes;
            }

            internal static void MapGroupsAndProperties(int contentTypeId, Database db, 
                out PropertyTypeCollection propertyTypeCollection, 
                out PropertyGroupCollection propertyGroupCollection)
            {
                // first part Gets all property groups including property type data even when no property type exists on the group
                // second part Gets all property types including ones that are not on a group
                // therefore the union of the two contains all of the property type and property group information we need

                var sql = @"SELECT PT.ptId, PT.ptAlias, PT.ptDesc,PT.ptHelpText,PT.ptMandatory,PT.ptName,PT.ptSortOrder,PT.ptRegExp, 
                            PT.dtId,PT.dtDbType,PT.dtPropEdAlias,
                            PG.id as pgId, PG.parentGroupId as pgParentGroupId, PG.sortorder as pgSortOrder, PG." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("text") + @" as pgText
                        FROM cmsPropertyTypeGroup as PG
                        LEFT JOIN
                        (
                            SELECT PT.id as ptId, PT.Alias as ptAlias, PT." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("Description") + @" as ptDesc, PT.helpText as ptHelpText,
                                    PT.mandatory as ptMandatory, PT.Name as ptName, PT.sortOrder as ptSortOrder, PT.validationRegExp as ptRegExp,
                                    PT.propertyTypeGroupId as ptGroupId,
                                    DT.dbType as dtDbType, DT.nodeId as dtId, DT.propertyEditorAlias as dtPropEdAlias
                            FROM cmsPropertyType as PT
                            INNER JOIN cmsDataType as DT
                            ON PT.dataTypeId = DT.nodeId
                        )  as  PT
                        ON PT.ptGroupId = PG.id
                        WHERE (PG.contenttypeNodeId = @contentTypeId)
                        
                        UNION

                        SELECT PT.id as ptId, PT.Alias as ptAlias, PT." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("Description") + @" as ptDesc, PT.helpText as ptHelpText,
                                PT.mandatory as ptMandatory, PT.Name as ptName, PT.sortOrder as ptSortOrder, PT.validationRegExp as ptRegExp,
                                DT.nodeId as dtId, DT.dbType as dtDbType, DT.propertyEditorAlias as dtPropEdAlias,
                                PG.id as pgId, PG.parentGroupId as pgParentGroupId, PG.sortorder as pgSortOrder, PG." + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("text") + @" as pgText
                        FROM cmsPropertyType as PT
                        INNER JOIN cmsDataType as DT
                        ON PT.dataTypeId = DT.[nodeId]
                        LEFT JOIN cmsPropertyTypeGroup as PG
                        ON PG.[id] = PT.propertyTypeGroupId
                        WHERE (PT.contentTypeId = @contentTypeId)

                        ORDER BY (PG.id)";

                var result = db.Fetch<dynamic>(sql, new { contentTypeId = contentTypeId });

                //from this we need to make :
                // * PropertyGroupCollection - Contains all property groups along with all property types associated with a group
                // * PropertyTypeCollection - Contains all property types that do not belong to a group

                //create the property group collection first, this means all groups (even empty ones) and all groups with properties

                propertyGroupCollection = new PropertyGroupCollection(result
                    //get all rows that have a group id
                    .Where(x => x.pgId != null)
                    //turn that into a custom object containing only the group info
                    .Select(x => new { GroupId = x.pgId, ParentGroupId = x.pgParentGroupId, SortOrder = x.pgSortOrder, Text = x.pgText })
                    //get distinct data by id
                    .DistinctBy(x => (int)x.GroupId)
                    //for each of these groups, create a group object with it's associated properties
                    .Select(group => new PropertyGroup(new PropertyTypeCollection(
                        result
                            .Where(row => row.pgId == group.GroupId && row.ptId != null)
                            .Select(row => new PropertyType(row.dtPropEdAlias, Enum<DataTypeDatabaseType>.Parse(row.dtDbType))
                            {
                                //fill in the rest of the property type properties
                                Alias = row.ptAlias,
                                Description = row.ptDesc,
                                DataTypeDefinitionId = row.dtId,
                                Id = row.ptId,
                                Mandatory = row.ptMandatory,
                                Name = row.ptName,
                                PropertyGroupId = new Lazy<int>(() => group.GroupId, false),
                                SortOrder = row.ptSortOrder,
                                ValidationRegExp = row.ptRegExp
                            })))
                    {
                        //fill in the rest of the group properties
                        Id = group.GroupId,
                        Name = group.Text,
                        ParentId = group.ParentGroupId,
                        SortOrder = group.SortOrder
                    }).ToArray());

                //Create the property type collection now (that don't have groups)

                propertyTypeCollection = new PropertyTypeCollection(result
                    .Where(x => x.pgId == null)
                    .Select(row => new PropertyType(row.dtPropEdAlias, Enum<DataTypeDatabaseType>.Parse(row.dtDbType))
                    {
                        //fill in the rest of the property type properties
                        Alias = row.ptAlias,
                        Description = row.ptDesc,
                        DataTypeDefinitionId = row.dtId,
                        Id = row.ptId,
                        Mandatory = row.ptMandatory,
                        Name = row.ptName,
                        PropertyGroupId = null,
                        SortOrder = row.ptSortOrder,
                        ValidationRegExp = row.ptRegExp
                    }).ToArray());
            }

        }
    }
}