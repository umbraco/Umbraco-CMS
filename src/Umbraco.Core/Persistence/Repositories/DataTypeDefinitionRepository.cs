using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;

using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="DataTypeDefinition"/>
    /// </summary>
    internal class DataTypeDefinitionRepository : PetaPocoRepositoryBase<int, IDataTypeDefinition>, IDataTypeDefinitionRepository
    {
        private readonly CacheHelper _cacheHelper;
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly DataTypePreValueRepository _preValRepository;

        public DataTypeDefinitionRepository(IDatabaseUnitOfWork work, CacheHelper cache, CacheHelper cacheHelper, ILogger logger, ISqlSyntaxProvider sqlSyntax,
            IContentTypeRepository contentTypeRepository)
            : base(work, cache, logger, sqlSyntax)
        {
            _cacheHelper = cacheHelper;
            _contentTypeRepository = contentTypeRepository;
            _preValRepository = new DataTypePreValueRepository(work, CacheHelper.CreateDisabledCacheHelper(), logger, sqlSyntax);
        }

        #region Overrides of RepositoryBase<int,DataTypeDefinition>

        protected override IDataTypeDefinition PerformGet(int id)
        {
            return GetAll(new[] { id }).FirstOrDefault();
        }

        protected override IEnumerable<IDataTypeDefinition> PerformGetAll(params int[] ids)
        {
            var factory = new DataTypeDefinitionFactory(NodeObjectTypeId);
            var dataTypeSql = GetBaseQuery(false);

            if (ids.Any())
            {
                dataTypeSql.Where("umbracoNode.id in (@ids)", new { ids = ids });
            }
            else
            {
                dataTypeSql.Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
            }

            var dtos = Database.Fetch<DataTypeDto, NodeDto>(dataTypeSql);
            return dtos.Select(factory.BuildEntity).ToArray();
        }

        protected override IEnumerable<IDataTypeDefinition> PerformGetByQuery(IQuery<IDataTypeDefinition> query)
        {
            var factory = new DataTypeDefinitionFactory(NodeObjectTypeId);

            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IDataTypeDefinition>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<DataTypeDto, NodeDto>(sql);

            return dtos.Select(factory.BuildEntity).ToArray();
        }

        /// <summary>
        /// Override the delete method so that we can ensure that all related content type's are updated as part of the overall transaction
        /// </summary>
        /// <param name="entity"></param>
        public override void Delete(IDataTypeDefinition entity)
        {
            //Find ContentTypes using this IDataTypeDefinition on a PropertyType
            var query = Query<PropertyType>.Builder.Where(x => x.DataTypeDefinitionId == entity.Id);

            //TODO: Don't we need to be concerned about media and member types here too ?
            var contentTypes = _contentTypeRepository.GetByQuery(query);

            //Loop through the list of results and remove the PropertyTypes that references the DataTypeDefinition that is being deleted
            foreach (var contentType in contentTypes)
            {
                if (contentType == null) continue;

                foreach (var group in contentType.PropertyGroups)
                {
                    var types = @group.PropertyTypes.Where(x => x.DataTypeDefinitionId == entity.Id).ToList();
                    foreach (var propertyType in types)
                    {
                        @group.PropertyTypes.Remove(propertyType);
                    }
                }

                _contentTypeRepository.AddOrUpdate(contentType);
            }

            //call the base method to queue the deletion of this data type
            base.Delete(entity);
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<int,DataTypeDefinition>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*")
               .From<DataTypeDto>()
               .InnerJoin<NodeDto>()
               .On<DataTypeDto, NodeDto>(left => left.DataTypeId, right => right.NodeId)
               .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "umbracoNode.id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            return new List<string>();
        }

        protected override Guid NodeObjectTypeId
        {
            get { return new Guid(Constants.ObjectTypes.DataType); }
        }

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(IDataTypeDefinition entity)
        {
            ((DataTypeDefinition)entity).AddingEntity();

            //Cannot add a duplicate data type
            var exists = Database.ExecuteScalar<int>(@"SELECT COUNT(*) FROM cmsDataType
INNER JOIN umbracoNode ON cmsDataType.nodeId = umbracoNode.id
WHERE umbracoNode." + SqlSyntax.GetQuotedColumnName("text") + "= @name", new { name = entity.Name });
            if (exists > 0)
            {
                throw new DuplicateNameException("A data type with the name " + entity.Name + " already exists");
            }

            var factory = new DataTypeDefinitionFactory(NodeObjectTypeId);
            var dto = factory.BuildDto(entity);

            //Logic for setting Path, Level and SortOrder
            var parent = Database.First<NodeDto>("WHERE id = @ParentId", new { ParentId = entity.ParentId });
            int level = parent.Level + 1;
            int sortOrder =
                Database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoNode WHERE parentID = @ParentId AND nodeObjectType = @NodeObjectType",
                                                      new { ParentId = entity.ParentId, NodeObjectType = NodeObjectTypeId });

            //Create the (base) node data - umbracoNode
            var nodeDto = dto.NodeDto;
            nodeDto.Path = parent.Path;
            nodeDto.Level = short.Parse(level.ToString(CultureInfo.InvariantCulture));
            nodeDto.SortOrder = sortOrder;
            var o = Database.IsNew(nodeDto) ? Convert.ToInt32(Database.Insert(nodeDto)) : Database.Update(nodeDto);

            //Update with new correct path
            nodeDto.Path = string.Concat(parent.Path, ",", nodeDto.NodeId);
            Database.Update(nodeDto);

            //Update entity with correct values
            entity.Id = nodeDto.NodeId; //Set Id on entity to ensure an Id is set
            entity.Path = nodeDto.Path;
            entity.SortOrder = sortOrder;
            entity.Level = level;

            dto.DataTypeId = nodeDto.NodeId;
            Database.Insert(dto);

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IDataTypeDefinition entity)
        {

            //Cannot change to a duplicate alias
            var exists = Database.ExecuteScalar<int>(@"SELECT COUNT(*) FROM cmsDataType
INNER JOIN umbracoNode ON cmsDataType.nodeId = umbracoNode.id
WHERE umbracoNode." + SqlSyntax.GetQuotedColumnName("text") + @"= @name
AND umbracoNode.id <> @id",
                    new { id = entity.Id, name = entity.Name });
            if (exists > 0)
            {
                throw new DuplicateNameException("A data type with the name " + entity.Name + " already exists");
            }

            //Updates Modified date and Version Guid
            ((DataTypeDefinition)entity).UpdatingEntity();

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

            var factory = new DataTypeDefinitionFactory(NodeObjectTypeId);
            //Look up DataTypeDefinition entry to get Primary for updating the DTO
            var dataTypeDto = Database.SingleOrDefault<DataTypeDto>("WHERE nodeId = @Id", new { Id = entity.Id });
            factory.SetPrimaryKey(dataTypeDto.PrimaryKey);
            var dto = factory.BuildDto(entity);

            //Updates the (base) node data - umbracoNode
            var nodeDto = dto.NodeDto;
            Database.Update(nodeDto);
            Database.Update(dto);

            //NOTE: This is a special case, we need to clear the custom cache for pre-values here so they are not stale if devs
            // are querying for them in the Saved event (before the distributed call cache is clearing it)
            _cacheHelper.RuntimeCache.ClearCacheItem(GetPrefixedCacheKey(entity.Id));

            entity.ResetDirtyProperties();
        }

        protected override void PersistDeletedItem(IDataTypeDefinition entity)
        {
            //Remove Notifications
            Database.Delete<User2NodeNotifyDto>("WHERE nodeId = @Id", new { Id = entity.Id });

            //Remove Permissions
            Database.Delete<User2NodePermissionDto>("WHERE nodeId = @Id", new { Id = entity.Id });

            //Remove associated tags
            Database.Delete<TagRelationshipDto>("WHERE nodeId = @Id", new { Id = entity.Id });

            //PropertyTypes containing the DataType being deleted
            var propertyTypeDtos = Database.Fetch<PropertyTypeDto>("WHERE dataTypeId = @Id", new { Id = entity.Id });
            //Go through the PropertyTypes and delete referenced PropertyData before deleting the PropertyType
            foreach (var dto in propertyTypeDtos)
            {
                Database.Delete<PropertyDataDto>("WHERE propertytypeid = @Id", new { Id = dto.Id });
                Database.Delete<PropertyTypeDto>("WHERE id = @Id", new { Id = dto.Id });
            }

            //Delete the pre-values
            Database.Delete<DataTypePreValueDto>("WHERE datatypeNodeId = @Id", new { Id = entity.Id });

            //Delete Content specific data
            Database.Delete<DataTypeDto>("WHERE nodeId = @Id", new { Id = entity.Id });

            //Delete (base) node data
            Database.Delete<NodeDto>("WHERE uniqueID = @Id", new { Id = entity.Key });
        }

        #endregion

        public PreValueCollection GetPreValuesCollectionByDataTypeId(int dataTypeId)
        {
            var cached = _cacheHelper.RuntimeCache.GetCacheItemsByKeySearch<PreValueCollection>(GetPrefixedCacheKey(dataTypeId));
            if (cached != null && cached.Any())
            {
                //return from the cache, ensure it's a cloned result
                return (PreValueCollection)cached.First().DeepClone();
            }

            return GetAndCachePreValueCollection(dataTypeId);
        }

        public string GetPreValueAsString(int preValueId)
        {
            //We need to see if we can find the cached PreValueCollection based on the cache key above

            var regex = CacheKeys.DataTypePreValuesCacheKey + @"[\d]+-[,\d]*" + preValueId + @"[,\d$]*";

            var cached = _cacheHelper.RuntimeCache.GetCacheItemsByKeyExpression<PreValueCollection>(regex);
            if (cached != null && cached.Any())
            {
                //return from the cache
                var collection = cached.First();
                var preVal = collection.FormatAsDictionary().Single(x => x.Value.Id == preValueId);
                return preVal.Value.Value;
            }

            //go and find the data type id for the pre val id passed in

            var dto = Database.FirstOrDefault<DataTypePreValueDto>("WHERE id = @preValueId", new { preValueId = preValueId });
            if (dto == null)
            {
                return string.Empty;
            }
            // go cache the collection
            var preVals = GetAndCachePreValueCollection(dto.DataTypeNodeId);

            //return the single value for this id
            var pv = preVals.FormatAsDictionary().Single(x => x.Value.Id == preValueId);
            return pv.Value.Value;
        }

        public void AddOrUpdatePreValues(int dataTypeId, IDictionary<string, PreValue> values)
        {
            var dtd = Get(dataTypeId);
            if (dtd == null)
            {
                throw new InvalidOperationException("No data type found with id " + dataTypeId);
            }
            AddOrUpdatePreValues(dtd, values);
        }

        public void AddOrUpdatePreValues(IDataTypeDefinition dataType, IDictionary<string, PreValue> values)
        {
            var currentVals = new DataTypePreValueDto[] { };
            if (dataType.HasIdentity)
            {
                //first just get all pre-values for this data type so we can compare them to see if we need to insert or update or replace
                var sql = new Sql().Select("*")
                                   .From<DataTypePreValueDto>()
                                   .Where<DataTypePreValueDto>(dto => dto.DataTypeNodeId == dataType.Id)
                                   .OrderBy<DataTypePreValueDto>(dto => dto.SortOrder);
                currentVals = Database.Fetch<DataTypePreValueDto>(sql).ToArray();
            }

            //already existing, need to be updated
            var valueIds = values.Where(x => x.Value.Id > 0).Select(x => x.Value.Id).ToArray();
            var existingByIds = currentVals.Where(x => valueIds.Contains(x.Id)).ToArray();

            //These ones need to be removed from the db, they no longer exist in the new values
            var deleteById = currentVals.Where(x => valueIds.Contains(x.Id) == false);

            foreach (var d in deleteById)
            {
                _preValRepository.Delete(new PreValueEntity
                {
                    Alias = d.Alias,
                    Id = d.Id,
                    Value = d.Value,
                    DataType = dataType,
                    SortOrder = d.SortOrder
                });
            }

            var sortOrder = 1;

            foreach (var pre in values)
            {
                var existing = existingByIds.FirstOrDefault(valueDto => valueDto.Id == pre.Value.Id);
                if (existing != null)
                {
                    _preValRepository.AddOrUpdate(new PreValueEntity
                    {
                        //setting an id will update it
                        Id = existing.Id,
                        DataType = dataType,
                        //These are the new values to update
                        Alias = pre.Key,
                        SortOrder = sortOrder,
                        Value = pre.Value.Value
                    });
                }
                else
                {
                    _preValRepository.AddOrUpdate(new PreValueEntity
                    {
                        Alias = pre.Key,
                        SortOrder = sortOrder,
                        Value = pre.Value.Value,
                        DataType = dataType,
                    });
                }

                sortOrder++;
            }

        }

        private string GetPrefixedCacheKey(int dataTypeId)
        {
            return CacheKeys.DataTypePreValuesCacheKey + dataTypeId + "-";
        }

        private PreValueCollection GetAndCachePreValueCollection(int dataTypeId)
        {
            //go get the data
            var dtos = Database.Fetch<DataTypePreValueDto>("WHERE datatypeNodeId = @Id", new { Id = dataTypeId });
            var list = dtos.Select(x => new Tuple<PreValue, string, int>(new PreValue(x.Id, x.Value, x.SortOrder), x.Alias, x.SortOrder)).ToList();
            var collection = PreValueConverter.ConvertToPreValuesCollection(list);

            //now create the cache key, this needs to include all pre-value ids so that we can use this cached item in the GetPreValuesAsString method
            //the key will be: "UmbracoPreValDATATYPEID-CSVOFPREVALIDS

            var key = GetPrefixedCacheKey(dataTypeId)
                      + string.Join(",", collection.FormatAsDictionary().Select(x => x.Value.Id).ToArray());

            //store into cache
            _cacheHelper.RuntimeCache.InsertCacheItem(key, () => collection,
                //30 mins
                new TimeSpan(0, 0, 30),
                //sliding is true
                true);

            return collection;
        }

        /// <summary>
        /// Private class to handle pre-value crud based on units of work with transactions
        /// </summary>
        private class PreValueEntity : Entity, IAggregateRoot
        {
            public string Value { get; set; }
            public string Alias { get; set; }
            public IDataTypeDefinition DataType { get; set; }
            public int SortOrder { get; set; }
        }

        /// <summary>
        /// Private class to handle pre-value crud based on standard principles and units of work with transactions
        /// </summary>
        private class DataTypePreValueRepository : PetaPocoRepositoryBase<int, PreValueEntity>
        {
            public DataTypePreValueRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
                : base(work, cache, logger, sqlSyntax)
            {
            }

            #region Not implemented (don't need to for the purposes of this repo)
            protected override PreValueEntity PerformGet(int id)
            {
                throw new NotImplementedException();
            }

            protected override IEnumerable<PreValueEntity> PerformGetAll(params int[] ids)
            {
                throw new NotImplementedException();
            }

            protected override IEnumerable<PreValueEntity> PerformGetByQuery(IQuery<PreValueEntity> query)
            {
                throw new NotImplementedException();
            }

            protected override Sql GetBaseQuery(bool isCount)
            {
                throw new NotImplementedException();
            }

            protected override string GetBaseWhereClause()
            {
                throw new NotImplementedException();
            }

            protected override IEnumerable<string> GetDeleteClauses()
            {
                return new List<string>();
            }

            protected override Guid NodeObjectTypeId
            {
                get { throw new NotImplementedException(); }
            }
            #endregion

            protected override void PersistDeletedItem(PreValueEntity entity)
            {
                Database.Execute(
                    "DELETE FROM cmsDataTypePreValues WHERE id=@Id",
                    new { Id = entity.Id });
            }

            protected override void PersistNewItem(PreValueEntity entity)
            {
                if (entity.DataType.HasIdentity == false)
                {
                    throw new InvalidOperationException("Cannot insert a pre value for a data type that has no identity");
                }

                //Cannot add a duplicate alias
                var exists = Database.ExecuteScalar<int>(@"SELECT COUNT(*) FROM cmsDataTypePreValues
WHERE alias = @alias
AND datatypeNodeId = @dtdid",
                        new { alias = entity.Alias, dtdid = entity.DataType.Id });
                if (exists > 0)
                {
                    throw new DuplicateNameException("A pre value with the alias " + entity.Alias + " already exists for this data type");
                }

                var dto = new DataTypePreValueDto
                {
                    DataTypeNodeId = entity.DataType.Id,
                    Value = entity.Value,
                    SortOrder = entity.SortOrder,
                    Alias = entity.Alias
                };
                Database.Insert(dto);
            }

            protected override void PersistUpdatedItem(PreValueEntity entity)
            {
                if (entity.DataType.HasIdentity == false)
                {
                    throw new InvalidOperationException("Cannot update a pre value for a data type that has no identity");
                }

                //Cannot change to a duplicate alias
                var exists = Database.ExecuteScalar<int>(@"SELECT COUNT(*) FROM cmsDataTypePreValues
WHERE alias = @alias
AND datatypeNodeId = @dtdid
AND id <> @id",
                        new { id = entity.Id, alias = entity.Alias, dtdid = entity.DataType.Id });
                if (exists > 0)
                {
                    throw new DuplicateNameException("A pre value with the alias " + entity.Alias + " already exists for this data type");
                }

                var dto = new DataTypePreValueDto
                {
                    DataTypeNodeId = entity.DataType.Id,
                    Id = entity.Id,
                    Value = entity.Value,
                    SortOrder = entity.SortOrder,
                    Alias = entity.Alias
                };
                Database.Update(dto);
            }
        }

        internal static class PreValueConverter
        {
            /// <summary>
            /// Converts the tuple to a pre-value collection
            /// </summary>
            /// <param name="list"></param>
            /// <returns></returns>
            internal static PreValueCollection ConvertToPreValuesCollection(IEnumerable<Tuple<PreValue, string, int>> list)
            {
                //now we need to determine if they are dictionary based, otherwise they have to be array based
                var dictionary = new Dictionary<string, PreValue>();

                //need to check all of the keys, if there's only one and it is empty then it's an array
                var keys = list.Select(x => x.Item2).Distinct().ToArray();
                if (keys.Length == 1 && keys[0].IsNullOrWhiteSpace())
                {
                    return new PreValueCollection(list.OrderBy(x => x.Item3).Select(x => x.Item1));
                }

                foreach (var item in list
                    .OrderBy(x => x.Item3) //we'll order them first so we maintain the order index in the dictionary
                    .GroupBy(x => x.Item2)) //group by alias
                {
                    if (item.Count() > 1)
                    {
                        //if there's more than 1 item per key, then it cannot be a dictionary, just return the array
                        return new PreValueCollection(list.OrderBy(x => x.Item3).Select(x => x.Item1));
                    }

                    dictionary.Add(item.Key, item.First().Item1);
                }

                return new PreValueCollection(dictionary);
            }
        }

        /// <summary>
        /// Dispose disposable properties
        /// </summary>
        /// <remarks>
        /// Ensure the unit of work is disposed
        /// </remarks>
        protected override void DisposeResources()
        {
            _contentTypeRepository.Dispose();
            _preValRepository.Dispose();
        }
    }


}