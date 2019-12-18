using System;
using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(Constants.DatabaseSchema.Tables.KeyValue)]
    [PrimaryKey("key", AutoIncrement = false)]
    [ExplicitColumns]
    internal class KeyValueDto
    {
        [Column("key")]
        [Length(256)]
        [PrimaryKeyColumn(AutoIncrement = false, Clustered = true)]
        public string Key { get; set; }

        [Column("value")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Value { get; set; }

        [Column("updated")]
        [Constraint(Default = SystemMethods.CurrentDateTime)]
        public DateTime Updated { get; set; }
    }
}
