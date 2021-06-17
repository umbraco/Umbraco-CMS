using System;
using System.Collections.Generic;
using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(TableName)]
    [PrimaryKey("id", AutoIncrement = true)]
    [ExplicitColumns]
    internal class PropertyTypeGroupDto
    {
        public const string TableName = Constants.DatabaseSchema.Tables.PropertyTypeGroup;

        [Column("id")]
        [PrimaryKeyColumn(IdentitySeed = 12)]
        public int Id { get; set; }

        [Column("uniqueID")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Constraint(Default = SystemMethods.NewGuid)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_cmsPropertyTypeGroupUniqueID")]
        public Guid UniqueId { get; set; }

        [Column("parentKey")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [ForeignKey(typeof(PropertyTypeGroupDto), Column = "uniqueID", Name = "FK_" + TableName + "_parentKey")]
        public Guid? ParentKey { get; set; }

        [Column("level")]
        [Constraint(Default = 1)] // TODO We default to 1 (property group) for backwards compatibility, but should use zero/no default at some point.
        public short Level { get; set; } = 1;

        [Column("contenttypeNodeId")]
        [ForeignKey(typeof(ContentTypeDto), Column = "nodeId")]
        public int ContentTypeNodeId { get; set; }

        [Column("icon")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Icon { get; set; }

        [Column("text")]
        public string Text { get; set; }

        [Column("sortorder")]
        public int SortOrder { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.Many, ReferenceMemberName = "PropertyTypeGroupId")]
        public List<PropertyTypeDto> PropertyTypeDtos { get; set; }
    }
}
