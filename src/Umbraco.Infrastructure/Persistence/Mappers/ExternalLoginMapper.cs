using System;
using System.Collections.Concurrent;
using Umbraco.Cms.Core.Models.Identity;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Infrastructure.Persistence.Mappers;

namespace Umbraco.Core.Persistence.Mappers
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
            DefineMap<IdentityUserLogin, ExternalLoginDto>(nameof(IdentityUserLogin.UserId), nameof(ExternalLoginDto.UserId));
        }
    }
}
