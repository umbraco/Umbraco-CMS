using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[ExplicitColumns]
[PrimaryKey("Id")]
internal class ExternalLoginDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ExternalLogin;

    [Column("id")]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    [Obsolete("This only exists to ensure you can upgrade using external logins from umbraco version where this was used to the new where it is not used")]
    [ResultColumn("userId")]
    public int? UserId { get; set; }

    [Column("userOrMemberKey")]
    [Index(IndexTypes.NonClustered)]
    public Guid UserOrMemberKey { get; set; }

    /// <summary>
    ///     Used to store the name of the provider (i.e. Facebook, Google)
    /// </summary>
    [Column("loginProvider")]
    [Length(400)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Index(IndexTypes.UniqueNonClustered, ForColumns = "loginProvider,userOrMemberKey", Name = "IX_" + TableName + "_LoginProvider")]
    public string LoginProvider { get; set; } = null!;

    /// <summary>
    ///     Stores the key the provider uses to lookup the login
    /// </summary>
    [Column("providerKey")]
    [Length(4000)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Index(IndexTypes.NonClustered, ForColumns = "loginProvider,providerKey", Name = "IX_" + TableName + "_ProviderKey")]
    public string ProviderKey { get; set; } = null!;

    [Column("createDate")]
    [Constraint(Default = SystemMethods.CurrentDateTime)]
    public DateTime CreateDate { get; set; }

    /// <summary>
    ///     Used to store any arbitrary data for the user and external provider - like user tokens returned from the provider
    /// </summary>
    [Column("userData")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [SpecialDbType(SpecialDbTypes.NTEXT)]
    public string? UserData { get; set; }
}
