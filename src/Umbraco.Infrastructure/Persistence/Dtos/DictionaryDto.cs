using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
public class DictionaryDto // public as required to be accessible from Deploy for the RepairDictionaryIdsWorkItem.
{
    public const string TableName = Constants.DatabaseSchema.Tables.DictionaryEntry;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNamePk;
    public const string UniqueIdColumnName = "id"; // More commonly we use `uniqueId` for `uniqueidentifer` database fields, but it's correct for this table to use "id", as that's the name the field was given for this table when it was added.

    internal const string ReferenceColumnName = "UniqueId"; // should be DataTypeDto.PrimaryKeyColumnName, but for database compatibility we keep it like this

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int PrimaryKey { get; set; }

    [Column(UniqueIdColumnName)]
    [Index(IndexTypes.UniqueNonClustered)]
    public Guid UniqueId { get; set; }

    [Column("parent")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [ForeignKey(typeof(DictionaryDto), Column = "id")]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_Parent")]
    public Guid? Parent { get; set; }

    [Column("key")]
    [Length(450)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_key")]
    public string Key { get; set; } = null!;

    [ResultColumn]
    [Reference(ReferenceType.Many, ColumnName = ReferenceColumnName, ReferenceMemberName = LanguageTextDto.ReferenceMemberName)]
    public List<LanguageTextDto> LanguageTextDtos { get; set; } = [];
}
