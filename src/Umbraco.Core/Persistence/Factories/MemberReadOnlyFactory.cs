using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class MemberReadOnlyFactory : IEntityFactory<IMember, MemberReadOnlyDto>
    {
        private readonly IDictionary<string, IMemberType> _memberTypes;

        public MemberReadOnlyFactory(IDictionary<string, IMemberType> memberTypes)
        {
            _memberTypes = memberTypes;
        }

        public IMember BuildEntity(MemberReadOnlyDto dto)
        {
            var properties = CreateProperties(_memberTypes[dto.ContentTypeAlias], dto.Properties, dto.CreateDate);

            var member = new Member(dto.Text, dto.Email, dto.LoginName, dto.Password, _memberTypes[dto.ContentTypeAlias])
                         {
                             Id = dto.NodeId,
                             CreateDate = dto.CreateDate,
                             UpdateDate = dto.UpdateDate,
                             Name = dto.Text,
                             ProviderUserKey = dto.NodeId,
                             Trashed = dto.Trashed,
                             Key = dto.UniqueId.Value,
                             CreatorId = dto.UserId.HasValue ? dto.UserId.Value : 0,
                             Level = dto.Level,
                             Path = dto.Path,
                             SortOrder = dto.SortOrder,
                             Version = dto.VersionId,
                             Properties = new PropertyCollection(properties)
                         };

            member.SetProviderUserKeyType(typeof(Guid));
            member.ResetDirtyProperties(false);
            return member;
        }

        public MemberReadOnlyDto BuildDto(IMember entity)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<Property> CreateProperties(IMemberType memberType, IEnumerable<PropertyDataReadOnlyDto> dtos, DateTime createDate)
        {
            var properties = new List<Property>();

            foreach (var propertyType in memberType.CompositionPropertyTypes)
            {
                var propertyDataDto = dtos.LastOrDefault(x => x.PropertyTypeId == propertyType.Id);
                var property = propertyDataDto == null
                                   ? propertyType.CreatePropertyFromValue(null)
                                   : propertyType.CreatePropertyFromRawValue(propertyDataDto.GetValue,
                                                                             propertyDataDto.VersionId,
                                                                             propertyDataDto.PropertyDataId.Value);
                //on initial construction we don't want to have dirty properties tracked
                property.CreateDate = createDate;
                property.UpdateDate = createDate;
                // http://issues.umbraco.org/issue/U4-1946
                property.ResetDirtyProperties(false);
                properties.Add(property);
            }

            return properties;
        }
    }
}