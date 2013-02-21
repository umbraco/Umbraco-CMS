using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Relators;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represent an abstract Repository for ContentType based repositories
    /// </summary>
    /// <remarks>Exposes shared functionality</remarks>
    /// <typeparam name="TId"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    internal abstract class ContentTypeBaseRepository<TId, TEntity> : PetaPocoRepositoryBase<TId, TEntity>
        where TEntity : IContentTypeComposition
    {
		protected ContentTypeBaseRepository(IDatabaseUnitOfWork work)
			: base(work)
        {
        }

		protected ContentTypeBaseRepository(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache)
			: base(work, cache)
        {
        }

        protected IEnumerable<int> PerformGetByQuery(IQuery<PropertyType> query)
        {
            var sqlClause = new Sql();
            sqlClause.Select("*")
               .From<PropertyTypeGroupDto>()
               .RightJoin<PropertyTypeDto>()
               .On<PropertyTypeGroupDto, PropertyTypeDto>(left => left.Id, right => right.PropertyTypeGroupId)
               .InnerJoin<DataTypeDto>()
               .On<PropertyTypeDto, DataTypeDto>(left => left.DataTypeId, right => right.DataTypeId)
               .OrderBy<PropertyTypeDto>(x => x.PropertyTypeGroupId);

            var translator = new SqlTranslator<PropertyType>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<PropertyTypeGroupDto, PropertyTypeDto, DataTypeDto, PropertyTypeGroupDto>(new GroupPropertyTypeRelator().Map, sql);

            foreach (var dto in dtos.DistinctBy(x => x.ContentTypeNodeId))
            {
                yield return dto.ContentTypeNodeId;
            }
        }

        protected void PersistNewBaseContentType(ContentTypeDto dto, IContentTypeComposition entity)
        {
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

            //Insert new ContentType entry
            dto.NodeId = nodeDto.NodeId;
            Database.Insert(dto);

            //Insert ContentType composition in new table
            foreach (var composition in entity.ContentTypeComposition)
            {
                if (composition.Id == entity.Id) continue;//Just to ensure that we aren't creating a reference to ourself.

                if (composition.HasIdentity)
                {
                    Database.Insert(new ContentType2ContentTypeDto { ParentId = composition.Id, ChildId = entity.Id });
                }
                else
                {
                    //Fallback for ContentTypes with no identity
                    var contentTypeDto = Database.FirstOrDefault<ContentTypeDto>("WHERE alias = @Alias", new {Alias = composition.Alias});
                    if (contentTypeDto != null)
                    {
                        Database.Insert(new ContentType2ContentTypeDto { ParentId = contentTypeDto.NodeId, ChildId = entity.Id });
                    }
                }
            }

            //Insert collection of allowed content types
            foreach (var allowedContentType in entity.AllowedContentTypes)
            {
                Database.Insert(new ContentTypeAllowedContentTypeDto { Id = entity.Id, AllowedId = allowedContentType.Id.Value, SortOrder = allowedContentType.SortOrder });
            }

            var propertyFactory = new PropertyGroupFactory(nodeDto.NodeId);

            //Insert Tabs
            foreach (var propertyGroup in entity.PropertyGroups)
            {
                var tabDto = propertyFactory.BuildGroupDto(propertyGroup);
                var primaryKey = Convert.ToInt32(Database.Insert(tabDto));
                propertyGroup.Id = primaryKey;//Set Id on PropertyGroup
            }

            //Insert PropertyTypes
            foreach (var propertyGroup in entity.PropertyGroups)
            {
                foreach (var propertyType in propertyGroup.PropertyTypes)
                {
                    var propertyTypeDto = propertyFactory.BuildPropertyTypeDto(propertyGroup.Id, propertyType);
                    var primaryKey = Convert.ToInt32(Database.Insert(propertyTypeDto));
                    propertyType.Id = primaryKey;//Set Id on PropertyType

                    //Update the current PropertyType with correct ControlId and DatabaseType
                    var dataTypeDto = Database.FirstOrDefault<DataTypeDto>("WHERE nodeId = @Id", new { Id = propertyTypeDto.DataTypeId });
                    propertyType.DataTypeId = dataTypeDto.ControlId;
                    propertyType.DataTypeDatabaseType = dataTypeDto.DbType.EnumParse<DataTypeDatabaseType>(true);
                }
            }
        }

        protected void PersistUpdatedBaseContentType(ContentTypeDto dto, IContentTypeComposition entity)
        {
            var propertyFactory = new PropertyGroupFactory(entity.Id);

            var nodeDto = dto.NodeDto;
            var o = Database.Update(nodeDto);

            //Look up ContentType entry to get PrimaryKey for updating the DTO
            var dtoPk = Database.First<ContentTypeDto>("WHERE nodeId = @Id", new { Id = entity.Id });
            dto.PrimaryKey = dtoPk.PrimaryKey;
            Database.Update(dto);

            //Delete the ContentType composition entries before adding the updated collection
            Database.Delete<ContentType2ContentTypeDto>("WHERE childContentTypeId = @Id", new { Id = entity.Id });
            //Update ContentType composition in new table
            foreach (var composition in entity.ContentTypeComposition)
            {
                Database.Insert(new ContentType2ContentTypeDto { ParentId = composition.Id, ChildId = entity.Id });
            }

            //TODO U4-1690 - test what happens with property types and groups from content type compositions when a content type is removed
            //1. Find content based on the current content type: entity.Id
            //2. Find all property types (and groups?) on the content type that was removed - tracked id
            //3. Remove properties based on property types from the removed content type where the content ids correspond to those found in step one

            //Delete the allowed content type entries before adding the updated collection
            Database.Delete<ContentTypeAllowedContentTypeDto>("WHERE Id = @Id", new { Id = entity.Id });
            //Insert collection of allowed content types
            foreach (var allowedContentType in entity.AllowedContentTypes)
            {
                Database.Insert(new ContentTypeAllowedContentTypeDto { Id = entity.Id, AllowedId = allowedContentType.Id.Value, SortOrder = allowedContentType.SortOrder });
            }

            //Check Dirty properties for Tabs/Groups and PropertyTypes - insert and delete accordingly
            if (((ICanBeDirty)entity).IsPropertyDirty("PropertyGroups") || entity.PropertyGroups.Any(x => x.IsDirty()))
            {
                //Delete PropertyTypes by excepting entries from db with entries from collections
                var dbPropertyTypes = Database.Fetch<PropertyTypeDto>("WHERE contentTypeId = @Id", new { Id = entity.Id });
                var dbPropertyTypeAlias = dbPropertyTypes.Select(x => x.Alias.ToLowerInvariant());
                var entityPropertyTypes = entity.PropertyTypes.Select(x => x.Alias.ToLowerInvariant());
                var aliases = dbPropertyTypeAlias.Except(entityPropertyTypes);
                foreach (var alias in aliases)
                {
                    //Before a PropertyType can be deleted, all Properties based on that PropertyType should be deleted.
                    var propertyType = dbPropertyTypes.FirstOrDefault(x => x.Alias.ToLowerInvariant() == alias);
                    if (propertyType != null)
                    {
                        Database.Delete<PropertyDataDto>("WHERE propertytypeid = @Id", new { Id = propertyType.Id });
                    }

                    Database.Delete<PropertyTypeDto>("WHERE contentTypeId = @Id AND Alias = @Alias", new { Id = entity.Id, Alias = alias });
                }
                //Delete Tabs/Groups by excepting entries from db with entries from collections
                var dbPropertyGroups = Database.Fetch<PropertyTypeGroupDto>("WHERE contenttypeNodeId = @Id", new { Id = entity.Id }).Select(x => x.Text);
                var entityPropertyGroups = entity.PropertyGroups.Select(x => x.Name);
                var tabs = dbPropertyGroups.Except(entityPropertyGroups);
                foreach (var tabName in tabs)
                {
                    Database.Delete<PropertyTypeGroupDto>("WHERE contenttypeNodeId = @Id AND text = @Name", new { Id = entity.Id, Name = tabName });
                }

                //Run through all groups to insert or update entries
                foreach (var propertyGroup in entity.PropertyGroups)
                {
                    var tabDto = propertyFactory.BuildGroupDto(propertyGroup);
                    int groupPrimaryKey = propertyGroup.HasIdentity
                                              ? Database.Update(tabDto)
                                              : Convert.ToInt32(Database.Insert(tabDto));
                    if (propertyGroup.HasIdentity == false)
                        propertyGroup.Id = groupPrimaryKey;//Set Id on new PropertyGroup

                    //Ensure that the PropertyGroup's Id is set on the PropertyTypes within a group
                    foreach (var propertyType in propertyGroup.PropertyTypes)
                    {
                        propertyType.PropertyGroupId = propertyGroup.Id;
                    }
                }

                //Run through all PropertyTypes to insert or update entries
                foreach (var propertyType in entity.PropertyTypes)
                {
                    var propertyTypeDto = propertyFactory.BuildPropertyTypeDto(propertyType.PropertyGroupId, propertyType);
                    int typePrimaryKey = propertyType.HasIdentity
                                             ? Database.Update(propertyTypeDto)
                                             : Convert.ToInt32(Database.Insert(propertyTypeDto));
                    if (propertyType.HasIdentity == false)
                        propertyType.Id = typePrimaryKey;//Set Id on new PropertyType
                }
            }
        }

        protected IEnumerable<ContentTypeSort> GetAllowedContentTypeIds(int id)
        {
            var sql = new Sql();
            sql.Select("*")
               .From<ContentTypeAllowedContentTypeDto>()
               .Where<ContentTypeAllowedContentTypeDto>(x => x.Id == id);

            var allowedContentTypeDtos = Database.Fetch<ContentTypeAllowedContentTypeDto>(sql);
            return allowedContentTypeDtos.Select(x => new ContentTypeSort { Id = new Lazy<int>(() => x.AllowedId), SortOrder = x.SortOrder }).ToList();
        }

        protected PropertyGroupCollection GetPropertyGroupCollection(int id)
        {
            var sql = new Sql();
            sql.Select("*")
               .From<PropertyTypeGroupDto>()
               .LeftJoin<PropertyTypeDto>()
               .On<PropertyTypeGroupDto, PropertyTypeDto>(left => left.Id, right => right.PropertyTypeGroupId)
               .LeftJoin<DataTypeDto>()
               .On<PropertyTypeDto, DataTypeDto>(left => left.DataTypeId, right => right.DataTypeId)
               .Where<PropertyTypeDto>(x => x.ContentTypeId == id)
               .OrderBy<PropertyTypeGroupDto>(x => x.Id);

            var dtos = Database.Fetch<PropertyTypeGroupDto, PropertyTypeDto, DataTypeDto, PropertyTypeGroupDto>(new GroupPropertyTypeRelator().Map, sql);

            var propertyFactory = new PropertyGroupFactory(id);
            var propertyGroups = propertyFactory.BuildEntity(dtos);
            return new PropertyGroupCollection(propertyGroups);
        }

        protected PropertyTypeCollection GetPropertyTypeCollection(int id)
        {
            var sql = new Sql();
            sql.Select("*")
               .From<PropertyTypeDto>()
               .InnerJoin<DataTypeDto>()
               .On<PropertyTypeDto, DataTypeDto>(left => left.DataTypeId, right => right.DataTypeId)
               .Where<PropertyTypeDto>(x => x.ContentTypeId == id);

            var dtos = Database.Fetch<PropertyTypeDto, DataTypeDto>(sql);

            //TODO Move this to a PropertyTypeFactory
            var list = (from dto in dtos
                        where (dto.PropertyTypeGroupId > 0) == false
                        select
                            new PropertyType(dto.DataTypeDto.ControlId,
                                             dto.DataTypeDto.DbType.EnumParse<DataTypeDatabaseType>(true))
                                {
                                    Alias = dto.Alias,
                                    DataTypeDefinitionId = dto.DataTypeId,
                                    Description = dto.Description,
                                    Id = dto.Id,
                                    Name = dto.Name,
                                    HelpText = dto.HelpText,
                                    Mandatory = dto.Mandatory,
                                    SortOrder = dto.SortOrder
                                });
            
            return new PropertyTypeCollection(list);
        }
    }
}