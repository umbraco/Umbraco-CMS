using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.User2ClientId)]
[PrimaryKey("userId", AutoIncrement = false)]
[ExplicitColumns]
public class User2ClientIdDto
{
    [Column("userId")]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_umbracoUser2ClientId", OnColumns = "userId, clientId")]
    [ForeignKey(typeof(UserDto))]
    public int UserId { get; set; }

    [Column("clientId")]
    [Length(255)]
    public string? ClientId { get; set; }
}
