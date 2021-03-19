using System;
using System.Collections.Generic;
using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(Constants.DatabaseSchema.Tables.PropertyTypeTab)]
    [PrimaryKey("id", AutoIncrement = true)]
    [ExplicitColumns]
    internal class PropertyTypeTabDto
    {
        [Column("id")]
        [PrimaryKeyColumn(IdentitySeed = 12)]
        public int Id { get; set; }

        [Column("contenttypeNodeId")]
        [ForeignKey(typeof(ContentTypeDto), Column = "nodeId")]
        public int ContentTypeNodeId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("sortorder")]
        public int SortOrder { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.Many, ReferenceMemberName = "propertyTypeTabId")]
        public List<PropertyTypeDto> PropertyTypeDtos { get; set; }

        [Column("uniqueID")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Constraint(Default = SystemMethods.NewGuid)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_cmsPropertyTypeTabUniqueID")]
        public Guid UniqueId { get; set; }
    }
}
