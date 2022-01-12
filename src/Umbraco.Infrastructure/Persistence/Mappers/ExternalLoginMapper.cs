using System;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers
{
    [MapperFor(typeof(IIdentityUserLogin))]
    [MapperFor(typeof(IdentityUserLogin))]
    public sealed class ExternalLoginMapper : BaseMapper
    {
        public ExternalLoginMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
            : base(sqlContext, maps)
        { }

        protected override void DefineMaps()
        {
            DefineMap<IdentityUserLogin, ExternalLoginDto>(nameof(IdentityUserLogin.Id), nameof(ExternalLoginDto.Id));
            DefineMap<IdentityUserLogin, ExternalLoginDto>(nameof(IdentityUserLogin.CreateDate), nameof(ExternalLoginDto.CreateDate));
            DefineMap<IdentityUserLogin, ExternalLoginDto>(nameof(IdentityUserLogin.LoginProvider), nameof(ExternalLoginDto.LoginProvider));
            DefineMap<IdentityUserLogin, ExternalLoginDto>(nameof(IdentityUserLogin.ProviderKey), nameof(ExternalLoginDto.ProviderKey));
            DefineMap<IdentityUserLogin, ExternalLoginDto>(nameof(IdentityUserLogin.Key), nameof(ExternalLoginDto.UserId));
        }
    }

    [MapperFor(typeof(IIdentityUserLogin))]
    [MapperFor(typeof(IdentityUserLogin))]
    public sealed class ExternalLoginWithKeyMapper : BaseMapper
    {
        public ExternalLoginWithKeyMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
            : base(sqlContext, maps)
        { }

        protected override void DefineMaps()
        {
            DefineMap<IdentityUserLogin, ExternalLoginWithKeyDto>(nameof(IdentityUserLogin.Id), nameof(ExternalLoginWithKeyDto.Id));
            DefineMap<IdentityUserLogin, ExternalLoginWithKeyDto>(nameof(IdentityUserLogin.CreateDate), nameof(ExternalLoginWithKeyDto.CreateDate));
            DefineMap<IdentityUserLogin, ExternalLoginWithKeyDto>(nameof(IdentityUserLogin.LoginProvider), nameof(ExternalLoginWithKeyDto.LoginProvider));
            DefineMap<IdentityUserLogin, ExternalLoginWithKeyDto>(nameof(IdentityUserLogin.ProviderKey), nameof(ExternalLoginWithKeyDto.ProviderKey));
            DefineMap<IdentityUserLogin, ExternalLoginWithKeyDto>(nameof(IdentityUserLogin.Key), nameof(ExternalLoginWithKeyDto.UserOrMemberKey));
            DefineMap<IdentityUserLogin, ExternalLoginWithKeyDto>(nameof(IdentityUserLogin.UserData), nameof(ExternalLoginWithKeyDto.UserData));
        }
    }
}
