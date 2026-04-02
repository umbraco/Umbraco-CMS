using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a RelationType
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class RelationType : EntityBase, IRelationTypeWithIsDependency
{
    private string _alias;
    private Guid? _childObjectType;
    private bool _isBidirectional;
    private bool _isDependency;
    private string _name;
    private Guid? _parentObjectType;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelationType" /> class with alias and name.
    /// </summary>
    /// <param name="alias">The alias of the relation type.</param>
    /// <param name="name">The name of the relation type.</param>
    public RelationType(string alias, string name)
        : this(name, alias, false, null, null, false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RelationType" /> class with full configuration.
    /// </summary>
    /// <param name="name">The name of the relation type.</param>
    /// <param name="alias">The alias of the relation type.</param>
    /// <param name="isBidrectional">A value indicating whether the relation is bidirectional.</param>
    /// <param name="parentObjectType">The object type GUID of the parent.</param>
    /// <param name="childObjectType">The object type GUID of the child.</param>
    /// <param name="isDependency">A value indicating whether this relation represents a dependency.</param>
    /// <param name="key">The optional unique key for the relation type.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> or <paramref name="alias"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> or <paramref name="alias"/> is empty or whitespace.</exception>
    public RelationType(string? name, string? alias, bool isBidrectional, Guid? parentObjectType, Guid? childObjectType, bool isDependency, Guid? key = null)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(name));
        }

        if (alias == null)
        {
            throw new ArgumentNullException(nameof(alias));
        }

        if (string.IsNullOrWhiteSpace(alias))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(alias));
        }

        if (key.HasValue)
        {
            Key = key.Value;
        }

        _name = name;
        _alias = alias;
        _isBidirectional = isBidrectional;
        _isDependency = isDependency;
        _parentObjectType = parentObjectType;
        _childObjectType = childObjectType;
    }

    /// <summary>
    ///     Gets or sets the Name of the RelationType
    /// </summary>
    [DataMember]
    public string? Name
    {
        get => _name;
        set => SetPropertyValueAndDetectChanges(value, ref _name!, nameof(Name));
    }

    /// <summary>
    ///     Gets or sets the Alias of the RelationType
    /// </summary>
    [DataMember]
    public string Alias
    {
        get => _alias;
        set => SetPropertyValueAndDetectChanges(value, ref _alias!, nameof(Alias));
    }

    /// <summary>
    ///     Gets or sets a boolean indicating whether the RelationType is Bidirectional (true) or Parent to Child (false)
    /// </summary>
    [DataMember]
    public bool IsBidirectional
    {
        get => _isBidirectional;
        set => SetPropertyValueAndDetectChanges(value, ref _isBidirectional, nameof(IsBidirectional));
    }

    /// <summary>
    ///     Gets or sets the Parents object type id
    /// </summary>
    /// <remarks>Corresponds to the NodeObjectType in the umbracoNode table</remarks>
    [DataMember]
    public Guid? ParentObjectType
    {
        get => _parentObjectType;
        set => SetPropertyValueAndDetectChanges(value, ref _parentObjectType, nameof(ParentObjectType));
    }

    /// <summary>
    ///     Gets or sets the Childs object type id
    /// </summary>
    /// <remarks>Corresponds to the NodeObjectType in the umbracoNode table</remarks>
    [DataMember]
    public Guid? ChildObjectType
    {
        get => _childObjectType;
        set => SetPropertyValueAndDetectChanges(value, ref _childObjectType, nameof(ChildObjectType));
    }

    /// <summary>
    /// Gets or sets a value indicating whether the relation represents a dependency.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, the child entity depends on the parent entity.
    /// </remarks>
    public bool IsDependency
    {
        get => _isDependency;
        set => SetPropertyValueAndDetectChanges(value, ref _isDependency, nameof(IsDependency));
    }
}
