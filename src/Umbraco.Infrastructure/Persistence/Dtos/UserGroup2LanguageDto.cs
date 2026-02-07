using System.Data;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey([UserGroupIdColumnName, LanguageIdColumnName], AutoIncrement = false)]
[ExplicitColumns]
public class UserGroup2LanguageDto
{
    public const string TableName = Cms.Core.Constants.DatabaseSchema.Tables.UserGroup2Language;
    public const string PrimaryKeyColumnName = "PK_userGroup2language";

    internal const string ReferenceMemberName = "UserGroupId"; // should be UserGroupIdColumnName, but for database compatibility we keep it like this

    private const string UserGroupIdColumnName = "userGroupId";
    private const string LanguageIdColumnName = "languageId";

    [Column(UserGroupIdColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = PrimaryKeyColumnName, OnColumns = $"{UserGroupIdColumnName}, {LanguageIdColumnName}")]
    [ForeignKey(typeof(UserGroupDto), OnDelete = Rule.Cascade)]
    public int UserGroupId { get; set; }

    [Column(LanguageIdColumnName)]
    [ForeignKey(typeof(LanguageDto), OnDelete = Rule.Cascade)]
    public int LanguageId { get; set; }
}
