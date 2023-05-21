using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a Macro Property
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class MacroProperty : BeingDirtyBase, IMacroProperty
{
    private string _alias;
    private string _editorAlias;
    private int _id;

    private Guid _key;
    private string? _name;
    private int _sortOrder;

    public MacroProperty()
    {
        _editorAlias = string.Empty;
        _alias = string.Empty;
        _key = Guid.NewGuid();
    }

    /// <summary>
    ///     Ctor for creating a new property
    /// </summary>
    /// <param name="alias"></param>
    /// <param name="name"></param>
    /// <param name="sortOrder"></param>
    /// <param name="editorAlias"></param>
    public MacroProperty(string alias, string? name, int sortOrder, string editorAlias)
    {
        _alias = alias;
        _name = name;
        _sortOrder = sortOrder;
        _key = Guid.NewGuid();
        _editorAlias = editorAlias;
    }

    /// <summary>
    ///     Ctor for creating an existing property
    /// </summary>
    /// <param name="id"></param>
    /// <param name="key"></param>
    /// <param name="alias"></param>
    /// <param name="name"></param>
    /// <param name="sortOrder"></param>
    /// <param name="editorAlias"></param>
    public MacroProperty(int id, Guid key, string alias, string? name, int sortOrder, string editorAlias)
    {
        _id = id;
        _alias = alias;
        _name = name;
        _sortOrder = sortOrder;
        _key = key;
        _editorAlias = editorAlias;
    }

    /// <summary>
    ///     Gets or sets the Key of the Property
    /// </summary>
    [DataMember]
    public Guid Key
    {
        get => _key;
        set => SetPropertyValueAndDetectChanges(value, ref _key, nameof(Key));
    }

    /// <summary>
    ///     Gets or sets the Alias of the Property
    /// </summary>
    [DataMember]
    public int Id
    {
        get => _id;
        set => SetPropertyValueAndDetectChanges(value, ref _id, nameof(Id));
    }

    /// <summary>
    ///     Gets or sets the Alias of the Property
    /// </summary>
    [DataMember]
    public string Alias
    {
        get => _alias;
        set => SetPropertyValueAndDetectChanges(value, ref _alias!, nameof(Alias));
    }

    /// <summary>
    ///     Gets or sets the Name of the Property
    /// </summary>
    [DataMember]
    public string? Name
    {
        get => _name;
        set => SetPropertyValueAndDetectChanges(value, ref _name, nameof(Name));
    }

    /// <summary>
    ///     Gets or sets the Sort Order of the Property
    /// </summary>
    [DataMember]
    public int SortOrder
    {
        get => _sortOrder;
        set => SetPropertyValueAndDetectChanges(value, ref _sortOrder, nameof(SortOrder));
    }

    /// <summary>
    ///     Gets or sets the Type for this Property
    /// </summary>
    /// <remarks>
    ///     The MacroPropertyTypes acts as a plugin for Macros.
    ///     All types was previously contained in the database, but has been ported to code.
    /// </remarks>
    [DataMember]
    public string EditorAlias
    {
        get => _editorAlias;
        set => SetPropertyValueAndDetectChanges(value, ref _editorAlias!, nameof(EditorAlias));
    }

    public object DeepClone()
    {
        // Memberwise clone on MacroProperty will work since it doesn't have any deep elements
        // for any sub class this will work for standard properties as well that aren't complex object's themselves.
        var clone = (MacroProperty)MemberwiseClone();

        // Automatically deep clone ref properties that are IDeepCloneable
        DeepCloneHelper.DeepCloneRefProperties(this, clone);
        clone.ResetDirtyProperties(false);
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

        return Equals((MacroProperty)obj);
    }

    protected bool Equals(MacroProperty other) => string.Equals(_alias, other._alias) && _id == other._id;

    public override int GetHashCode()
    {
        unchecked
        {
            return ((_alias != null ? _alias.GetHashCode() : 0) * 397) ^ _id;
        }
    }
}
