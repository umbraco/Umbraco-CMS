using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[ExplicitColumns]
[PrimaryKey(PrimaryKeyColumnName)]
internal sealed class TwoFactorLoginDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.TwoFactorLogin;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    private const string UserOrMemberKeyColumnName = "userOrMemberKey";
    private const string ProviderNameColumnName = "providerName";

    /// <summary>
    /// Gets or sets the unique identifier for the two-factor login record.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique key identifying the user or member for this two-factor login entry.
    /// </summary>
    [Column(UserOrMemberKeyColumnName)]
    [Index(IndexTypes.NonClustered)]
    public Guid UserOrMemberKey { get; set; }

    /// <summary>
    /// Gets or sets the name of the two-factor authentication provider.
    /// </summary>
    [Column(ProviderNameColumnName)]
    [Length(400)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Index(IndexTypes.UniqueNonClustered, ForColumns = $"{ProviderNameColumnName},{UserOrMemberKeyColumnName}", Name = "IX_" + TableName + "_ProviderName")]
    public string ProviderName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the secret key used for two-factor authentication.
    /// </summary>
    [Column("secret")]
    [Length(400)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Secret { get; set; } = null!;
}
