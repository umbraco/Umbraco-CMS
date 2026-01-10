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

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    [Column(UserOrMemberKeyColumnName)]
    [Index(IndexTypes.NonClustered)]
    public Guid UserOrMemberKey { get; set; }

    [Column(ProviderNameColumnName)]
    [Length(400)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Index(IndexTypes.UniqueNonClustered, ForColumns = $"{ProviderNameColumnName},{UserOrMemberKeyColumnName}", Name = "IX_" + TableName + "_ProviderName")]
    public string ProviderName { get; set; } = null!;

    [Column("secret")]
    [Length(400)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Secret { get; set; } = null!;
}
