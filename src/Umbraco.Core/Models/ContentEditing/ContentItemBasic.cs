using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     A model representing a basic content item
/// </summary>
[DataContract(Name = "content", Namespace = "")]
public class ContentItemBasic : EntityBasic
{
    [DataMember(Name = "updateDate")]
    public DateTime UpdateDate { get; set; }

    [DataMember(Name = "createDate")]
    public DateTime CreateDate { get; set; }

    /// <summary>
    ///     Boolean indicating if this item is published or not based on it's <see cref="State" />
    /// </summary>
    [DataMember(Name = "published")]
    public bool Published => State == ContentSavedState.Published || State == ContentSavedState.PublishedPendingChanges;

    /// <summary>
    ///     Determines if the content item is a draft
    /// </summary>
    [DataMember(Name = "edited")]
    public bool Edited { get; set; }

    [DataMember(Name = "owner")]
    public UserProfile? Owner { get; set; }

    [DataMember(Name = "updater")]
    public UserProfile? Updater { get; set; }

    public int ContentTypeId { get; set; }

    [DataMember(Name = "contentTypeAlias", IsRequired = true)]
    [Required(AllowEmptyStrings = false)]
    public string ContentTypeAlias { get; set; } = null!;

    [DataMember(Name = "sortOrder")]
    public int SortOrder { get; set; }

    /// <summary>
    ///     The saved/published state of an item
    /// </summary>
    /// <remarks>
    ///     This is nullable since it's only relevant for content (non-content like media + members will be null)
    /// </remarks>
    [DataMember(Name = "state")]
    public ContentSavedState? State { get; set; }

    [DataMember(Name = "variesByCulture")]
    public bool VariesByCulture { get; set; }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj is ContentItemBasic other && Equals(other);
    }

    protected bool Equals(ContentItemBasic other) => Id == other.Id;

    public override int GetHashCode()
    {
        if (Id is not null)
        {
            return Id.GetHashCode();
        }

        return base.GetHashCode();
    }
}

/// <summary>
///     A model representing a basic content item with properties
/// </summary>
[DataContract(Name = "content", Namespace = "")]
public class ContentItemBasic<T> : ContentItemBasic, IContentProperties<T>
    where T : ContentPropertyBasic
{
    private IEnumerable<T> _properties;

    public ContentItemBasic() =>

        // ensure its not null
        _properties = Enumerable.Empty<T>();

    [DataMember(Name = "properties")]
    public virtual IEnumerable<T> Properties
    {
        get => _properties;
        set => _properties = value;
    }
}
