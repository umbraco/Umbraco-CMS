using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Relators;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="IMemberType"/>
    /// </summary>
    internal class MemberTypeRepository : ContentTypeBaseRepository<IMemberType>, IMemberTypeRepository
    {

        public MemberTypeRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
            : base(work, cache, logger, sqlSyntax)
        {
        }

        #region Overrides of RepositoryBase<int, IMemberType>

        protected override IMemberType PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });
            sql.OrderByDescending<NodeDto>(x => x.NodeId);

            var dtos =
                Database.Fetch<MemberTypeReadOnlyDto, PropertyTypeReadOnlyDto, PropertyTypeGroupReadOnlyDto, MemberTypeReadOnlyDto>(
                    new PropertyTypePropertyGroupRelator().Map, sql);

            if (dtos == null || dtos.Any() == false)
                return null;

            var factory = new MemberTypeReadOnlyFactory();
            var member = factory.BuildEntity(dtos.First());

            return member;
        }

        protected override IEnumerable<IMemberType> PerformGetAll(params int[] ids)
        {
            var sql = GetBaseQuery(false);
            if (ids.Any())
            {
                var statement = string.Join(" OR ", ids.Select(x => string.Format("umbracoNode.id='{0}'", x)));
                sql.Where(statement);
            }
            sql.OrderByDescending<NodeDto>(x => x.NodeId);

            var dtos =
                Database.Fetch<MemberTypeReadOnlyDto, PropertyTypeReadOnlyDto, PropertyTypeGroupReadOnlyDto, MemberTypeReadOnlyDto>(
                    new PropertyTypePropertyGroupRelator().Map, sql);

            return BuildFromDtos(dtos);
        }

        protected override IEnumerable<IMemberType> PerformGetByQuery(IQuery<IMemberType> query)
        {
            var sqlSubquery = GetSubquery();
            var translator = new SqlTranslator<IMemberType>(sqlSubquery, query);
            var subquery = translator.Translate();
            var sql = GetBaseQuery(false)
                .Append(new Sql("WHERE umbracoNode.id IN (" + subquery.SQL + ")", subquery.Arguments))
                .OrderBy<NodeDto>(x => x.SortOrder);

            var dtos =
                Database.Fetch<MemberTypeReadOnlyDto, PropertyTypeReadOnlyDto, PropertyTypeGroupReadOnlyDto, MemberTypeReadOnlyDto>(
                    new PropertyTypePropertyGroupRelator().Map, sql);

            return BuildFromDtos(dtos);
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<int, IMemberType>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();

            if (isCount)
            {
                sql.Select("COUNT(*)")
                    .From<NodeDto>()
                    .InnerJoin<ContentTypeDto>().On<ContentTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                    .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
                return sql;
            }

            sql.Select("umbracoNode.*", "cmsContentType.*", "cmsPropertyType.id AS PropertyTypeId", "cmsPropertyType.Alias",
                "cmsPropertyType.Name", "cmsPropertyType.Description", "cmsPropertyType.mandatory",
                "cmsPropertyType.validationRegExp", "cmsPropertyType.dataTypeId", "cmsPropertyType.sortOrder AS PropertyTypeSortOrder",
                "cmsPropertyType.propertyTypeGroupId AS PropertyTypesGroupId", "cmsMemberType.memberCanEdit", "cmsMemberType.viewOnProfile",
                "cmsDataType.propertyEditorAlias", "cmsDataType.dbType", "cmsPropertyTypeGroup.id AS PropertyTypeGroupId", 
                "cmsPropertyTypeGroup.text AS PropertyGroupName", "cmsPropertyTypeGroup.parentGroupId",
                "cmsPropertyTypeGroup.sortorder AS PropertyGroupSortOrder", "cmsPropertyTypeGroup.contenttypeNodeId")
                .From<NodeDto>()
                .InnerJoin<ContentTypeDto>().On<ContentTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .LeftJoin<PropertyTypeDto>().On<PropertyTypeDto, NodeDto>(left => left.ContentTypeId, right => right.NodeId)
                .LeftJoin<MemberTypeDto>().On<MemberTypeDto, PropertyTypeDto>(left => left.PropertyTypeId, right => right.Id)
                .LeftJoin<DataTypeDto>().On<DataTypeDto, PropertyTypeDto>(left => left.DataTypeId, right => right.DataTypeId)
                .LeftJoin<PropertyTypeGroupDto>().On<PropertyTypeGroupDto, NodeDto>(left => left.ContentTypeNodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
            return sql;
        }

        protected Sql GetSubquery()
        {
            var sql = new Sql()
                .Select("DISTINCT(umbracoNode.id)")
                .From<NodeDto>()
                .InnerJoin<ContentTypeDto>().On<ContentTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .LeftJoin<PropertyTypeDto>().On<PropertyTypeDto, NodeDto>(left => left.ContentTypeId, right => right.NodeId)
                .LeftJoin<MemberTypeDto>().On<MemberTypeDto, PropertyTypeDto>(left => left.PropertyTypeId, right => right.Id)
                .LeftJoin<DataTypeDto>().On<DataTypeDto, PropertyTypeDto>(left => left.DataTypeId, right => right.DataTypeId)
                .LeftJoin<PropertyTypeGroupDto>().On<PropertyTypeGroupDto, NodeDto>(left => left.ContentTypeNodeId, right => right.NodeId)
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
                               "DELETE FROM cmsMemberType WHERE NodeId = @Id",
                               "DELETE FROM cmsContentType WHERE nodeId = @Id",
                               "DELETE FROM umbracoNode WHERE id = @Id"
                           };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { return new Guid(Constants.ObjectTypes.MemberType); }
        }

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(IMemberType entity)
        {
            ValidateAlias(entity);

            ((MemberType)entity).AddingEntity();
            
            //set a default icon if one is not specified
            if (entity.Icon.IsNullOrWhiteSpace())
            {
                entity.Icon = "icon-user";
            }

            //By Convention we add 9 stnd PropertyTypes to an Umbraco MemberType
            entity.AddPropertyGroup(Constants.Conventions.Member.StandardPropertiesGroupName);
            var standardPropertyTypes = Constants.Conventions.Member.GetStandardPropertyTypeStubs();
            foreach (var standardPropertyType in standardPropertyTypes)
            {
                entity.AddPropertyType(standardPropertyType.Value, Constants.Conventions.Member.StandardPropertiesGroupName);
            }

            var factory = new MemberTypeFactory(NodeObjectTypeId);
            var dto = factory.BuildDto(entity);

            EnsureExplicitDataTypeForBuiltInProperties(entity);

            PersistNewBaseContentType(dto, entity);

            //Handles the MemberTypeDto (cmsMemberType table)
            var memberTypeDtos = factory.BuildMemberTypeDtos(entity);
            foreach (var memberTypeDto in memberTypeDtos)
            {
                Database.Insert(memberTypeDto);
            }

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IMemberType entity)
        {
            ValidateAlias(entity);

            //Updates Modified date
            ((MemberType)entity).UpdatingEntity();

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

            var factory = new MemberTypeFactory(NodeObjectTypeId);
            var dto = factory.BuildDto(entity);

            EnsureExplicitDataTypeForBuiltInProperties(entity);

            PersistUpdatedBaseContentType(dto, entity);

            //Remove existing entries before inserting new ones
            Database.Delete<MemberTypeDto>("WHERE NodeId = @Id", new { Id = entity.Id });

            //Handles the MemberTypeDto (cmsMemberType table)
            var memberTypeDtos = factory.BuildMemberTypeDtos(entity);
            foreach (var memberTypeDto in memberTypeDtos)
            {
                Database.Insert(memberTypeDto);
            }

            entity.ResetDirtyProperties();
        }

        #endregion

        /// <summary>
        /// Override so we can specify explicit db type's on any property types that are built-in.
        /// </summary>
        /// <param name="propertyEditorAlias"></param>
        /// <param name="dbType"></param>
        /// <param name="propertyTypeAlias"></param>
        /// <returns></returns>
        protected override PropertyType CreatePropertyType(string propertyEditorAlias, DataTypeDatabaseType dbType, string propertyTypeAlias)
        {
            //custom property type constructor logic to set explicit dbtype's for built in properties
            var stdProps = Constants.Conventions.Member.GetStandardPropertyTypeStubs();
            var propDbType = GetDbTypeForBuiltInProperty(propertyTypeAlias, dbType, stdProps);
            return new PropertyType(propertyEditorAlias, propDbType.Result,
                //This flag tells the property type that it has an explicit dbtype and that it cannot be changed
                // which is what we want for the built-in properties.
                propDbType.Success,
                propertyTypeAlias);
        }

        /// <summary>
        /// Ensure that all the built-in membership provider properties have their correct data type
        /// and property editors assigned. This occurs prior to saving so that the correct values are persisted.
        /// </summary>
        /// <param name="memberType"></param>
        private static void EnsureExplicitDataTypeForBuiltInProperties(IContentTypeBase memberType)
        {
            var stdProps = Constants.Conventions.Member.GetStandardPropertyTypeStubs();
            foreach (var propertyType in memberType.PropertyTypes)
            {
                var dbTypeAttempt = GetDbTypeForBuiltInProperty(propertyType.Alias, propertyType.DataTypeDatabaseType, stdProps);
                if (dbTypeAttempt)
                {
                    //this reset's it's current data type reference which will be re-assigned based on the property editor assigned on the next line
                    propertyType.DataTypeDefinitionId = 0;
                    propertyType.DataTypeId = GetPropertyEditorForBuiltInProperty(propertyType.Alias, propertyType.DataTypeId, stdProps).Result;
                }
            }
        }

        /// <summary>
        /// Builds a collection of entities from a collection of Dtos
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        private static IEnumerable<IMemberType> BuildFromDtos(List<MemberTypeReadOnlyDto> dtos)
        {
            if (dtos == null || dtos.Any() == false)
                return Enumerable.Empty<IMemberType>();

            var factory = new MemberTypeReadOnlyFactory();
            return dtos.Select(factory.BuildEntity);
        }

        /// <summary>
        /// If this is one of our internal properties - we will manually assign the data type since they must 
        /// always correspond to the correct db type no matter what the backing data type is assigned.
        /// </summary>
        /// <param name="propAlias"></param>
        /// <param name="dbType"></param>
        /// <param name="standardProps"></param>
        /// <returns>
        /// Successful attempt if it was a built in property
        /// </returns>
        internal static Attempt<DataTypeDatabaseType> GetDbTypeForBuiltInProperty(
            string propAlias,
            DataTypeDatabaseType dbType,
            Dictionary<string, PropertyType> standardProps)
        {
            var aliases = standardProps.Select(x => x.Key).ToArray();

            //check if it is built in
            if (aliases.Contains(propAlias))
            {
                //return the pre-determined db type for this property
                return Attempt<DataTypeDatabaseType>.Succeed(standardProps.Single(x => x.Key == propAlias).Value.DataTypeDatabaseType);
            }

            return Attempt<DataTypeDatabaseType>.Fail(dbType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propAlias"></param>
        /// <param name="propertyEditor"></param>
        /// <param name="standardProps"></param>
        /// <returns>
        /// Successful attempt if it was a built in property
        /// </returns>
        internal static Attempt<Guid> GetPropertyEditorForBuiltInProperty(
            string propAlias,
            Guid propertyEditor,
            Dictionary<string, PropertyType> standardProps)
        {
            var aliases = standardProps.Select(x => x.Key).ToArray();

            //check if it is built in
            if (aliases.Contains(propAlias))
            {
                //return the pre-determined db type for this property
                return Attempt<Guid>.Succeed(standardProps.Single(x => x.Key == propAlias).Value.DataTypeId);
            }

            return Attempt<Guid>.Fail(propertyEditor);
        }
    }
}