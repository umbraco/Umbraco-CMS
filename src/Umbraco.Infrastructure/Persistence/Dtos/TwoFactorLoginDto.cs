using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[ExplicitColumns]
[PrimaryKey("Id")]
internal class TwoFactorLoginDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.TwoFactorLogin;

    [Column("id")]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    [Column("userOrMemberKey")]
    [Index(IndexTypes.NonClustered)]
    public Guid UserOrMemberKey { get; set; }

    [Column("providerName")]
    [Length(400)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Index(IndexTypes.UniqueNonClustered, ForColumns = "providerName,userOrMemberKey", Name = "IX_" + TableName + "_ProviderName")]
    public string ProviderName { get; set; } = null!;

    [Column("secret")]
    [Length(400)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Secret { get; set; } = null!;
}
