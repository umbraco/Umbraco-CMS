using System.ComponentModel;
using System.Globalization;

namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Provides a type converter for <see cref="IPublishedContent"/> instances.
/// </summary>
/// <remarks>
///     This converter allows converting <see cref="IPublishedContent"/> to integer (Id).
/// </remarks>
internal sealed class PublishedContentTypeConverter : TypeConverter
{
    private static readonly Type[] ConvertingTypes = { typeof(int) };

    /// <inheritdoc />
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
        ConvertingTypes.Any(x => x.IsAssignableFrom(destinationType))
        || (destinationType is not null && CanConvertFrom(context, destinationType));

    /// <inheritdoc />
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (!(value is IPublishedContent publishedContent))
        {
            return null;
        }

        return typeof(int).IsAssignableFrom(destinationType)
            ? publishedContent.Id
            : base.ConvertTo(context, culture, value, destinationType);
    }
}
