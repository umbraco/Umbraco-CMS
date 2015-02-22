using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof(IUser))]
    [MapperFor(typeof(User))]
    public sealed class UserMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();


        #region Overrides of BaseMapper

        public UserMapper(ISqlSyntaxProvider sqlSyntax) : base(sqlSyntax)
        {
        }

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache
        {
            get { return PropertyInfoCacheInstance; }
        }

        protected override void BuildMap()
        {
            CacheMap<User, UserDto>(src => src.Id, dto => dto.Id);
            CacheMap<User, UserDto>(src => src.Email, dto => dto.Email);
            CacheMap<User, UserDto>(src => src.Username, dto => dto.Login);
            CacheMap<User, UserDto>(src => src.RawPasswordValue, dto => dto.Password);
            CacheMap<User, UserDto>(src => src.Name, dto => dto.UserName);
            //NOTE: This column in the db is *not* used!
            //CacheMap<User, UserDto>(src => src.DefaultPermissions, dto => dto.DefaultPermissions);
            CacheMap<User, UserDto>(src => src.StartMediaId, dto => dto.MediaStartId);
            CacheMap<User, UserDto>(src => src.StartContentId, dto => dto.ContentStartId);
            CacheMap<User, UserDto>(src => src.IsApproved, dto => dto.Disabled);
            CacheMap<User, UserDto>(src => src.IsLockedOut, dto => dto.NoConsole);
            CacheMap<User, UserDto>(src => src.UserType, dto => dto.Type);
            CacheMap<User, UserDto>(src => src.Language, dto => dto.UserLanguage);
        }

        #endregion
    }
}