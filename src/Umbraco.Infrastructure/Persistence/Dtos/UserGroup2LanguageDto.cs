using System.Data;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Represents a data transfer object (DTO) that defines the association between a user group and a language in the system.
/// </summary>
[TableName(TableName)]
[PrimaryKey([UserGroupIdColumnName, LanguageIdColumnName], AutoIncrement = false)]
[ExplicitColumns]
public class UserGroup2LanguageDto
{
    public const string TableName = Cms.Core.Constants.DatabaseSchema.Tables.UserGroup2Language;

    private const string UserGroupIdColumnName = "userGroupId";
    private const string LanguageIdColumnName = "languageId";

    /// <summary>
    /// Gets or sets the unique identifier of the user group associated with this mapping.
    /// </summary>
    [Column(UserGroupIdColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_userGroup2language", OnColumns = $"{UserGroupIdColumnName}, {LanguageIdColumnName}")]
    [ForeignKey(typeof(UserGroupDto), OnDelete = Rule.Cascade)]
    public int UserGroupId { get; set; }

    /// <summary>
    /// Gets or sets the language identifier.
    /// </summary>
    [Column(LanguageIdColumnName)]
    [ForeignKey(typeof(LanguageDto), OnDelete = Rule.Cascade)]
    public int LanguageId { get; set; }
}
