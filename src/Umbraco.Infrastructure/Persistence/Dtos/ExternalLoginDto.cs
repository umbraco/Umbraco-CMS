using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[ExplicitColumns]
[PrimaryKey(PrimaryKeyColumnName)]
internal sealed class ExternalLoginDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ExternalLogin;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    private const string UserIdColumnName = "userId";
    private const string UserOrMemberKeyColumnName = "userOrMemberKey";
    private const string LoginProviderColumnName = "loginProvider";
    private const string ProviderKeyColumnName = "providerKey";


    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    [Obsolete("This only exists to ensure you can upgrade using external logins from umbraco version where this was used to the new where it is not used")]
    [ResultColumn(UserIdColumnName)]
    public int? UserId { get; set; }

    [Column(UserOrMemberKeyColumnName)]
    [Index(IndexTypes.NonClustered)]
    public Guid UserOrMemberKey { get; set; }

    /// <summary>
    ///     Used to store the name of the provider (i.e. Facebook, Google)
    /// </summary>
    [Column(LoginProviderColumnName)]
    [Length(400)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Index(IndexTypes.UniqueNonClustered, ForColumns = $"{LoginProviderColumnName},{UserOrMemberKeyColumnName}", Name = "IX_" + TableName + "_LoginProvider")]
    public string LoginProvider { get; set; } = null!;

    /// <summary>
    ///     Stores the key the provider uses to lookup the login
    /// </summary>
    [Column(ProviderKeyColumnName)]
    [Length(4000)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Index(IndexTypes.NonClustered, ForColumns = $"{LoginProviderColumnName},{ProviderKeyColumnName}", Name = "IX_" + TableName + "_ProviderKey")]
    public string ProviderKey { get; set; } = null!;

    [Column("createDate")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime CreateDate { get; set; }

    /// <summary>
    ///     Used to store any arbitrary data for the user and external provider - like user tokens returned from the provider
    /// </summary>
    [Column("userData")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    public string? UserData { get; set; }
}
