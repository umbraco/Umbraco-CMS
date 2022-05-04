using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

[MapperFor(typeof(IIdentityUserToken))]
[MapperFor(typeof(IdentityUserToken))]
public sealed class ExternalLoginTokenMapper : BaseMapper
{
    public ExternalLoginTokenMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<IdentityUserToken, ExternalLoginTokenDto>(
            nameof(IdentityUserToken.Id),
            nameof(ExternalLoginTokenDto.Id));
        DefineMap<IdentityUserToken, ExternalLoginTokenDto>(
            nameof(IdentityUserToken.CreateDate),
            nameof(ExternalLoginTokenDto.CreateDate));
        DefineMap<IdentityUserToken, ExternalLoginTokenDto>(
            nameof(IdentityUserToken.Name),
            nameof(ExternalLoginTokenDto.Name));
        DefineMap<IdentityUserToken, ExternalLoginTokenDto>(
            nameof(IdentityUserToken.Value),
            nameof(ExternalLoginTokenDto.Value));

        // separate table
        DefineMap<IdentityUserLogin, ExternalLoginDto>(
            nameof(IdentityUserLogin.Key),
            nameof(ExternalLoginDto.UserOrMemberKey));
    }
}
