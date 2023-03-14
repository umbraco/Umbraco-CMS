using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents a content app.
/// </summary>
/// <remarks>
///     <para>Content apps are editor extensions.</para>
/// </remarks>
[DataContract(Name = "app", Namespace = "")]
public class ContentApp
{
    /// <summary>
    ///     Gets the name of the content app.
    /// </summary>
    [DataMember(Name = "name")]
    public string? Name { get; set; }

    /// <summary>
    ///     Gets the unique alias of the content app.
    /// </summary>
    /// <remarks>
    ///     <para>Must be a valid javascript identifier, ie no spaces etc.</para>
    /// </remarks>
    [DataMember(Name = "alias")]
    public string? Alias { get; set; }

    /// <summary>
    ///     Gets or sets the weight of the content app.
    /// </summary>
    /// <remarks>
    ///     <para>Content apps are ordered by weight, from left (lowest values) to right (highest values).</para>
    ///     <para>Some built-in apps have special weights: listview is -666, content is -100 and infos is +100.</para>
    ///     <para>
    ///         The default weight is 0, meaning somewhere in-between content and infos, but weight could
    ///         be used for ordering between user-level apps, or anything really.
    ///     </para>
    /// </remarks>
    [DataMember(Name = "weight")]
    public int Weight { get; set; }

    /// <summary>
    ///     Gets the icon of the content app.
    /// </summary>
    /// <remarks>
    ///     <para>Must be a valid helveticons class name (see http://hlvticons.ch/).</para>
    /// </remarks>
    [DataMember(Name = "icon")]
    public string? Icon { get; set; }

    /// <summary>
    ///     Gets the view for rendering the content app.
    /// </summary>
    [DataMember(Name = "view")]
    public string? View { get; set; }

    /// <summary>
    ///     The view model specific to this app
    /// </summary>
    [DataMember(Name = "viewModel")]
    public object? ViewModel { get; set; }

    /// <summary>
    ///     Gets a value indicating whether the app is active.
    /// </summary>
    /// <remarks>
    ///     <para>Normally reserved for Angular to deal with but in some cases this can be set on the server side.</para>
    /// </remarks>
    [DataMember(Name = "active")]
    public bool Active { get; set; }

    /// <summary>
    ///     Gets or sets the content app badge.
    /// </summary>
    [DataMember(Name = "badge")]
    public ContentAppBadge? Badge { get; set; }
}
