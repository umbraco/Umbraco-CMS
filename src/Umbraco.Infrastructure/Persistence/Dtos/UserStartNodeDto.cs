using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Represents a data transfer object (DTO) that defines the start node configuration for a user within the Umbraco CMS persistence layer.
/// </summary>
[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = true)]
[ExplicitColumns]
public class UserStartNodeDto : IEquatable<UserStartNodeDto>
{
    public const string TableName = Constants.DatabaseSchema.Tables.UserStartNode;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    private const string UserIdColumnName = "userId";
    private const string StartNodeColumnName = "startNode";
    private const string StartNodeTypeColumnName = "startNodeType";

    /// <summary>
    /// Gets or sets the value that indicates the type of the start node, typically as an integer or enumeration.
    /// </summary>
    public enum StartNodeTypeValue
    {
        Content = 1,
        Media = 2,
    }

    /// <summary>
    /// Gets or sets the unique identifier of the user start node.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(Name = "PK_userStartNode")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user associated with this start node.
    /// </summary>
    [Column(UserIdColumnName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [ForeignKey(typeof(UserDto))]
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the start node identifier for the user.
    /// </summary>
    [Column(StartNodeColumnName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [ForeignKey(typeof(NodeDto))]
    public int StartNode { get; set; }

    /// <summary>
    /// Gets or sets the type of the start node, indicating whether the node is for content, media, or another type.
    /// The value is typically an integer representing the node type (e.g., 0 for content, 1 for media).
    /// </summary>
    [Column(StartNodeTypeColumnName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Index(IndexTypes.UniqueNonClustered, ForColumns = $"{StartNodeTypeColumnName}, {StartNodeColumnName}, {UserIdColumnName}", Name = "IX_umbracoUserStartNode_startNodeType")]
    public int StartNodeType { get; set; }

    public static bool operator ==(UserStartNodeDto left, UserStartNodeDto right) => Equals(left, right);

    public static bool operator !=(UserStartNodeDto left, UserStartNodeDto right) => !Equals(left, right);

    /// <summary>
    /// Determines whether this instance and a specified <see cref="UserStartNodeDto"/> have the same identifier.
    /// </summary>
    /// <param name="other">The <see cref="UserStartNodeDto"/> to compare with this instance.</param>
    /// <returns><c>true</c> if the specified <see cref="UserStartNodeDto"/> has the same <c>Id</c> as this instance; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="UserStartNodeDto"/> instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified object is equal to the current instance; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Returns a hash code for this instance, based on the <see cref="Id"/> property.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode() => Id;
}
