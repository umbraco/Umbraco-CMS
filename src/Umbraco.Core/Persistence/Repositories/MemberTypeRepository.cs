using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="IMemberType"/>
    /// </summary>
    internal class MemberTypeRepository : ContentTypeRepositoryBase<IMemberType>, IMemberTypeRepository
    {
        private IRepositoryCachePolicy<IMemberType, int> _cachePolicy;

        public MemberTypeRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, IMappingResolver mappingResolver)
            : base(work, cache, logger, mappingResolver)
        { }

        protected override IRepositoryCachePolicy<IMemberType, int> CachePolicy
        {
            get
            {
                if (_cachePolicy != null) return _cachePolicy;

                _cachePolicy = new FullDataSetRepositoryCachePolicy<IMemberType, int>(RuntimeCache, GetEntityId, /*expires:*/ true);

                return _cachePolicy;
            }
        }

        protected override IMemberType PerformGet(int id)
        {
            //use the underlying GetAll which will force cache all content types
            return GetAll().FirstOrDefault(x => x.Id == id);
        }

        protected override IMemberType PerformGet(Guid id)
        {
            //use the underlying GetAll which will force cache all content types
            return GetAll().FirstOrDefault(x => x.Key == id);
        }

        protected override IEnumerable<IMemberType> PerformGetAll(params Guid[] ids)
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

        protected override IMemberType PerformGet(string alias)
        {
            //use the underlying GetAll which will force cache all content types
            return GetAll().FirstOrDefault(x => x.Alias.InvariantEquals(alias));
        }

        protected override IEnumerable<IMemberType> PerformGetAll(params int[] ids)
        {
            var sql = GetBaseQuery(false);
            if (ids.Any())
            {
                //NOTE: This logic should never be executed according to our cache policy
                var statement = string.Join(" OR ", ids.Select(x => string.Format("umbracoNode.id='{0}'", x)));
                sql.Where(statement);
            }
            sql.OrderByDescending<NodeDto>(x => x.NodeId);

            var dtos = Database
                .Fetch<MemberTypeReadOnlyDto>(sql) // cannot use FetchOneToMany because we have 2 collections!
                .Transform(MapOneToManies)
                .ToList();

            return BuildFromDtos(dtos);
        }

        protected override IEnumerable<IMemberType> PerformGetByQuery(IQuery<IMemberType> query)
        {
            var sqlSubquery = GetSubquery();
            var translator = new SqlTranslator<IMemberType>(sqlSubquery, query);
            var subquery = translator.Translate();
            var sql = GetBaseQuery(false)
                .Append("WHERE umbracoNode.id IN (" + subquery.SQL + ")", subquery.Arguments)
                .OrderBy<NodeDto>(x => x.SortOrder);

            var dtos = Database
                .Fetch<MemberTypeReadOnlyDto>(sql) // cannot use FetchOneToMany because we have 2 collections!
                .Transform(MapOneToManies)
                .ToList();

            return BuildFromDtos(dtos);
        }

        private IEnumerable<MemberTypeReadOnlyDto> MapOneToManies(IEnumerable<MemberTypeReadOnlyDto> dtos)
        {
            MemberTypeReadOnlyDto acc = null;
            foreach (var dto in dtos)
            {
                if (acc == null)
                {
                    acc = dto;
                }
                else if (acc.UniqueId == dto.UniqueId)
                {
                    var prop = dto.PropertyTypes.SingleOrDefault();
                    var group = dto.PropertyTypeGroups.SingleOrDefault();

                    if (prop != null && prop.Id.HasValue && acc.PropertyTypes.Any(x => x.Id == prop.Id.Value) == false)
                        acc.PropertyTypes.Add(prop);

                    if (group != null && group.Id.HasValue && acc.PropertyTypeGroups.Any(x => x.Id == group.Id.Value) == false)
                        acc.PropertyTypeGroups.Add(group);
                }
                else
                {
                    yield return acc;
                    acc = dto;
                }
            }

            if (acc != null)
                yield return acc;
        }

        protected override Sql<SqlContext> GetBaseQuery(bool isCount)
        {
            if (isCount)
            {
                return Sql()
                    .SelectCount()
                    .From<NodeDto>()
                    .InnerJoin<ContentTypeDto>().On<ContentTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                    .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
            }

            var sql = Sql()
                .Select("umbracoNode.*", "cmsContentType.*", "cmsPropertyType.id AS PropertyTypeId", "cmsPropertyType.Alias",
                    "cmsPropertyType.Name", "cmsPropertyType.Description", "cmsPropertyType.mandatory",
                    "cmsPropertyType.validationRegExp", "cmsPropertyType.dataTypeId", "cmsPropertyType.sortOrder AS PropertyTypeSortOrder",
                    "cmsPropertyType.propertyTypeGroupId AS PropertyTypesGroupId", "cmsMemberType.memberCanEdit", "cmsMemberType.viewOnProfile",
                    "cmsDataType.propertyEditorAlias", "cmsDataType.dbType", "cmsPropertyTypeGroup.id AS PropertyTypeGroupId",
                    "cmsPropertyTypeGroup.text AS PropertyGroupName",
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

        protected Sql<SqlContext> GetSubquery()
        {
            var sql = Sql()
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
            var l = (List<string>)base.GetDeleteClauses(); // we know it's a list
            l.Add("DELETE FROM cmsMemberType WHERE NodeId = @Id");
            l.Add("DELETE FROM cmsContentType WHERE nodeId = @Id");
            l.Add("DELETE FROM umbracoNode WHERE id = @Id");
            return l;
        }

        protected override Guid NodeObjectTypeId => Constants.ObjectTypes.MemberTypeGuid;

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

            var factory = new ContentTypeFactory();

            EnsureExplicitDataTypeForBuiltInProperties(entity);
            PersistNewBaseContentType(entity);

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

            var factory = new ContentTypeFactory();

            EnsureExplicitDataTypeForBuiltInProperties(entity);
            PersistUpdatedBaseContentType(entity);

            // remove and insert - handle cmsMemberType table
            Database.Delete<MemberTypeDto>("WHERE NodeId = @Id", new { Id = entity.Id });
            var memberTypeDtos = factory.BuildMemberTypeDtos(entity);
            foreach (var memberTypeDto in memberTypeDtos)
            {
                Database.Insert(memberTypeDto);
            }

            entity.ResetDirtyProperties();
        }

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