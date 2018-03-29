﻿using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoUserGroup2NodePermission")]
    [ExplicitColumns]
    internal class UserGroup2NodePermissionDto
    {
        [Column("userGroupId")]
        [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_umbracoUserGroup2NodePermission", OnColumns = "userGroupId, nodeId, permission")]
        [ForeignKey(typeof(UserGroupDto))]
        public int UserGroupId { get; set; }

        [Column("nodeId")]
        [ForeignKey(typeof(NodeDto))]
        [Index(IndexTypes.NonClustered, Name = "IX_umbracoUser2NodePermission_nodeId")]
        public int NodeId { get; set; }

        [Column("permission")]
        public string Permission { get; set; }
    }
}