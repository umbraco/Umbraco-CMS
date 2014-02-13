﻿using System.Globalization;
using System.Linq;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class UserTypeFactory : IEntityFactory<IUserType, UserTypeDto>
    {
        #region Implementation of IEntityFactory<IUserType,UserTypeDto>

        public IUserType BuildEntity(UserTypeDto dto)
        {
            var userType = new UserType
                {
                    Alias = dto.Alias,
                    Id = dto.Id,
                    Name = dto.Name,
                    Permissions = dto.DefaultPermissions.IsNullOrWhiteSpace()
                                      ? Enumerable.Empty<string>()
                                      : dto.DefaultPermissions.ToCharArray().Select(x => x.ToString(CultureInfo.InvariantCulture))
                };
            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            userType.ResetDirtyProperties(false);
            return userType;
        }

        public UserTypeDto BuildDto(IUserType entity)
        {
            var userType = new UserTypeDto
                               {
                                   Alias = entity.Alias,
                                   DefaultPermissions = entity.Permissions == null ? "" : string.Join("", entity.Permissions),
                                   Name = entity.Name
                               };

            if(entity.HasIdentity)
                userType.Id = short.Parse(entity.Id.ToString());

            return userType;
        }

        #endregion
    }
}