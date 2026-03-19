using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
/// Provides mapping configuration between the <c>ExternalLogin</c> entity and its corresponding database schema.
/// </summary>
[MapperFor(typeof(IIdentityUserLogin))]
[MapperFor(typeof(IdentityUserLogin))]
public sealed class ExternalLoginMapper : BaseMapper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalLoginMapper"/> class.
    /// </summary>
    /// <param name="sqlContext">The lazy-loaded SQL context used for database operations.</param>
    /// <param name="maps">The mapper configuration store used to configure mappings.</param>
    public ExternalLoginMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<IdentityUserLogin, ExternalLoginDto>(nameof(IdentityUserLogin.Id), nameof(ExternalLoginDto.Id));
        DefineMap<IdentityUserLogin, ExternalLoginDto>(
            nameof(IdentityUserLogin.CreateDate),
            nameof(ExternalLoginDto.CreateDate));
        DefineMap<IdentityUserLogin, ExternalLoginDto>(
            nameof(IdentityUserLogin.LoginProvider),
            nameof(ExternalLoginDto.LoginProvider));
        DefineMap<IdentityUserLogin, ExternalLoginDto>(
            nameof(IdentityUserLogin.ProviderKey),
            nameof(ExternalLoginDto.ProviderKey));
        DefineMap<IdentityUserLogin, ExternalLoginDto>(
            nameof(IdentityUserLogin.Key),
            nameof(ExternalLoginDto.UserOrMemberKey));
        DefineMap<IdentityUserLogin, ExternalLoginDto>(
            nameof(IdentityUserLogin.UserData),
            nameof(ExternalLoginDto.UserData));
    }
}
