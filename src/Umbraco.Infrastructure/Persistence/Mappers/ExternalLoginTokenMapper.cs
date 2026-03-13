using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
/// Provides mapping configuration between the <see cref="ExternalLoginToken"/> entity and its corresponding database schema.
/// </summary>
[MapperFor(typeof(IIdentityUserToken))]
[MapperFor(typeof(IdentityUserToken))]
public sealed class ExternalLoginTokenMapper : BaseMapper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalLoginTokenMapper"/> class, which is responsible for mapping external login token entities to database representations.
    /// </summary>
    /// <param name="sqlContext">A lazily-initialized SQL context used for database operations.</param>
    /// <param name="maps">The configuration store containing mapping definitions for entity properties.</param>
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
