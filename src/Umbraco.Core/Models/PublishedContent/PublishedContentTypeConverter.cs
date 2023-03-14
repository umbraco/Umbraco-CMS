using System.ComponentModel;
using System.Globalization;

namespace Umbraco.Cms.Core.Models.PublishedContent;

internal class PublishedContentTypeConverter : TypeConverter
{
    private static readonly Type[] ConvertingTypes = { typeof(int) };

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
        ConvertingTypes.Any(x => x.IsAssignableFrom(destinationType))
        || (destinationType is not null && CanConvertFrom(context, destinationType));

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
