using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents a content app badge
/// </summary>
[DataContract(Name = "badge", Namespace = "")]
public class ContentAppBadge
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentAppBadge" /> class.
    /// </summary>
    public ContentAppBadge() => Type = ContentAppBadgeType.Default;

    /// <summary>
    ///     Gets or sets the number displayed in the badge
    /// </summary>
    [DataMember(Name = "count")]
    public int Count { get; set; }

    /// <summary>
    ///     Gets or sets the type of badge to display
    /// </summary>
    /// <remarks>
    ///     <para>This controls the background color of the badge.</para>
    ///     <para>Warning will display a dark yellow badge</para>
    ///     <para>Alert will display a red badge</para>
    ///     <para>Default will display a turquoise badge</para>
    /// </remarks>
    [DataMember(Name = "type")]
    public ContentAppBadgeType Type { get; set; }
}
