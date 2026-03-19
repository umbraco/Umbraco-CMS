using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Data transfer object representing a dictionary item in the Umbraco CMS database.
/// Used for persisting and retrieving dictionary entries, typically for localization or multilingual support.
/// </summary>
/// <remarks>public as required to be accessible from Deploy for the RepairDictionaryIdsWorkItem.</remarks>
[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
public class DictionaryDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DictionaryEntry;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNamePk;
    public const string UniqueIdColumnName = "id"; // More commonly we use `uniqueId` for `uniqueidentifer` database fields, but it's correct for this table to use "id", as that's the name the field was given for this table when it was added.

    /// <summary>
    /// Gets or sets the primary key of the dictionary entry.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int PrimaryKey { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the dictionary item.
    /// </summary>
    [Column(UniqueIdColumnName)]
    [Index(IndexTypes.UniqueNonClustered)]
    public Guid UniqueId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the parent dictionary item, or <c>null</c> if this item has no parent.
    /// </summary>
    [Column("parent")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [ForeignKey(typeof(DictionaryDto), Column = UniqueIdColumnName)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_Parent")]
    public Guid? Parent { get; set; }

    /// <summary>
    /// Gets or sets the unique key for the dictionary item.
    /// </summary>
    [Column("key")]
    [Length(450)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_key")]
    public string Key { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of language-specific text DTOs associated with this dictionary entry.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.Many, ColumnName = nameof(UniqueId), ReferenceMemberName = nameof(LanguageTextDto.UniqueId))]
    public List<LanguageTextDto> LanguageTextDtos { get; set; } = [];
}
