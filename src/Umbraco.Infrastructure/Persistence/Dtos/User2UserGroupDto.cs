using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Represents a data transfer object (DTO) that defines the association between a user and a user group in the persistence layer.
/// Typically used to map user-to-group relationships in the database.
/// </summary>
[TableName(TableName)]
[PrimaryKey([UserIdColumnName, UserGroupIdColumnName], AutoIncrement = false)]
[ExplicitColumns]
public class User2UserGroupDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.User2UserGroup;

    private const string UserIdColumnName = "userId";
    private const string UserGroupIdColumnName = "userGroupId";

    /// <summary>
    /// Gets or sets the identifier of the user associated with this user-to-user group relationship.
    /// </summary>
    [Column(UserIdColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_user2userGroup", OnColumns = $"{UserIdColumnName}, {UserGroupIdColumnName}")]
    [ForeignKey(typeof(UserDto))]
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user group.
    /// </summary>
    [Column(UserGroupIdColumnName)]
    [ForeignKey(typeof(UserGroupDto))]
    public int UserGroupId { get; set; }
}
