using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Repositories.Implement;

namespace Umbraco.Core.Persistence.Factories
{
    internal static class MemberTypeReadOnlyFactory
    {
        public static IMemberType BuildEntity(MemberTypeReadOnlyDto dto)
        {
            var standardPropertyTypes = Constants.Conventions.Member.GetStandardPropertyTypeStubs();
            
            var memberType = new MemberType(dto.ParentId);

            try
            {
                memberType.DisableChangeTracking();

                memberType.Alias = dto.Alias;
                memberType.AllowedAsRoot = dto.AllowAtRoot;
                memberType.CreateDate = dto.CreateDate;
                memberType.CreatorId = dto.UserId.HasValue ? dto.UserId.Value : 0;
                memberType.Description = dto.Description;
                memberType.Icon = dto.Icon;
                memberType.Id = dto.NodeId;
                memberType.IsContainer = dto.IsContainer;
                memberType.Key = dto.UniqueId.Value;
                memberType.Level = dto.Level;
                memberType.Name = dto.Text;
                memberType.Path = dto.Path;
                memberType.SortOrder = dto.SortOrder;
                memberType.Thumbnail = dto.Thumbnail;
                memberType.Trashed = dto.Trashed;
                memberType.UpdateDate = dto.CreateDate;
                memberType.AllowedContentTypes = Enumerable.Empty<ContentTypeSort>();

                var propertyTypeGroupCollection = GetPropertyTypeGroupCollection(dto, memberType, standardPropertyTypes);
                memberType.PropertyGroups = propertyTypeGroupCollection;

                memberType.NoGroupPropertyTypes = GetNoGroupPropertyTypes(dto, memberType, standardPropertyTypes);

                // By Convention we add 9 standard PropertyTypes - This is only here to support loading of types that didn't have these conventions before.
                // In theory this should not happen! The only reason this did happen was because:
                // A) we didn't install all of the default membership properties by default
                // B) the existing data is super old when we didn't store membership properties at all (very very old)
                // So what to do? We absolutely do not want to update the database when only a read was requested, this will cause problems
                // so we should just add these virtual properties, they will have no ids but they will have aliases and this should be perfectly
                // fine for any membership provider logic to work since neither membership providers or asp.net identity care about whether a property
                // has an ID or not.
                // When the member type is saved, all the properties will correctly be added.

                //This will add this group if it doesn't exist, no need to error check here
                memberType.AddPropertyGroup(Constants.Conventions.Member.StandardPropertiesGroupName);
                foreach (var standardPropertyType in standardPropertyTypes)
                {
                    //This will add the property if it doesn't exist, no need to error check here
                    memberType.AddPropertyType(standardPropertyType.Value, Constants.Conventions.Member.StandardPropertiesGroupName);

                    //Internal dictionary for adding "MemberCanEdit", "VisibleOnProfile", "IsSensitive" properties to each PropertyType
                    if (!memberType.MemberTypePropertyTypes.TryGetValue(standardPropertyType.Key, out var memberTypePropertyProfile))
                        memberType.MemberTypePropertyTypes[standardPropertyType.Key] = new MemberTypePropertyProfileAccess(false, false, false);
                }

                return memberType;
            }
            finally
            {
                memberType.EnableChangeTracking();
            }
        }

        private static PropertyGroupCollection GetPropertyTypeGroupCollection(MemberTypeReadOnlyDto dto, MemberType memberType, Dictionary<string, PropertyType> standardProps)
        {
            // see PropertyGroupFactory, repeating code here...

            var propertyGroups = new PropertyGroupCollection();
            foreach (var groupDto in dto.PropertyTypeGroups.Where(x => x.Id.HasValue))
            {
                var group = new PropertyGroup(MemberType.SupportsPublishingConst);

                // if the group is defined on the current member type,
                // assign its identifier, else it will be zero
                if (groupDto.ContentTypeNodeId == memberType.Id)
                {
                    // note: no idea why Id is nullable here, but better check
                    if (groupDto.Id.HasValue == false)
                        throw new Exception("GroupDto.Id has no value.");
                    group.Id = groupDto.Id.Value;
                }

                group.Key = groupDto.UniqueId;
                group.Name = groupDto.Text;
                group.SortOrder = groupDto.SortOrder;
                group.PropertyTypes = new PropertyTypeCollection(MemberType.SupportsPublishingConst);

                //Because we are likely to have a group with no PropertyTypes we need to ensure that these are excluded
                var localGroupDto = groupDto;
                var typeDtos = dto.PropertyTypes.Where(x => x.Id.HasValue && x.Id > 0 && x.PropertyTypeGroupId.HasValue && x.PropertyTypeGroupId.Value == localGroupDto.Id.Value);
                foreach (var typeDto in typeDtos)
                {
                    //Internal dictionary for adding "MemberCanEdit" and "VisibleOnProfile" properties to each PropertyType
                    memberType.MemberTypePropertyTypes.Add(typeDto.Alias,
                        new MemberTypePropertyProfileAccess(typeDto.ViewOnProfile, typeDto.CanEdit, typeDto.IsSensitive));

                    var tempGroupDto = groupDto;

                    //ensures that any built-in membership properties have their correct dbtype assigned no matter
                    //what the underlying data type is
                    var propDbType = MemberTypeRepository.GetDbTypeForBuiltInProperty(
                        typeDto.Alias,
                        typeDto.DbType.EnumParse<ValueStorageType>(true),
                        standardProps);

                    var propertyType = new PropertyType(
                        typeDto.PropertyEditorAlias,
                        propDbType.Result,
                        //This flag tells the property type that it has an explicit dbtype and that it cannot be changed
                        // which is what we want for the built-in properties.
                        propDbType.Success,
                        typeDto.Alias)
                    {
                        DataTypeId = typeDto.DataTypeId,
                        Description = typeDto.Description,
                        Id = typeDto.Id.Value,
                        Name = typeDto.Name,
                        Mandatory = typeDto.Mandatory,
                        SortOrder = typeDto.SortOrder,
                        ValidationRegExp = typeDto.ValidationRegExp,
                        PropertyGroupId = new Lazy<int>(() => tempGroupDto.Id.Value),
                        CreateDate = memberType.CreateDate,
                        UpdateDate = memberType.UpdateDate,
                        Key = typeDto.UniqueId
                    };

                    // reset dirty initial properties (U4-1946)
                    propertyType.ResetDirtyProperties(false);
                    group.PropertyTypes.Add(propertyType);
                }

                // reset dirty initial properties (U4-1946)
                group.ResetDirtyProperties(false);
                propertyGroups.Add(group);
            }

            return propertyGroups;
        }

        private static List<PropertyType> GetNoGroupPropertyTypes(MemberTypeReadOnlyDto dto, MemberType memberType, Dictionary<string, PropertyType> standardProps)
        {
            //Find PropertyTypes that does not belong to a PropertyTypeGroup
            var propertyTypes = new List<PropertyType>();
            foreach (var typeDto in dto.PropertyTypes.Where(x => (x.PropertyTypeGroupId.HasValue == false || x.PropertyTypeGroupId.Value == 0) && x.Id.HasValue))
            {
                //Internal dictionary for adding "MemberCanEdit" and "VisibleOnProfile" properties to each PropertyType

                memberType.MemberTypePropertyTypes.Add(typeDto.Alias,
                    new MemberTypePropertyProfileAccess(typeDto.ViewOnProfile, typeDto.CanEdit, typeDto.IsSensitive));

                //ensures that any built-in membership properties have their correct dbtype assigned no matter
                //what the underlying data type is
                var propDbType = MemberTypeRepository.GetDbTypeForBuiltInProperty(
                    typeDto.Alias,
                    typeDto.DbType.EnumParse<ValueStorageType>(true),
                    standardProps);

                var propertyType = new PropertyType(
                    typeDto.PropertyEditorAlias,
                    propDbType.Result,
                    //This flag tells the property type that it has an explicit dbtype and that it cannot be changed
                    // which is what we want for the built-in properties.
                    propDbType.Success,
                    typeDto.Alias)
                {
                    DataTypeId = typeDto.DataTypeId,
                    Description = typeDto.Description,
                    Id = typeDto.Id.Value,
                    Mandatory = typeDto.Mandatory,
                    Name = typeDto.Name,
                    SortOrder = typeDto.SortOrder,
                    ValidationRegExp = typeDto.ValidationRegExp,
                    PropertyGroupId = null,
                    CreateDate = dto.CreateDate,
                    UpdateDate = dto.CreateDate,
                    Key = typeDto.UniqueId
                };

                propertyTypes.Add(propertyType);
            }
            return propertyTypes;
        }

    }
}
