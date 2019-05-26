using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(Constants.DatabaseSchema.Tables.UserGroup2ContentTemplate)]
    [ExplicitColumns]
    internal class UserGroup2ContentTemplateDto
    {
        [Column("userGroupId")]
        [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_umbracoUserGroup2ContentTemplate", OnColumns = "userGroupId, nodeId")]
        [ForeignKey(typeof(UserGroupDto))]
        public int UserGroupId { get; set; }

        [Column("nodeId")]
        [ForeignKey(typeof(NodeDto))]
        [Index(IndexTypes.NonClustered, Name = "IX_umbracoUser2ContentTemplate_nodeId")]
        public int NodeId { get; set; }
    }
}
