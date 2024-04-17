using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

public class PropertyValueConverterCollection : BuilderCollectionBase<IPropertyValueConverter>
{
    private readonly object _locker = new();
    private Dictionary<IPropertyValueConverter, Type[]>? _defaultConverters;

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

    internal bool IsDefault(IPropertyValueConverter converter)
        => DefaultConverters.ContainsKey(converter);

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
