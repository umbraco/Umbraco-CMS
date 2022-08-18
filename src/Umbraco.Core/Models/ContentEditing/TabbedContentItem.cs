using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

public abstract class TabbedContentItem<T> : ContentItemBasic<T>, ITabbedContent<T>
    where T : ContentPropertyBasic
{
    protected TabbedContentItem() => Tabs = new List<Tab<T>>();

    /// <summary>
    ///     Override the properties property to ensure we don't serialize this
    ///     and to simply return the properties based on the properties in the tabs collection
    /// </summary>
    /// <remarks>
    ///     This property cannot be set
    /// </remarks>
    [IgnoreDataMember]
    public override IEnumerable<T> Properties
    {
        get => Tabs.Where(x => x.Properties is not null).SelectMany(x => x.Properties!);
        set => throw new NotImplementedException();
    }

    /// <summary>
    ///     Defines the tabs containing display properties
    /// </summary>
    [DataMember(Name = "tabs")]
    public IEnumerable<Tab<T>> Tabs { get; set; }
}
