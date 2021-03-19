using System;
using NPoco;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(Constants.DatabaseSchema.Tables.PropertyTypeTab)]
    [PrimaryKey("id", AutoIncrement = true)]
    [ExplicitColumns]
    internal class PropertyTypeTabReadOnlyDto
    {
        [Column("PropertyTypeTabId")]
        public int? Id { get; set; }

        [Column("PropertyTabName")]
        public string Name { get; set; }

        [Column("PropertyTabSortOrder")]
        public int SortOrder { get; set; }

        [Column("contenttypeNodeId")]
        public int ContentTypeNodeId { get; set; }

        [Column("PropertyTabUniqueID")]
        public Guid UniqueId { get; set; }
    }
}
