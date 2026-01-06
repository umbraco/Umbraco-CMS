using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyName, AutoIncrement = false)]
[ExplicitColumns]
public class User2ClientIdDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.User2ClientId;
    public const string PrimaryKeyName = "userId";

    [Column(PrimaryKeyName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_umbracoUser2ClientId", OnColumns = $"{PrimaryKeyName}, clientId")]
    [ForeignKey(typeof(UserDto))]
    public int UserId { get; set; }

    [Column("clientId")]
    [Length(255)]
    public string? ClientId { get; set; }
}
