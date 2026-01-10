using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
internal sealed class DomainDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Domain;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    [Column("domainDefaultLanguage")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? DefaultLanguage { get; set; }

    [Column("domainRootStructureID")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [ForeignKey(typeof(NodeDto))]
    public int? RootStructureId { get; set; }

    [Column("domainName")]
    public string DomainName { get; set; } = null!;

    /// <summary>
    /// Used for a result on the query to get the associated language for a domain, if there is one.
    /// </summary>
    [ResultColumn("languageISOCode")]
    public string IsoCode { get; set; } = null!;

    [Column("sortOrder")]
    public int SortOrder { get; set; }
}
