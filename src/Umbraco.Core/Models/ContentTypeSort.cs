using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a POCO for setting sort order on a ContentType reference
/// </summary>
public class ContentTypeSort : IValueObject, IDeepCloneable
{
    // this parameterless ctor should never be used BUT is required by AutoMapper in EntityMapperProfile
    public ContentTypeSort()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
    /// </summary>
    public ContentTypeSort(Guid key, int sortOrder, string alias)
    {
        SortOrder = sortOrder;
        Alias = alias;
        Key = key;
    }

    /// <summary>
    ///     Gets or sets the Sort Order of the ContentType
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    ///     Gets or sets the Alias of the ContentType
    /// </summary>
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the unique Key of the ContentType
    /// </summary>
    public Guid Key { get; set; }

    public object DeepClone()
    {
        var clone = (ContentTypeSort)MemberwiseClone();
        return clone;
    }

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

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((ContentTypeSort)obj);
    }

    protected bool Equals(ContentTypeSort other) =>
        Key.Equals(other.Key) && string.Equals(Alias, other.Alias);

    public override int GetHashCode()
    {
        unchecked
        {
            // The hash code will just be the alias if one is assigned, otherwise it will be the hash code of the Id.
            // In some cases the alias can be null of the non lazy ctor is used, in that case, the lazy Id will already have a value created.
            return Alias != null ? Alias.GetHashCode() : Key.GetHashCode() * 397;
        }
    }
}
