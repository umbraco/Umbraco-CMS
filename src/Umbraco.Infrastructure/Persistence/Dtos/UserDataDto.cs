using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey("key", AutoIncrement = false)]
[ExplicitColumns]
public class UserDataDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.UserData;

    [Column("key")]
    [PrimaryKeyColumn(Name = "PK_umbracoUserDataDto", AutoIncrement = false)]
    public Guid Key { get; set; }

    [Column("userKey")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Index(IndexTypes.NonClustered, Name = "IX_umbracoUserDataDto_UserKey_Group_Identifier", IncludeColumns = "group,identifier")]
    [ForeignKey(typeof(UserDto), Column = "key")]
    public Guid UserKey { get; set; }

    [Column("group")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public required string Group { get; set; }

    [Column("identifier")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public required string Identifier { get; set; }

    [Column("value")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    public required string Value { get; set; }
}
