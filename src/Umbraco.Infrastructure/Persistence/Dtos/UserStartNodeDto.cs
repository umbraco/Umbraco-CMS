using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.UserStartNode)]
[PrimaryKey("id", AutoIncrement = true)]
[ExplicitColumns]
public class UserStartNodeDto : IEquatable<UserStartNodeDto>
{
    public enum StartNodeTypeValue
    {
        Content = 1,
        Media = 2,
    }

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
    [Index(IndexTypes.UniqueNonClustered, ForColumns = "startNodeType, startNode, userId", Name = "IX_umbracoUserStartNode_startNodeType")]
    public int StartNodeType { get; set; }

    public static bool operator ==(UserStartNodeDto left, UserStartNodeDto right) => Equals(left, right);

    public static bool operator !=(UserStartNodeDto left, UserStartNodeDto right) => !Equals(left, right);

    public bool Equals(UserStartNodeDto? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((UserStartNodeDto)obj);
    }

    public override int GetHashCode() => Id;
}
