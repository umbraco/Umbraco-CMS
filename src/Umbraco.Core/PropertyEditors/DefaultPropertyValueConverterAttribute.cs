namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Indicates that this is a default property value converter (shipped with Umbraco)
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class DefaultPropertyValueConverterAttribute : Attribute
{
    public DefaultPropertyValueConverterAttribute() => DefaultConvertersToShadow = Array.Empty<Type>();

    public DefaultPropertyValueConverterAttribute(params Type[] convertersToShadow) =>
        DefaultConvertersToShadow = convertersToShadow;

    /// <summary>
    ///     A DefaultPropertyValueConverter can 'shadow' other default property value converters so that
    ///     a DefaultPropertyValueConverter can be more specific than another one.
    /// </summary>
    /// <remarks>
    ///     An example where this is useful is that both the RelatedLiksEditorValueConverter and the JsonValueConverter
    ///     will be returned as value converters for the Related Links Property editor, however the JsonValueConverter
    ///     is a very generic converter and the RelatedLiksEditorValueConverter is more specific than it, so the
    ///     RelatedLiksEditorValueConverter
    ///     can specify that it 'shadows' the JsonValueConverter.
    /// </remarks>
    public Type[] DefaultConvertersToShadow { get; }
}
