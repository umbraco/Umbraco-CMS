using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Represents a data transfer object (DTO) containing user-related data for persistence operations in Umbraco CMS.
/// </summary>
[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
public class UserDataDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.UserData;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameKey;

    private const string UserKeyColumnName = "userKey";
    private const string GroupColumnName = "group";
    private const string IdentifierColumnName = "identifier";

    /// <summary>
    /// Gets or sets the unique identifier for this user data record.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(Name = "PK_umbracoUserDataDto", AutoIncrement = false)]
    public Guid Key { get; set; }

    /// <summary>
    /// Gets or sets the unique key that identifies the user.
    /// </summary>
    [Column(UserKeyColumnName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Index(IndexTypes.NonClustered, Name = "IX_umbracoUserDataDto_UserKey_Group_Identifier", IncludeColumns = $"{GroupColumnName},{IdentifierColumnName}")]
    [ForeignKey(typeof(UserDto), Column = UserDto.KeyColumnName)]
    public Guid UserKey { get; set; }

    /// <summary>
    /// Gets or sets the name of the group to which the user belongs.
    /// </summary>
    [Column(GroupColumnName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public required string Group { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier associated with the user data record.
    /// </summary>
    [Column(IdentifierColumnName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public required string Identifier { get; set; }

    /// <summary>
    /// Gets or sets the string value stored for the user data entry.
    /// </summary>
    [Column("value")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    public required string Value { get; set; }
}
