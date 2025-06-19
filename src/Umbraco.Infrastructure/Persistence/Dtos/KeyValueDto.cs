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
    public const string TableName = Constants.DatabaseSchema.Tables.KeyValue;

    [Column("key")]
    [Length(256)]
    [PrimaryKeyColumn(AutoIncrement = false, Clustered = true)]
    public string Key { get; set; } = null!;

    [Column("value")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Value { get; set; }

    [Column("updated", ForceToUtc = false)]
    [Constraint(Default = SystemMethods.CurrentDateTime)]
    public DateTime UpdateDate { get; set; }

    //NOTE that changes to this file needs to be backward compatible. Otherwise our upgrader cannot work, as it uses this to read from the db
}
