﻿using System;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsPropertyTypeGroup")]
    [PrimaryKey("id", autoIncrement = true)]
    [ExplicitColumns]
    internal class PropertyTypeGroupReadOnlyDto
    {
        [Column("PropertyTypeGroupId")]
        public int? Id { get; set; }

        [Column("PropertyGroupName")]
        public string Text { get; set; }

        [Column("PropertyGroupSortOrder")]
        public int SortOrder { get; set; }

        [Column("contenttypeNodeId")]
        public int ContentTypeNodeId { get; set; }

        [Column("PropertyGroupUniqueID")]
        public Guid UniqueId { get; set; }
    }
}