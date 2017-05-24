using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoUserStartNode")]
    [PrimaryKey("id", autoIncrement = true)]
    [ExplicitColumns]
    internal class UserStartNodeDto
    {
        [Column("id")]
        [PrimaryKeyColumn(Name = "PK_userStartNode")]
        public int Id { get; set; }

        [Column("userId")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [ForeignKey(typeof(UserDto))]
        public int UserId { get; set; }

        [Column("startNode")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [ForeignKey(typeof(NodeDto))]
        public int StartNode { get; set; }

        [Column("startNodeType")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Index(IndexTypes.UniqueNonClustered, ForColumns = "startNodeType, startNode", Name = "IX_umbracoUserStartNode_startNodeType")]
        public int StartNodeType { get; set; }

        public enum StartNodeTypeValue
        {
            Content = 1,
            Media = 2
        }
    }
}