using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

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

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(Name = "PK_umbracoUserDataDto", AutoIncrement = false)]
    public Guid Key { get; set; }

    [Column(UserKeyColumnName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Index(IndexTypes.NonClustered, Name = "IX_umbracoUserDataDto_UserKey_Group_Identifier", IncludeColumns = $"{GroupColumnName},{IdentifierColumnName}")]
    [ForeignKey(typeof(UserDto), Column = UserDto.KeyColumnName)]
    public Guid UserKey { get; set; }

    [Column(GroupColumnName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public required string Group { get; set; }

    [Column(IdentifierColumnName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public required string Identifier { get; set; }

    [Column("value")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    public required string Value { get; set; }
}
