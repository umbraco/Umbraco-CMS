using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;


[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
internal sealed class KeyValueDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.KeyValue;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameKey;

    [Column(PrimaryKeyColumnName)]
    [Length(256)]
    [PrimaryKeyColumn(AutoIncrement = false, Clustered = true)]
    public string Key { get; set; } = null!;

    [Column("value")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Value { get; set; }

    [Column("updated")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime UpdateDate { get; set; }

    //NOTE that changes to this file needs to be backward compatible. Otherwise our upgrader cannot work, as it uses this to read from the db
}
