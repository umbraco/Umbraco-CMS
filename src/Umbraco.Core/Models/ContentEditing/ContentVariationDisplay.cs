using System.ComponentModel;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents the variant info for a content item
/// </summary>
[DataContract(Name = "contentVariant", Namespace = "")]
public class ContentVariantDisplay : ITabbedContent<ContentPropertyDisplay>, IContentProperties<ContentPropertyDisplay>, INotificationModel
{
    public ContentVariantDisplay()
    {
        Tabs = new List<Tab<ContentPropertyDisplay>>();
        Notifications = new List<BackOfficeNotification>();
        AllowedActions = Enumerable.Empty<string>();
    }

    [DataMember(Name = "allowedActions", IsRequired = true)]
    public IEnumerable<string> AllowedActions { get; set; }

    [DataMember(Name = "name", IsRequired = true)]
    public string? Name { get; set; }

    [DataMember(Name = "displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    ///     The language/culture assigned to this content variation
    /// </summary>
    /// <remarks>
    ///     If this is null it means this content variant is an invariant culture
    /// </remarks>
    [DataMember(Name = "language")]
    public Language? Language { get; set; }

    [DataMember(Name = "segment")]
    public string? Segment { get; set; }

    [DataMember(Name = "state")]
    public ContentSavedState State { get; set; }

    [DataMember(Name = "updateDate")]
    public DateTime UpdateDate { get; set; }

    [DataMember(Name = "createDate")]
    public DateTime CreateDate { get; set; }

    [DataMember(Name = "publishDate")]
    public DateTime? PublishDate { get; set; }

    /// <summary>
    ///     Internal property used for tests to get all properties from all tabs
    /// </summary>
    [IgnoreDataMember]
    IEnumerable<ContentPropertyDisplay> IContentProperties<ContentPropertyDisplay>.Properties =>
        Tabs.Where(x => x.Properties is not null).SelectMany(x => x.Properties!);

    /// <summary>
    ///     This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
    /// </summary>
    /// <remarks>
    ///     The notifications assigned to a variant are currently only used to show custom messages in the save/publish
    ///     dialogs.
    /// </remarks>
    [DataMember(Name = "notifications")]
    [ReadOnly(true)]
    public List<BackOfficeNotification> Notifications { get; private set; }

    /// <summary>
    ///     Defines the tabs containing display properties
    /// </summary>
    [DataMember(Name = "tabs")]
    public IEnumerable<Tab<ContentPropertyDisplay>> Tabs { get; set; }
}

public class ContentVariantScheduleDisplay : ContentVariantDisplay
{
    [DataMember(Name = "releaseDate")]
    public DateTime? ReleaseDate { get; set; }

    [DataMember(Name = "expireDate")]
    public DateTime? ExpireDate { get; set; }
}
