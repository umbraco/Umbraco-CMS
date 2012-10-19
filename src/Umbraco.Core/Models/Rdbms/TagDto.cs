using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsTags")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class TagDto
    {
        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Column("tag")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [DatabaseType(SpecialDbTypes.NVARCHAR, Length = 200)]//NOTE Is set to [varchar] (200) in Sql Server script
        public string Tag { get; set; }

        [Column("ParentId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? ParentId { get; set; }

        [Column("group")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [DatabaseType(SpecialDbTypes.NVARCHAR, Length = 100)]//NOTE Is set to [varchar] (100) in Sql Server script
        public string Group { get; set; }
    }
}