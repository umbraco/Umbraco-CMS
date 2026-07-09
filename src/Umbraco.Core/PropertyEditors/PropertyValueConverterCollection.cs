using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a collection of <see cref="IPropertyValueConverter"/> instances.
/// </summary>
public class PropertyValueConverterCollection : BuilderCollectionBase<IPropertyValueConverter>
{
    private readonly Lock _locker = new();
    private Dictionary<IPropertyValueConverter, Type[]>? _defaultConverters;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyValueConverterCollection"/> class.
    /// </summary>
    /// <param name="items">A factory function that returns the collection items.</param>
    public PropertyValueConverterCollection(Func<IEnumerable<IPropertyValueConverter>> items)
        : base(items)
    {
    }

    private Dictionary<IPropertyValueConverter, Type[]> DefaultConverters
    {
        get
        {
            lock (_locker)
            {
                if (_defaultConverters != null)
                {
                    return _defaultConverters;
                }

                _defaultConverters = new Dictionary<IPropertyValueConverter, Type[]>();

                foreach (IPropertyValueConverter converter in this)
                {
                    DefaultPropertyValueConverterAttribute? attr = converter.GetType().GetCustomAttribute<DefaultPropertyValueConverterAttribute>(false);
                    if (attr != null)
                    {
                        _defaultConverters[converter] = attr.DefaultConvertersToShadow;
                    }
                }

                return _defaultConverters;
            }
        }
    }

    /// <summary>
    /// Determines whether the specified converter is a default converter.
    /// </summary>
    /// <param name="converter">The converter to check.</param>
    /// <returns><c>true</c> if the converter is a default converter; otherwise, <c>false</c>.</returns>
    internal bool IsDefault(IPropertyValueConverter converter)
        => DefaultConverters.ContainsKey(converter);

    /// <summary>
    /// Determines whether one converter shadows another.
    /// </summary>
    /// <param name="shadowing">The converter that may be shadowing.</param>
    /// <param name="shadowed">The converter that may be shadowed.</param>
    /// <returns><c>true</c> if the shadowing converter shadows the shadowed converter; otherwise, <c>false</c>.</returns>
    internal bool Shadows(IPropertyValueConverter shadowing, IPropertyValueConverter shadowed)
    {
        Type shadowedType = shadowed.GetType();

        // any value converter built specifically to convert purely value type bound properties can always be shadowed
        if (shadowedType.GetCustomAttribute<DefaultValueTypePropertyValueConverterAttribute>(false) is not null)
        {
            return true;
        }

        return DefaultConverters.TryGetValue(shadowing, out Type[]? types) && types.Contains(shadowedType);
    }
}
