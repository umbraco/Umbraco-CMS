using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoApp")]
    [PrimaryKey("appAlias", autoIncrement = false)]
    [ExplicitColumns]
    internal class AppDto
    {
        [Column("appAlias")]
        [PrimaryKeyColumn(AutoIncrement = false, Clustered = true)]
        [DatabaseType(DatabaseTypes.Nvarchar, Length = 50)]
        public string AppAlias { get; set; }

        [Column("appIcon")]
        [DatabaseType(DatabaseTypes.Nvarchar, Length = 255)]
        public string AppIcon { get; set; }

        [Column("appName")]
        [DatabaseType(DatabaseTypes.Nvarchar, Length = 255)]
        public string AppName { get; set; }

        [Column("appInitWithTreeAlias")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [DatabaseType(DatabaseTypes.Nvarchar, Length = 255)]
        public string AppInitWithTreeAlias { get; set; }

        [Column("sortOrder")]
        [DatabaseType(DatabaseTypes.TinyInteger)]
        [Constraint(Name = "DF_app_sortOrder", Default = "0")]
        public byte SortOrder { get; set; }
    }
}