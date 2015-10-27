using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class UserSectionFactory 
    {
        private readonly IUser _user;

        public UserSectionFactory(IUser user)
        {
            _user = user;
        }

        public IEnumerable<string> BuildEntity(IEnumerable<User2AppDto> dto)
        {
            return dto.Select(x => x.AppAlias);
        }

        public IEnumerable<User2AppDto> BuildDto(IEnumerable<string> entity)
        {
            return entity.Select(x => new User2AppDto
                {
                    //NOTE: We're force casting to int here! this might not work in the future
                    UserId = (int)_user.Id,
                    AppAlias = x
                });
        }

    }
}