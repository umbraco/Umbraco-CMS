using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class MemberReadOnlyFactory : IEntityFactory<IMembershipUser, MemberReadOnlyDto>
    {
        public IMembershipUser BuildEntity(MemberReadOnlyDto dto)
        {
            var member = new Member
                         {
                             Id = dto.NodeId,
                             CreateDate = dto.CreateDate,
                             UpdateDate = dto.UpdateDate,
                             Name = dto.Text,
                             Email = dto.Email,
                             Username = dto.LoginName,
                             Password = dto.Password,
                             ProviderUserKey = dto.UniqueId
                         };

            //Create PropertyType
            //Use PropertyType to create Property
            var properties = new List<Property>();
            foreach (var property in dto.Properties)
            {
                var propertyType = new PropertyType(property.ControlId,
                    property.DbType.EnumParse<DataTypeDatabaseType>(true))
                                   {
                                       Id = property.PropertyTypeId,
                                       Alias = property.Alias,
                                       Name = property.Name,
                                       Description = property.Description,
                                       HelpText = property.HelpText,
                                       Mandatory = property.Mandatory,
                                       ValidationRegExp = property.ValidationRegExp
                                       //SortOrder
                                       //PropertyGroupId
                                   };

                propertyType.ResetDirtyProperties(false);
                var prop = propertyType.CreatePropertyFromRawValue(property.GetValue, property.VersionId.Value, property.Id);
                properties.Add(prop);
            }

            member.Properties = new PropertyCollection(properties);

            member.SetProviderUserKeyType(typeof(Guid));
            member.ResetDirtyProperties(false);
            return member;
        }
        

        public MemberReadOnlyDto BuildDto(IMembershipUser entity)
        {
            throw new System.NotImplementedException();
        }
    }
}