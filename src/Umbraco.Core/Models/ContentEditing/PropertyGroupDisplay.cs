using System.ComponentModel;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "propertyGroup", Namespace = "")]
public class PropertyGroupDisplay<TPropertyTypeDisplay> : PropertyGroupBasic<TPropertyTypeDisplay>
    where TPropertyTypeDisplay : PropertyTypeDisplay
{
    public PropertyGroupDisplay()
    {
        Properties = new List<TPropertyTypeDisplay>();
        ParentTabContentTypeNames = new List<string>();
        ParentTabContentTypes = new List<int>();
    }

    /// <summary>
    ///     Gets the context content type.
    /// </summary>
    [DataMember(Name = "contentTypeId")]
    [ReadOnly(true)]
    public int ContentTypeId { get; set; }

    /// <summary>
    ///     Gets the identifiers of the content types that define this group.
    /// </summary>
    [DataMember(Name = "parentTabContentTypes")]
    [ReadOnly(true)]
    public IEnumerable<int> ParentTabContentTypes { get; set; }

    /// <summary>
    ///     Gets the name of the content types that define this group.
    /// </summary>
    [DataMember(Name = "parentTabContentTypeNames")]
    [ReadOnly(true)]
    public IEnumerable<string?> ParentTabContentTypeNames { get; set; }
}
