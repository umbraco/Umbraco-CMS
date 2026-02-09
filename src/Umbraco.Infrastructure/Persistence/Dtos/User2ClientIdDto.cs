using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
public class User2ClientIdDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.User2ClientId;
    public const string PrimaryKeyColumnName = "userId";

    private const string ClientIdColumnName = "clientId";

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_umbracoUser2ClientId", OnColumns = $"{PrimaryKeyColumnName}, {ClientIdColumnName}")]
    [ForeignKey(typeof(UserDto))]
    public int UserId { get; set; }

    [Column(ClientIdColumnName)]
    [Length(255)]
    public string? ClientId { get; set; }
}
