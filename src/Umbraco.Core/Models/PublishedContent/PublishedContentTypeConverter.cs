using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Umbraco.Core.Models.PublishedContent
{
    internal class PublishedContentTypeConverter : TypeConverter
    {
        private static readonly Type[] ConvertableTypes = new[]
        {
            typeof(int)
        };

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ConvertableTypes.Any(x => TypeHelper.IsTypeAssignableFrom(x, destinationType))
                   || base.CanConvertFrom(context, destinationType);
        }

        public override object ConvertTo(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value,
            Type destinationType)
        {
            var publishedContent = value as IPublishedContent;
            if (publishedContent == null)
                return null;

            if (TypeHelper.IsTypeAssignableFrom<int>(destinationType))
            {
                return publishedContent.Id;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}