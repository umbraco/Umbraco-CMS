using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoAppTree")]
    [PrimaryKey("appAlias", autoIncrement = false)]
    [ExplicitColumns]
    internal class AppTreeDto
    {
        [Column("treeSilent")]
        public bool Silent { get; set; }

        [Column("treeInitialize")]
        public bool Initialize { get; set; }

        [Column("treeSortOrder")]
        public byte SortOrder { get; set; }

        [Column("appAlias")]
        public string AppAlias { get; set; }

        [Column("treeAlias")]
        public string Alias { get; set; }

        [Column("treeTitle")]
        public string Title { get; set; }

        [Column("treeIconClosed")]
        public string IconClosed { get; set; }

        [Column("treeIconOpen")]
        public string IconOpen { get; set; }

        [Column("treeHandlerAssembly")]
        public string HandlerAssembly { get; set; }

        [Column("treeHandlerType")]
        public string HandlerType { get; set; }

        [Column("action")]
        public string Action { get; set; }
    }
}