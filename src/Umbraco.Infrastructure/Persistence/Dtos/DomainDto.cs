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

    /// <summary>
    /// Gets or sets the unique identifier for the domain.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the default language identifier for the domain.
    /// </summary>
    [Column("domainDefaultLanguage")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? DefaultLanguage { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the root structure node associated with the domain, or null if not set.
    /// </summary>
    [Column("domainRootStructureID")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [ForeignKey(typeof(NodeDto))]
    public int? RootStructureId { get; set; }

    /// <summary>
    /// Gets or sets the assigned domain name for this domain entity.
    /// </summary>
    [Column("domainName")]
    public string DomainName { get; set; } = null!;

    /// <summary>
    /// Used for a result on the query to get the associated language for a domain, if there is one.
    /// </summary>
    [ResultColumn("languageISOCode")]
    public string IsoCode { get; set; } = null!;

    /// <summary>
    /// Gets or sets the sort order of the domain.
    /// </summary>
    [Column("sortOrder")]
    public int SortOrder { get; set; }
}
