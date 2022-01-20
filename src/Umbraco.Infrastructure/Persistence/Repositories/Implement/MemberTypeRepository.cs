using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="IMemberType"/>
    /// </summary>
    internal class MemberTypeRepository : ContentTypeRepositoryBase<IMemberType>, IMemberTypeRepository
    {
        private readonly IShortStringHelper _shortStringHelper;

        public MemberTypeRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger<MemberTypeRepository> logger, IContentTypeCommonRepository commonRepository, ILanguageRepository languageRepository, IShortStringHelper shortStringHelper)
            : base(scopeAccessor, cache, logger, commonRepository, languageRepository, shortStringHelper)
        {
            _shortStringHelper = shortStringHelper;
        }

        protected override bool SupportsPublishing => MemberType.SupportsPublishingConst;

        protected override IRepositoryCachePolicy<IMemberType, int> CreateCachePolicy()
        {
            return new FullDataSetRepositoryCachePolicy<IMemberType, int>(GlobalIsolatedCache, ScopeAccessor, GetEntityId, /*expires:*/ true);
        }

        // every GetExists method goes cachePolicy.GetSomething which in turns goes PerformGetAll,
        // since this is a FullDataSet policy - and everything is cached
        // so here,
        // every PerformGet/Exists just GetMany() and then filters
        // except PerformGetAll which is the one really doing the job

        protected override IMemberType PerformGet(int id)
            => GetMany().FirstOrDefault(x => x.Id == id);

        protected override IMemberType PerformGet(Guid id)
            => GetMany().FirstOrDefault(x => x.Key == id);

        protected override IEnumerable<IMemberType> PerformGetAll(params Guid[] ids)
        {
            var all = GetMany();
            return ids.Any() ? all.Where(x => ids.Contains(x.Key)) : all;
        }

        protected override bool PerformExists(Guid id)
            => GetMany().FirstOrDefault(x => x.Key == id) != null;

        protected override IMemberType PerformGet(string alias)
            => GetMany().FirstOrDefault(x => x.Alias.InvariantEquals(alias));

        protected override IEnumerable<IMemberType> GetAllWithFullCachePolicy()
        {
            return CommonRepository.GetAllTypes().OfType<IMemberType>();
        }

        protected override IEnumerable<IMemberType> PerformGetByQuery(IQuery<IMemberType> query)
        {
            var subQuery = GetSubquery();
            var translator = new SqlTranslator<IMemberType>(subQuery, query);
            var subSql = translator.Translate();
            var sql = GetBaseQuery(false)
                .WhereIn<NodeDto>(x => x.NodeId, subSql)
                .OrderBy<NodeDto>(x => x.SortOrder);
            var ids = Database.Fetch<int>(sql).Distinct().ToArray();

            return ids.Length > 0 ? GetMany(ids).OrderBy(x => x.Name) : Enumerable.Empty<IMemberType>();
        }

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
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
                .Select<NodeDto>(x => x.NodeId)
                .From<NodeDto>()
                .InnerJoin<ContentTypeDto>().On<ContentTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .LeftJoin<PropertyTypeDto>().On<PropertyTypeDto, NodeDto>(left => left.ContentTypeId, right => right.NodeId)
                .LeftJoin<MemberPropertyTypeDto>().On<MemberPropertyTypeDto, PropertyTypeDto>(left => left.PropertyTypeId, right => right.Id)
                .LeftJoin<DataTypeDto>().On<DataTypeDto, PropertyTypeDto>(left => left.NodeId, right => right.DataTypeId)
                .LeftJoin<PropertyTypeGroupDto>().On<PropertyTypeGroupDto, NodeDto>(left => left.ContentTypeNodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);

            return sql;
        }

        protected Sql<ISqlContext> GetSubquery()
        {
            var sql = Sql()
                .Select("DISTINCT(umbracoNode.id)")
                .From<NodeDto>()
                .InnerJoin<ContentTypeDto>().On<ContentTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .LeftJoin<PropertyTypeDto>().On<PropertyTypeDto, NodeDto>(left => left.ContentTypeId, right => right.NodeId)
                .LeftJoin<MemberPropertyTypeDto>().On<MemberPropertyTypeDto, PropertyTypeDto>(left => left.PropertyTypeId, right => right.Id)
                .LeftJoin<DataTypeDto>().On<DataTypeDto, PropertyTypeDto>(left => left.NodeId, right => right.DataTypeId)
                .LeftJoin<PropertyTypeGroupDto>().On<PropertyTypeGroupDto, NodeDto>(left => left.ContentTypeNodeId, right => right.NodeId)
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
            l.Add("DELETE FROM cmsMemberType WHERE NodeId = @id");
            l.Add("DELETE FROM cmsContentType WHERE nodeId = @id");
            l.Add("DELETE FROM umbracoNode WHERE id = @id");
            return l;
        }

        protected override Guid NodeObjectTypeId => Cms.Core.Constants.ObjectTypes.MemberType;

        protected override void PersistNewItem(IMemberType entity)
        {
            ValidateAlias(entity);

            entity.AddingEntity();

            //set a default icon if one is not specified
            if (entity.Icon.IsNullOrWhiteSpace())
            {
                entity.Icon = Cms.Core.Constants.Icons.Member;
            }

            //By Convention we add 9 standard PropertyTypes to an Umbraco MemberType
            var standardPropertyTypes = ConventionsHelper.GetStandardPropertyTypeStubs(_shortStringHelper);
            foreach (var standardPropertyType in standardPropertyTypes)
            {
                entity.AddPropertyType(standardPropertyType.Value, Cms.Core.Constants.Conventions.Member.StandardPropertiesGroupAlias, Cms.Core.Constants.Conventions.Member.StandardPropertiesGroupName);
            }

            EnsureExplicitDataTypeForBuiltInProperties(entity);
            PersistNewBaseContentType(entity);

            //Handles the MemberTypeDto (cmsMemberType table)
            var memberTypeDtos = ContentTypeFactory.BuildMemberPropertyTypeDtos(entity);
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

            EnsureExplicitDataTypeForBuiltInProperties(entity);
            PersistUpdatedBaseContentType(entity);

            // remove and insert - handle cmsMemberType table
            Database.Delete<MemberPropertyTypeDto>("WHERE NodeId = @Id", new { Id = entity.Id });
            var memberTypeDtos = ContentTypeFactory.BuildMemberPropertyTypeDtos(entity);
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
        /// <param name="storageType"></param>
        /// <param name="propertyTypeAlias"></param>
        /// <returns></returns>
        protected override PropertyType CreatePropertyType(string propertyEditorAlias, ValueStorageType storageType, string propertyTypeAlias)
        {
            //custom property type constructor logic to set explicit dbtype's for built in properties
            var builtinProperties = ConventionsHelper.GetStandardPropertyTypeStubs(_shortStringHelper);
            var readonlyStorageType = builtinProperties.TryGetValue(propertyTypeAlias, out var propertyType);
            storageType = readonlyStorageType ? propertyType.ValueStorageType : storageType;
            return new PropertyType(_shortStringHelper, propertyEditorAlias, storageType, readonlyStorageType, propertyTypeAlias);
        }

        /// <summary>
        /// Ensure that all the built-in membership provider properties have their correct data type
        /// and property editors assigned. This occurs prior to saving so that the correct values are persisted.
        /// </summary>
        /// <param name="memberType"></param>
        private void EnsureExplicitDataTypeForBuiltInProperties(IContentTypeBase memberType)
        {
            var builtinProperties = ConventionsHelper.GetStandardPropertyTypeStubs(_shortStringHelper);
            foreach (var propertyType in memberType.PropertyTypes)
            {
                if (builtinProperties.ContainsKey(propertyType.Alias))
                {
                    //this reset's its current data type reference which will be re-assigned based on the property editor assigned on the next line
                    if (builtinProperties.TryGetValue(propertyType.Alias, out var propDefinition) && propDefinition != null)
                    {
                        propertyType.DataTypeId = propDefinition.DataTypeId;
                        propertyType.DataTypeKey = propDefinition.DataTypeKey;
                    }
                    else
                    {
                        propertyType.DataTypeId = 0;
                        propertyType.DataTypeKey = default;
                    }
                }
            }
        }
    }
}
