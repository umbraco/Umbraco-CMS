using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class MemberTypeReadOnlyFactory : IEntityFactory<IMemberType, MemberTypeReadOnlyDto>
    {
        public IMemberType BuildEntity(MemberTypeReadOnlyDto dto)
        {
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

            var propertyTypeGroupCollection = GetPropertyTypeGroupCollection(dto, memberType);
            memberType.PropertyGroups = propertyTypeGroupCollection;

            var propertyTypes = GetPropertyTypes(dto, memberType);
            //By Convention we add 9 stnd PropertyTypes - This is only here to support loading of types that didn't have these conventions before.
            var standardPropertyTypes = Constants.Conventions.Member.StandardPropertyTypeStubs;
            foreach (var standardPropertyType in standardPropertyTypes)
            {
                if(dto.PropertyTypes.Any(x => x.Alias.Equals(standardPropertyType.Key))) continue;
                
                //Add the standard PropertyType to the current list
                propertyTypes.Add(standardPropertyType.Value);

                //Internal dictionary for adding "MemberCanEdit" and "VisibleOnProfile" properties to each PropertyType
                memberType.MemberTypePropertyTypes.Add(standardPropertyType.Key,
                    new Tuple<bool, bool, int>(false, false, default(int)));
            }
            memberType.PropertyTypes = propertyTypes;

            return memberType;
        }

        private PropertyGroupCollection GetPropertyTypeGroupCollection(MemberTypeReadOnlyDto dto, MemberType memberType)
        {
            var propertyGroups = new PropertyGroupCollection();
            foreach (var groupDto in dto.PropertyTypeGroups.Where(x => x.Id.HasValue))
            {
                var group = new PropertyGroup();
                //Only assign an Id if the PropertyGroup belongs to this ContentType
                if (groupDto.Id.HasValue && groupDto.Id == memberType.Id)
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
                var typeDtos = dto.PropertyTypes.Where(x => x.Id.HasValue && x.Id > 0 && x.PropertyTypeGroupId.HasValue && x.PropertyTypeGroupId.Value == groupDto.Id.Value);
                foreach (var typeDto in typeDtos)
                {
                    //Internal dictionary for adding "MemberCanEdit" and "VisibleOnProfile" properties to each PropertyType
                    memberType.MemberTypePropertyTypes.Add(typeDto.Alias,
                        new Tuple<bool, bool, int>(typeDto.CanEdit, typeDto.ViewOnProfile, typeDto.Id.Value));

                    var tempGroupDto = groupDto;
                    var propertyType = new PropertyType(typeDto.ControlId,
                                                             typeDto.DbType.EnumParse<DataTypeDatabaseType>(true))
                    {
                        Alias = typeDto.Alias,
                        DataTypeDefinitionId = typeDto.DataTypeId,
                        Description = typeDto.Description,
                        Id = typeDto.Id.Value,
                        Name = typeDto.Name,
                        HelpText = typeDto.HelpText,
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

        private List<PropertyType> GetPropertyTypes(MemberTypeReadOnlyDto dto, MemberType memberType)
        {
            //Find PropertyTypes that does not belong to a PropertyTypeGroup
            var propertyTypes = new List<PropertyType>();
            foreach (var propertyType in dto.PropertyTypes.Where(x => (x.PropertyTypeGroupId.HasValue == false || x.PropertyTypeGroupId.Value == 0) && x.Id.HasValue))
            {
                //Internal dictionary for adding "MemberCanEdit" and "VisibleOnProfile" properties to each PropertyType
                memberType.MemberTypePropertyTypes.Add(propertyType.Alias,
                    new Tuple<bool, bool, int>(propertyType.CanEdit, propertyType.ViewOnProfile, propertyType.Id.Value));
                //PropertyType Collection
                propertyTypes.Add(new PropertyType(propertyType.PropertyEditorAlias,
                    propertyType.DbType.EnumParse<DataTypeDatabaseType>(true))
                                  {
                                      Alias = propertyType.Alias,
                                      DataTypeDefinitionId = propertyType.DataTypeId,
                                      Description = propertyType.Description,
                                      HelpText = propertyType.HelpText,
                                      Id = propertyType.Id.Value,
                                      Mandatory = propertyType.Mandatory,
                                      Name = propertyType.Name,
                                      SortOrder = propertyType.SortOrder,
                                      ValidationRegExp = propertyType.ValidationRegExp,
                                      PropertyGroupId = new Lazy<int>(() => default(int)),
                                      CreateDate = dto.CreateDate,
                                      UpdateDate = dto.CreateDate
                                  });
            }
            return propertyTypes;
        }

        public MemberTypeReadOnlyDto BuildDto(IMemberType entity)
        {
            throw new System.NotImplementedException();
        }
        
    }
}