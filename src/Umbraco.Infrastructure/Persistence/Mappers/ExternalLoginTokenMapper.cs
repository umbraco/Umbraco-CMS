using System;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers
{
    [MapperFor(typeof(IIdentityUserToken))]
    [MapperFor(typeof(IdentityUserToken))]
    public sealed class ExternalLoginTokenMapper : BaseMapper
    {
        public ExternalLoginTokenMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
            : base(sqlContext, maps)
        { }

        protected override void DefineMaps()
        {
            DefineMap<IdentityUserToken, ExternalLoginTokenDto>(nameof(IdentityUserToken.Id), nameof(ExternalLoginTokenDto.Id));
            DefineMap<IdentityUserToken, ExternalLoginTokenDto>(nameof(IdentityUserToken.CreateDate), nameof(ExternalLoginTokenDto.CreateDate));
            DefineMap<IdentityUserToken, ExternalLoginTokenDto>(nameof(IdentityUserToken.Name), nameof(ExternalLoginTokenDto.Name));
            DefineMap<IdentityUserToken, ExternalLoginTokenDto>(nameof(IdentityUserToken.Value), nameof(ExternalLoginTokenDto.Value));
            // separate table
            DefineMap<IdentityUserToken, ExternalLoginDto>(nameof(IdentityUserToken.UserId), nameof(ExternalLoginDto.UserId));
        }
    }

    [MapperFor(typeof(IIdentityUserToken))]
    [MapperFor(typeof(IdentityUserToken))]
    public sealed class ExternalLoginTokenWithKeyMapper : BaseMapper
    {
        public ExternalLoginTokenWithKeyMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
            : base(sqlContext, maps)
        { }

        protected override void DefineMaps()
        {
            DefineMap<IdentityUserToken, ExternalLoginTokenWithKeyDto>(nameof(IdentityUserToken.Id), nameof(ExternalLoginTokenWithKeyDto.Id));
            DefineMap<IdentityUserToken, ExternalLoginTokenWithKeyDto>(nameof(IdentityUserToken.CreateDate), nameof(ExternalLoginTokenWithKeyDto.CreateDate));
            DefineMap<IdentityUserToken, ExternalLoginTokenWithKeyDto>(nameof(IdentityUserToken.Name), nameof(ExternalLoginTokenWithKeyDto.Name));
            DefineMap<IdentityUserToken, ExternalLoginTokenWithKeyDto>(nameof(IdentityUserToken.Value), nameof(ExternalLoginTokenWithKeyDto.Value));
            // separate table
            DefineMap<IdentityUserToken, ExternalLoginWithKeyDto>(nameof(IdentityUserToken.Key), nameof(ExternalLoginWithKeyDto.UserOrMemberKey));
        }
    }
}
