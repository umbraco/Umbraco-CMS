using System.Diagnostics;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a property type.
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
[DebuggerDisplay("Id: {Id}, Name: {Name}, Alias: {Alias}")]
public class PropertyType : EntityBase, IPropertyType, IEquatable<PropertyType>
{
    private readonly bool _forceValueStorageType;
    private readonly IShortStringHelper _shortStringHelper;
    private string _alias;
    private int _dataTypeId;
    private Guid _dataTypeKey;
    private string? _description;
    private bool _labelOnTop;
    private bool _mandatory;
    private string? _mandatoryMessage;
    private string _name;
    private string _propertyEditorAlias;
    private Lazy<int>? _propertyGroupId;
    private int _sortOrder;
    private string? _validationRegExp;
    private string? _validationRegExpMessage;
    private ValueStorageType _valueStorageType;
    private ContentVariation _variations;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PropertyType" /> class.
    /// </summary>
    public PropertyType(IShortStringHelper shortStringHelper, IDataType dataType)
    {
        if (dataType == null)
        {
            throw new ArgumentNullException(nameof(dataType));
        }

        _shortStringHelper = shortStringHelper;

        if (dataType.HasIdentity)
        {
            _dataTypeId = dataType.Id;
        }

        _propertyEditorAlias = dataType.EditorAlias;
        _valueStorageType = dataType.DatabaseType;
        _variations = ContentVariation.Nothing;
        _alias = string.Empty;
        _name = string.Empty;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PropertyType" /> class.
    /// </summary>
    public PropertyType(IShortStringHelper shortStringHelper, IDataType dataType, string propertyTypeAlias)
        : this(shortStringHelper, dataType) =>
        _alias = SanitizeAlias(propertyTypeAlias);

    /// <summary>
    ///     Initializes a new instance of the <see cref="PropertyType" /> class.
    /// </summary>
    public PropertyType(IShortStringHelper shortStringHelper, string propertyEditorAlias, ValueStorageType valueStorageType)
        : this(shortStringHelper, propertyEditorAlias, valueStorageType, false)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PropertyType" /> class.
    /// </summary>
    public PropertyType(IShortStringHelper shortStringHelper, string propertyEditorAlias, ValueStorageType valueStorageType, string propertyTypeAlias)
        : this(shortStringHelper, propertyEditorAlias, valueStorageType, false, propertyTypeAlias)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PropertyType" /> class.
    /// </summary>
    /// <remarks>
    ///     Set <paramref name="forceValueStorageType" /> to true to force the value storage type. Values assigned to
    ///     the property, eg from the underlying datatype, will be ignored.
    /// </remarks>
    public PropertyType(IShortStringHelper shortStringHelper, string propertyEditorAlias, ValueStorageType valueStorageType, bool forceValueStorageType, string? propertyTypeAlias = null)
    {
        _shortStringHelper = shortStringHelper;
        _propertyEditorAlias = propertyEditorAlias;
        _valueStorageType = valueStorageType;
        _forceValueStorageType = forceValueStorageType;
        _alias = propertyTypeAlias == null ? string.Empty : SanitizeAlias(propertyTypeAlias);
        _variations = ContentVariation.Nothing;
        _name = string.Empty;
    }

    /// <summary>
    ///     Gets a value indicating whether the content type owning this property type is publishing.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A publishing content type supports draft and published values for properties.
    ///         It is possible to retrieve either the draft (default) or published value of a property.
    ///         Setting the value always sets the draft value, which then needs to be published.
    ///     </para>
    ///     <para>
    ///         A non-publishing content type only supports one value for properties. Getting
    ///         the draft or published value of a property returns the same thing, and publishing
    ///         a value property has no effect.
    ///     </para>
    ///     <para>
    ///         When true, getting the property value returns the edited value by default, but
    ///         it is possible to get the published value using the appropriate 'published' method
    ///         parameter.
    ///     </para>
    ///     <para>
    ///         When false, getting the property value always return the edited value,
    ///         regardless of the 'published' method parameter.
    ///     </para>
    /// </remarks>
    public bool SupportsPublishing { get; set; }

    /// <inheritdoc />
    public bool Equals(PropertyType? other) =>
        other != null && (base.Equals(other) || (Alias?.InvariantEquals(other.Alias) ?? false));

    /// <inheritdoc />
    [DataMember]
    public string Name
    {
        get => _name;
        set => SetPropertyValueAndDetectChanges(value, ref _name!, nameof(Name));
    }

    /// <inheritdoc />
    [DataMember]
    public virtual string Alias
    {
        get => _alias;
        set => SetPropertyValueAndDetectChanges(SanitizeAlias(value), ref _alias!, nameof(Alias));
    }

    /// <inheritdoc />
    [DataMember]
    public string? Description
    {
        get => _description;
        set => SetPropertyValueAndDetectChanges(value, ref _description, nameof(Description));
    }

    /// <inheritdoc />
    [DataMember]
    public int DataTypeId
    {
        get => _dataTypeId;
        set => SetPropertyValueAndDetectChanges(value, ref _dataTypeId, nameof(DataTypeId));
    }

    [DataMember]
    public Guid DataTypeKey
    {
        get => _dataTypeKey;
        set => SetPropertyValueAndDetectChanges(value, ref _dataTypeKey, nameof(DataTypeKey));
    }

    /// <inheritdoc />
    [DataMember]
    public string PropertyEditorAlias
    {
        get => _propertyEditorAlias;
        set => SetPropertyValueAndDetectChanges(value, ref _propertyEditorAlias!, nameof(PropertyEditorAlias));
    }

    /// <inheritdoc />
    [DataMember]
    public ValueStorageType ValueStorageType
    {
        get => _valueStorageType;
        set
        {
            if (_forceValueStorageType)
            {
                return; // ignore changes
            }

            SetPropertyValueAndDetectChanges(value, ref _valueStorageType, nameof(ValueStorageType));
        }
    }

    /// <inheritdoc />
    [DataMember]
    [DoNotClone]
    public Lazy<int>? PropertyGroupId
    {
        get => _propertyGroupId;
        set => SetPropertyValueAndDetectChanges(value, ref _propertyGroupId, nameof(PropertyGroupId));
    }

    /// <inheritdoc />
    [DataMember]
    public bool Mandatory
    {
        get => _mandatory;
        set => SetPropertyValueAndDetectChanges(value, ref _mandatory, nameof(Mandatory));
    }

    /// <inheritdoc />
    [DataMember]
    public string? MandatoryMessage
    {
        get => _mandatoryMessage;
        set => SetPropertyValueAndDetectChanges(value, ref _mandatoryMessage, nameof(MandatoryMessage));
    }

    /// <inheritdoc />
    [DataMember]
    public bool LabelOnTop
    {
        get => _labelOnTop;
        set => SetPropertyValueAndDetectChanges(value, ref _labelOnTop, nameof(LabelOnTop));
    }

    /// <inheritdoc />
    [DataMember]
    public int SortOrder
    {
        get => _sortOrder;
        set => SetPropertyValueAndDetectChanges(value, ref _sortOrder, nameof(SortOrder));
    }

    /// <inheritdoc />
    [DataMember]
    public string? ValidationRegExp
    {
        get => _validationRegExp;
        set => SetPropertyValueAndDetectChanges(value, ref _validationRegExp, nameof(ValidationRegExp));
    }

    /// <summary>
    ///     Gets or sets the custom validation message used when a pattern for this PropertyType must be matched
    /// </summary>
    [DataMember]
    public string? ValidationRegExpMessage
    {
        get => _validationRegExpMessage;
        set => SetPropertyValueAndDetectChanges(value, ref _validationRegExpMessage, nameof(ValidationRegExpMessage));
    }

    /// <inheritdoc />
    public ContentVariation Variations
    {
        get => _variations;
        set => SetPropertyValueAndDetectChanges(value, ref _variations, nameof(Variations));
    }

    /// <inheritdoc />
    public bool SupportsVariation(string? culture, string? segment, bool wildcards = false) =>

        // exact validation: cannot accept a 'null' culture if the property type varies
        //  by culture, and likewise for segment
        // wildcard validation: can accept a '*' culture or segment
        Variations.ValidateVariation(culture, segment, true, wildcards, false);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // Get hash code for the Name field if it is not null.
        var baseHash = base.GetHashCode();

        // Get hash code for the Alias field.
        var hashAlias = Alias?.ToLowerInvariant().GetHashCode();

        // Calculate the hash code for the product.
        return baseHash ^ hashAlias ?? baseHash;
    }

    /// <inheritdoc />
    protected override void PerformDeepClone(object clone)
    {
        base.PerformDeepClone(clone);

        var clonedEntity = (PropertyType)clone;
        clonedEntity.ClearPropertyChangedEvents();
    }

    /// <summary>
    ///     Sanitizes a property type alias.
    /// </summary>
    private string SanitizeAlias(string value) =>

        // NOTE: WE are doing this because we don't want to do a ToSafeAlias when the alias is the special case of
        // being prefixed with Constants.PropertyEditors.InternalGenericPropertiesPrefix
        // which is used internally
        value.StartsWith(Constants.PropertyEditors.InternalGenericPropertiesPrefix)
            ? value
            : value.ToCleanString(_shortStringHelper, CleanStringType.Alias | CleanStringType.UmbracoCase);
}
