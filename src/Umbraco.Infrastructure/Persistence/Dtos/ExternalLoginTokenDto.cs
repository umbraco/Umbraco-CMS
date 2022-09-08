using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[ExplicitColumns]
[PrimaryKey("Id")]
internal class ExternalLoginTokenDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ExternalLoginToken;

    [Column("id")]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    [Column("externalLoginId")]
    [ForeignKey(typeof(ExternalLoginDto), Column = "id")]
    public int ExternalLoginId { get; set; }

    [Column("name")]
    [Length(255)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Index(IndexTypes.UniqueNonClustered, ForColumns = "externalLoginId,name", Name = "IX_" + TableName + "_Name")]
    public string Name { get; set; } = null!;

    [Column("value")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Value { get; set; } = null!;

    [Column("createDate")]
    [Constraint(Default = SystemMethods.CurrentDateTime)]
    public DateTime CreateDate { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ColumnName = "ExternalLoginId")]
    public ExternalLoginDto ExternalLoginDto { get; set; } = null!;
}
