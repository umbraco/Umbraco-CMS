using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Repositories;

namespace Umbraco.Core.Persistence.Factories
{
    internal class MemberTypeReadOnlyFactory 
    {
        public IMemberType BuildEntity(MemberTypeReadOnlyDto dto)
        {
            var standardPropertyTypes = Constants.Conventions.Member.GetStandardPropertyTypeStubs();
            
            var memberType = new MemberType(dto.ParentId)
                             {
                                 Alias = dto.Alias,
                                 AllowedAsRoot = dto.AllowAtRoot,
                                 CreateDate = dto.CreateDate,
                                 CreatorId = dto.UserId.HasValue ? dto.UserId.Value : 0,
                                 Description = dto.Description,
                                 Icon = dto.Icon,
                                 Id = dto.NodeId,
                                 IsContainer = dto.IsContainer,
                                 Key = dto.UniqueId.Value,
                                 Level = dto.Level,
                                 Name = dto.Text,
                                 Path = dto.Path,
                                 SortOrder = dto.SortOrder,
                                 Thumbnail = dto.Thumbnail,
                                 Trashed = dto.Trashed,
                                 UpdateDate = dto.CreateDate,
                                 AllowedContentTypes = Enumerable.Empty<ContentTypeSort>()
                             };

            var propertyTypeGroupCollection = GetPropertyTypeGroupCollection(dto, memberType, standardPropertyTypes);
            memberType.PropertyGroups = propertyTypeGroupCollection;

            var propertyTypes = GetPropertyTypes(dto, memberType, standardPropertyTypes);

            //By Convention we add 9 stnd PropertyTypes - This is only here to support loading of types that didn't have these conventions before.            
            foreach (var standardPropertyType in standardPropertyTypes)
            {
                if(dto.PropertyTypes.Any(x => x.Alias.Equals(standardPropertyType.Key))) continue;
                
                //Add the standard PropertyType to the current list
                propertyTypes.Add(standardPropertyType.Value);

                //Internal dictionary for adding "MemberCanEdit" and "VisibleOnProfile" properties to each PropertyType
                memberType.MemberTypePropertyTypes.Add(standardPropertyType.Key,
                    new MemberTypePropertyProfileAccess(false, false));
            }
            memberType.PropertyTypes = propertyTypes;

            return memberType;
        }

        private PropertyGroupCollection GetPropertyTypeGroupCollection(MemberTypeReadOnlyDto dto, MemberType memberType, Dictionary<string, PropertyType> standardProps)
        {
            var propertyGroups = new PropertyGroupCollection();            
            
            foreach (var groupDto in dto.PropertyTypeGroups.Where(x => x.Id.HasValue))
            {
                var group = new PropertyGroup();
               
                //Only assign an Id if the PropertyGroup belongs to this ContentType
                if (groupDto.ContentTypeNodeId == memberType.Id)
                {
                    group.Id = groupDto.Id.Value;

                    if (groupDto.ParentGroupId.HasValue)
                        group.ParentId = groupDto.ParentGroupId.Value;
                }
                else
                {
                    //If the PropertyGroup is inherited, we add a reference to the group as a Parent.
                    group.ParentId = groupDto.Id;
                }

                group.Name = groupDto.Text;
                group.SortOrder = groupDto.SortOrder;
                group.PropertyTypes = new PropertyTypeCollection();

                //Because we are likely to have a group with no PropertyTypes we need to ensure that these are excluded
                var localGroupDto = groupDto;
                var typeDtos = dto.PropertyTypes.Where(x => x.Id.HasValue && x.Id > 0 && x.PropertyTypeGroupId.HasValue && x.PropertyTypeGroupId.Value == localGroupDto.Id.Value);
                foreach (var typeDto in typeDtos)
                {
                    //Internal dictionary for adding "MemberCanEdit" and "VisibleOnProfile" properties to each PropertyType
                    memberType.MemberTypePropertyTypes.Add(typeDto.Alias,
                        new MemberTypePropertyProfileAccess(typeDto.ViewOnProfile, typeDto.CanEdit));

                    var tempGroupDto = groupDto;

                    //ensures that any built-in membership properties have their correct dbtype assigned no matter
                    //what the underlying data type is
                    var propDbType = MemberTypeRepository.GetDbTypeForBuiltInProperty(
                        typeDto.Alias,
                        typeDto.DbType.EnumParse<DataTypeDatabaseType>(true),
                        standardProps);
                    
                    var propertyType = new PropertyType(
                        typeDto.PropertyEditorAlias,
                        propDbType.Result,
                        //This flag tells the property type that it has an explicit dbtype and that it cannot be changed
                        // which is what we want for the built-in properties.
                        propDbType.Success)
                    {
                        Alias = typeDto.Alias,
                        DataTypeDefinitionId = typeDto.DataTypeId,
                        Description = typeDto.Description,
                        Id = typeDto.Id.Value,
                        Name = typeDto.Name,
                        Mandatory = typeDto.Mandatory,
                        SortOrder = typeDto.SortOrder,
                        ValidationRegExp = typeDto.ValidationRegExp,
                        PropertyGroupId = new Lazy<int>(() => tempGroupDto.Id.Value),
                        CreateDate = memberType.CreateDate,
                        UpdateDate = memberType.UpdateDate
                    };
                    //on initial construction we don't want to have dirty properties tracked
                    // http://issues.umbraco.org/issue/U4-1946
                    propertyType.ResetDirtyProperties(false);
                    group.PropertyTypes.Add(propertyType);
                }
                //on initial construction we don't want to have dirty properties tracked
                // http://issues.umbraco.org/issue/U4-1946
                group.ResetDirtyProperties(false);
                propertyGroups.Add(group);
            }

            return propertyGroups;
        }



        private List<PropertyType> GetPropertyTypes(MemberTypeReadOnlyDto dto, MemberType memberType, Dictionary<string, PropertyType> standardProps)
        {
            //Find PropertyTypes that does not belong to a PropertyTypeGroup
            var propertyTypes = new List<PropertyType>();
            foreach (var typeDto in dto.PropertyTypes.Where(x => (x.PropertyTypeGroupId.HasValue == false || x.PropertyTypeGroupId.Value == 0) && x.Id.HasValue))
            {
                //Internal dictionary for adding "MemberCanEdit" and "VisibleOnProfile" properties to each PropertyType
                memberType.MemberTypePropertyTypes.Add(typeDto.Alias,
                    new MemberTypePropertyProfileAccess(typeDto.ViewOnProfile, typeDto.CanEdit));

                //ensures that any built-in membership properties have their correct dbtype assigned no matter
                //what the underlying data type is
                var propDbType = MemberTypeRepository.GetDbTypeForBuiltInProperty(
                    typeDto.Alias,
                    typeDto.DbType.EnumParse<DataTypeDatabaseType>(true),
                    standardProps);

                var propertyType = new PropertyType(
                    typeDto.PropertyEditorAlias,
                    propDbType.Result,
                    //This flag tells the property type that it has an explicit dbtype and that it cannot be changed
                    // which is what we want for the built-in properties.
                    propDbType.Success)
                {
                    Alias = typeDto.Alias,
                    DataTypeDefinitionId = typeDto.DataTypeId,
                    Description = typeDto.Description,
                    Id = typeDto.Id.Value,
                    Mandatory = typeDto.Mandatory,
                    Name = typeDto.Name,
                    SortOrder = typeDto.SortOrder,
                    ValidationRegExp = typeDto.ValidationRegExp,
                    PropertyGroupId = new Lazy<int>(() => default(int)),
                    CreateDate = dto.CreateDate,
                    UpdateDate = dto.CreateDate
                };
                
                propertyTypes.Add(propertyType);
            }
            return propertyTypes;
        }

        public MemberTypeReadOnlyDto BuildDto(IMemberType entity)
        {
            throw new System.NotImplementedException();
        }
        
    }
}