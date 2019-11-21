using System;
using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(TableName)]
    [PrimaryKey("id", AutoIncrement = true)]
    [ExplicitColumns]
    internal class UserStartNodeDto : IEquatable<UserStartNodeDto>
    {
        public const string TableName = Constants.DatabaseSchema.Tables.UserStartNode;

        [Column("id")]
        [PrimaryKeyColumn(Name = "PK_userStartNode")]
        public int Id { get; set; }

        [Column("userId")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [ForeignKey(typeof(UserDto))]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_UserId")]
        public int UserId { get; set; }

        [Column("startNode")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [ForeignKey(typeof(NodeDto))]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_StartNode")]
        public int StartNode { get; set; }

        [Column("startNodeType")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Index(IndexTypes.UniqueNonClustered, ForColumns = "startNodeType, startNode, userId", Name = "IX_umbracoUserStartNode_startNodeType")]
        public int StartNodeType { get; set; }

        public enum StartNodeTypeValue
        {
            Content = 1,
            Media = 2
        }

        public bool Equals(UserStartNodeDto other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((UserStartNodeDto) obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public static bool operator ==(UserStartNodeDto left, UserStartNodeDto right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(UserStartNodeDto left, UserStartNodeDto right)
        {
            return !Equals(left, right);
        }
    }
}
