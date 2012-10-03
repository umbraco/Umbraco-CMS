using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoApp")]
    [PrimaryKey("appAlias", autoIncrement = false)]
    [ExplicitColumns]
    internal class AppDto
    {
        [Column("appAlias")]
        public string AppAlias { get; set; }

        [Column("appIcon")]
        public string AppIcon { get; set; }

        [Column("appName")]
        public string AppName { get; set; }

        [Column("appInitWithTreeAlias")]
        public string AppInitWithTreeAlias { get; set; }

        [Column("sortOrder")]
        public byte SortOrder { get; set; }
    }
}