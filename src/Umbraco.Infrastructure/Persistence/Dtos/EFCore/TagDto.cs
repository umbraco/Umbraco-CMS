using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(TagDtoConfiguration))]
public class TagDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Tag;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string GroupColumnName = "group";
    public const string LanguageIdColumnName = "languageId";
    public const string TextColumnName = "tag";

    /// <summary>Gets or sets the unique identifier for the tag.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the name of the group to which the tag belongs.</summary>
    public string Group { get; set; } = null!;

    /// <summary>Gets or sets the language identifier associated with the tag.</summary>
    public int? LanguageId { get; set; }

    /// <summary>Gets or sets the text value of the tag.</summary>
    public string Text { get; set; } = null!;
}
