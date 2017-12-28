using System.Collections.Concurrent;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof(IIdentityUserLogin))]
    [MapperFor(typeof(IdentityUserLogin))]
    public sealed class ExternalLoginMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();
        public ExternalLoginMapper()
        {
            BuildMap();
        }

        #region Overrides of BaseMapper

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache => PropertyInfoCacheInstance;

        protected override void BuildMap()
        {
            CacheMap<IdentityUserLogin, ExternalLoginDto>(src => src.Id, dto => dto.Id);
            CacheMap<IdentityUserLogin, ExternalLoginDto>(src => src.CreateDate, dto => dto.CreateDate);
            CacheMap<IdentityUserLogin, ExternalLoginDto>(src => src.LoginProvider, dto => dto.LoginProvider);
            CacheMap<IdentityUserLogin, ExternalLoginDto>(src => src.ProviderKey, dto => dto.ProviderKey);
            CacheMap<IdentityUserLogin, ExternalLoginDto>(src => src.UserId, dto => dto.UserId);
        }

        #endregion
    }
}
