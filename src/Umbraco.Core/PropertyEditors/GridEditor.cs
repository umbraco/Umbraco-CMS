using System.Runtime.Serialization;
using Umbraco.Cms.Core.Configuration.Grid;

namespace Umbraco.Cms.Core.PropertyEditors;

[DataContract]
public class GridEditor : IGridEditorConfig
{
    public GridEditor()
    {
        Config = new Dictionary<string, object>();
        Alias = string.Empty;
    }

    [DataMember(Name = "name", IsRequired = true)]
    public string? Name { get; set; }

    [DataMember(Name = "nameTemplate")]
    public string? NameTemplate { get; set; }

    [DataMember(Name = "alias", IsRequired = true)]
    public string Alias { get; set; }

    [DataMember(Name = "view", IsRequired = true)]
    public string? View { get; set; }

    [DataMember(Name = "render")]
    public string? Render { get; set; }

    [DataMember(Name = "icon", IsRequired = true)]
    public string? Icon { get; set; }

    [DataMember(Name = "config")]
    public IDictionary<string, object> Config { get; set; }

    /// <summary>
    ///     Determines whether the specified <see cref="T:System.Object" /> is equal to the current
    ///     <see cref="T:System.Object" />.
    /// </summary>
    /// <returns>
    ///     true if the specified object  is equal to the current object; otherwise, false.
    /// </returns>
    /// <param name="obj">The object to compare with the current object. </param>
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

        return Equals((GridEditor)obj);
    }

    protected bool Equals(GridEditor other) => string.Equals(Alias, other.Alias);

    /// <summary>
    ///     Serves as a hash function for a particular type.
    /// </summary>
    /// <returns>
    ///     A hash code for the current <see cref="T:System.Object" />.
    /// </returns>
    public override int GetHashCode() => Alias.GetHashCode();
}
