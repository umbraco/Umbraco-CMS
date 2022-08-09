using System.Data;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Cms.Core.Constants.DatabaseSchema.Tables.UserGroup2Language)]
[ExplicitColumns]
public class UserGroup2LanguageDto
{
    public const string TableName = Cms.Core.Constants.DatabaseSchema.Tables.UserGroup2Language;

    [Column("userGroupId")]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_userGroup2language", OnColumns = "userGroupId, languageId")]
    [ForeignKey(typeof(UserGroupDto), OnDelete = Rule.Cascade)]
    public int UserGroupId { get; set; }

    [Column("languageId")]
    [ForeignKey(typeof(LanguageDto), OnDelete = Rule.Cascade)]
    public int LanguageId { get; set; }
}
