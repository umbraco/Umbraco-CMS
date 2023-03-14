using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.Consent)]
[PrimaryKey("id")]
[ExplicitColumns]
public class ConsentDto
{
    [Column("id")]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    [Column("current")]
    public bool Current { get; set; }

    [Column("source")]
    [Length(512)]
    public string? Source { get; set; }

    [Column("context")]
    [Length(128)]
    public string? Context { get; set; }

    [Column("action")]
    [Length(512)]
    public string? Action { get; set; }

    [Column("createDate")]
    [Constraint(Default = SystemMethods.CurrentDateTime)]
    public DateTime CreateDate { get; set; }

    [Column("state")]
    public int State { get; set; }

    [Column("comment")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Comment { get; set; }
}
