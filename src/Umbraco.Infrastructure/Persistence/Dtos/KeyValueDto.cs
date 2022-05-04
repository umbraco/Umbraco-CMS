using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.KeyValue)]
[PrimaryKey("key", AutoIncrement = false)]
[ExplicitColumns]
internal class KeyValueDto
{
    [Column("key")]
    [Length(256)]
    [PrimaryKeyColumn(AutoIncrement = false, Clustered = true)]
    public string Key { get; set; } = null!;

    [Column("value")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Value { get; set; }

    [Column("updated")]
    [Constraint(Default = SystemMethods.CurrentDateTime)]
    public DateTime UpdateDate { get; set; }
}
